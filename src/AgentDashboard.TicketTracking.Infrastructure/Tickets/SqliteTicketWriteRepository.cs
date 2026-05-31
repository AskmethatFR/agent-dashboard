namespace AgentDashboard.TicketTracking.Infrastructure.Tickets;

using System.Collections.Concurrent;
using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.Infrastructure.Tickets.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// EF Core (code-first) implementation of the <see cref="ITicketWriteRepository"/> port.
/// Schema is bootstrapped idempotently at construction (see ADR-010): a per-data-source lock
/// serializes schema creation across repository instances in the process, tables are created
/// only when absent (<see cref="IRelationalDatabaseCreator.HasTables"/>), and the WAL is
/// checkpointed so the schema is visible in the main database file rather than only the
/// <c>-wal</c> sidecar. A context is created and disposed per <see cref="SaveAsync"/> call so
/// the externally-singleton repository never shares a mutable change tracker across calls.
/// </summary>
public sealed class SqliteTicketWriteRepository : ITicketWriteRepository
{
    private static readonly ConcurrentDictionary<string, object> SchemaLocksByDataSource = new();

    private readonly PooledDbContextFactory<TicketTrackingDbContext> _contextFactory;

    public SqliteTicketWriteRepository(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        var effectiveConnectionString = new SqliteConnectionStringBuilder(connectionString)
        {
            Pooling = false,
        }.ToString();

        EnsureDataDirectoryExists(effectiveConnectionString);

        var options = new DbContextOptionsBuilder<TicketTrackingDbContext>()
            .UseSqlite(effectiveConnectionString)
            .Options;
        _contextFactory = new PooledDbContextFactory<TicketTrackingDbContext>(options);

        InitializeSchema(effectiveConnectionString);
    }

    private void InitializeSchema(string effectiveConnectionString)
    {
        var dataSource = new SqliteConnectionStringBuilder(effectiveConnectionString).DataSource;
        var schemaLock = SchemaLocksByDataSource.GetOrAdd(dataSource, _ => new object());

        lock (schemaLock)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");

            CreateSchemaIfAbsent(context);

            // Warm the EF model and the FindAsync query plan so the first SaveAsync
            // on the hot path pays only I/O cost, not first-use compilation.
            _ = context.Tickets.AsNoTracking().Any();
        }
    }

    private static void CreateSchemaIfAbsent(TicketTrackingDbContext context)
    {
        var creator = context.Database.GetService<IRelationalDatabaseCreator>();

        if (!creator.Exists())
        {
            creator.Create();
        }

        if (!creator.HasTables())
        {
            creator.CreateTables();
        }

        CheckpointWal(context);
    }

    private static void CheckpointWal(TicketTrackingDbContext context)
        => context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint(TRUNCATE);");

    public async Task SaveAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        var row = TicketRow.FromTicket(ticket);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        var existing = await context.Tickets
            .FindAsync([row.GitHubIssueNumber], cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            context.Tickets.Add(row);
        }
        else
        {
            existing.Title = row.Title;
            existing.Status = row.Status;
            existing.Agent = row.Agent;
            existing.RetryCount = row.RetryCount;
            existing.GitHubUrl = row.GitHubUrl;
            existing.UpdatedAtUtc = row.UpdatedAtUtc;
            existing.ClosedAtUtc = row.ClosedAtUtc;
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void EnsureDataDirectoryExists(string connectionString)
    {
        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (string.IsNullOrEmpty(dataSource))
        {
            return;
        }

        var directory = System.IO.Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
    }
}
