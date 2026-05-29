#nullable disable

namespace AgentDashboard.TicketTracking.Infrastructure.Tickets;

using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Tickets;
using Microsoft.Data.Sqlite;

/// <summary>
/// SQLite implementation of the ITicketWriteRepository port.
/// Handles persisting Ticket entities to SQLite with WAL mode for concurrent access.
/// </summary>
internal sealed class SqliteTicketWriteRepository : ITicketWriteRepository
{
    private readonly string _connectionString;
    private readonly string _schemaSql;

    public SqliteTicketWriteRepository(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
        _schemaSql = @"
            CREATE TABLE IF NOT EXISTS tickets (
                repo TEXT NOT NULL,
                github_issue_number INTEGER NOT NULL,
                title TEXT NOT NULL,
                status TEXT NOT NULL,
                agent TEXT,
                retry_count INTEGER NOT NULL,
                github_url TEXT NOT NULL,
                created_at_utc TEXT NOT NULL,
                updated_at_utc TEXT NOT NULL,
                closed_at_utc TEXT,
                PRIMARY KEY (repo, github_issue_number)
            );
            CREATE INDEX IF NOT EXISTS idx_tickets_repo ON tickets(repo);
            CREATE INDEX IF NOT EXISTS idx_tickets_status ON tickets(status);
            CREATE INDEX IF NOT EXISTS idx_tickets_agent ON tickets(agent);
        ";
    }

    public async Task SaveAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        // Enable WAL mode for concurrent read/write
        await using (var cmd = new SqliteCommand("PRAGMA journal_mode=WAL;", connection))
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        // Create schema if not exists
        await using (var cmd = new SqliteCommand(_schemaSql, connection))
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        // Upsert the ticket
        var commandText = @"
            INSERT INTO tickets (
                repo, github_issue_number, title, status, agent, retry_count, github_url, 
                created_at_utc, updated_at_utc, closed_at_utc
            ) VALUES (
                @repo, @github_issue_number, @title, @status, @agent, @retry_count, @github_url,
                @created_at_utc, @updated_at_utc, @closed_at_utc
            ) ON CONFLICT(repo, github_issue_number) DO UPDATE SET
                title = excluded.title,
                status = excluded.status,
                agent = excluded.agent,
                retry_count = excluded.retry_count,
                github_url = excluded.github_url,
                updated_at_utc = excluded.updated_at_utc,
                closed_at_utc = excluded.closed_at_utc;
        ";

        await using var command = new SqliteCommand(commandText, connection);
        command.Parameters.AddWithValue("@repo", ticket.RepositorySource.Value);
        command.Parameters.AddWithValue("@github_issue_number", ticket.GitHubIssueNumber.Value);
        command.Parameters.AddWithValue("@title", ticket.TicketTitle.Value);
        command.Parameters.AddWithValue("@status", ticket.TicketStatus.Value.ToString());
        command.Parameters.AddWithValue("@agent", (object)ticket.AgentId?.Value ?? DBNull.Value);
        command.Parameters.AddWithValue("@retry_count", ticket.RetryCount.Value);
        command.Parameters.AddWithValue("@github_url", ticket.GitHubUrl.Value);
        
        command.Parameters.AddWithValue("@created_at_utc", ticket.CreatedAtUtc.ToString());
        command.Parameters.AddWithValue("@updated_at_utc", ticket.UpdatedAtUtc.ToString());
        var closedAtStr = ticket.ClosedAtUtc?.ToString();
        command.Parameters.AddWithValue("@closed_at_utc", closedAtStr != null ? (object)closedAtStr : DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
