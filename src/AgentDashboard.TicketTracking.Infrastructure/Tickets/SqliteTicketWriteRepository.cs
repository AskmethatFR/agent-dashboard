
namespace AgentDashboard.TicketTracking.Infrastructure.Tickets;

using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Tickets;
using Microsoft.Data.Sqlite;

/// <summary>
/// SQLite implementation of the ITicketWriteRepository port.
/// Handles persisting Ticket entities to SQLite with WAL mode for concurrent access.
/// </summary>
public sealed class SqliteTicketWriteRepository : ITicketWriteRepository
{
    private readonly string _connectionString;

    public SqliteTicketWriteRepository(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
        InitializeSchemaAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeSchemaAsync()
    {
        // Ensure the directory exists
        var connectionStringBuilder = new SqliteConnectionStringBuilder(_connectionString);
        var dataSource = connectionStringBuilder.DataSource;
        if (!string.IsNullOrEmpty(dataSource))
        {
            var directory = System.IO.Path.GetDirectoryName(dataSource);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
        }

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        // Enable WAL mode for concurrent read/write
        await using (var cmd = new SqliteCommand("PRAGMA journal_mode=WAL;", connection))
        {
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Create schema if not exists
        const string schemaSql = @"
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
        await using (var cmd = new SqliteCommand(schemaSql, connection))
        {
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    public async Task SaveAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

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
        var agentValue = ticket.AgentId?.Value;
        var createdAtStr = ticket.CreatedAtUtc.ToString();
        var updatedAtStr = ticket.UpdatedAtUtc.ToString();
        var closedAtStr = ticket.ClosedAtUtc?.ToString();
        
        command.Parameters.AddWithValue("@repo", ticket.GitHubRepository.Value);
        command.Parameters.AddWithValue("@github_issue_number", ticket.GitHubIssueNumber.Value);
        command.Parameters.AddWithValue("@title", ticket.TicketTitle.Value);
        command.Parameters.AddWithValue("@status", ticket.TicketStatus.Value.ToString());
        command.Parameters.AddWithValue("@agent", agentValue != null ? (object)agentValue : DBNull.Value);
        command.Parameters.AddWithValue("@retry_count", ticket.RetryCount.Value);
        command.Parameters.AddWithValue("@github_url", ticket.GitHubUrl.Value);
        
        command.Parameters.AddWithValue("@created_at_utc", (object)createdAtStr);
        command.Parameters.AddWithValue("@updated_at_utc", (object)updatedAtStr);
        command.Parameters.AddWithValue("@closed_at_utc", closedAtStr != null ? (object)closedAtStr : DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
