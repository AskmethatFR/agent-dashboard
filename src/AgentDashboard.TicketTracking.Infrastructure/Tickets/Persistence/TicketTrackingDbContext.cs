namespace AgentDashboard.TicketTracking.Infrastructure.Tickets.Persistence;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// EF Core code-first context for the TicketTracking write side.
/// Maps the <see cref="TicketRow"/> POCO onto the <c>tickets</c> table via the
/// Fluent API, preserving the verbatim column shape (snake_case names,
/// composite PK, indexes) the read side and tests depend on.
/// </summary>
internal sealed class TicketTrackingDbContext : DbContext
{
    public TicketTrackingDbContext(DbContextOptions<TicketTrackingDbContext> options)
        : base(options)
    {
    }

    public DbSet<TicketRow> Tickets => Set<TicketRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var ticket = modelBuilder.Entity<TicketRow>();
        ticket.ToTable("tickets");
        ticket.HasKey(r => new { r.Repo, r.GitHubIssueNumber });

        ticket.Property(r => r.Repo).HasColumnName("repo").IsRequired();
        ticket.Property(r => r.GitHubIssueNumber).HasColumnName("github_issue_number").IsRequired();
        ticket.Property(r => r.Title).HasColumnName("title").IsRequired();
        ticket.Property(r => r.Status).HasColumnName("status").IsRequired();
        ticket.Property(r => r.Agent).HasColumnName("agent");
        ticket.Property(r => r.RetryCount).HasColumnName("retry_count").IsRequired();
        ticket.Property(r => r.GitHubUrl).HasColumnName("github_url").IsRequired();
        ticket.Property(r => r.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        ticket.Property(r => r.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        ticket.Property(r => r.ClosedAtUtc).HasColumnName("closed_at_utc");

        ticket.HasIndex(r => r.Repo);
        ticket.HasIndex(r => r.Status);
        ticket.HasIndex(r => r.Agent);
    }
}
