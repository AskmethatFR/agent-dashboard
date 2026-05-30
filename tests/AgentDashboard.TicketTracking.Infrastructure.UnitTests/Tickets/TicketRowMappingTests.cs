using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.Infrastructure.Tickets.Persistence;

namespace AgentDashboard.TicketTracking.Infrastructure.UnitTests.Tickets;

// Test List for TicketRow self-mapping (Infrastructure boundary, Ticket ⇄ TicketRow):
// ========================================================================
// Relocated from the deleted Domain TicketPersistenceSnapshotShould — the
// Domain↔persistence contract now lives on the Infra TicketRow itself.
//
// ROUND-TRIP (ToTicket(FromTicket(t)) == t, field-by-field):
//  [x] full ticket (every field populated) round-trips
//  [x] null Agent round-trips to null
//  [x] null ClosedAtUtc round-trips to null
//  [x] every TicketStatusValue round-trips (enum-name ⇄ Parse)
//
// FIELD FLATTENING (FromTicket flattens VO → primitive, exact column shape):
//  [x] FromTicket maps each VO to its primitive (repo, number, title, status name,
//      agent, retry, url, created/updated/closed as "o" ISO-8601)
//
// TIMESTAMP BYTE-COMPATIBILITY (must match on-disk format = DateTimeOffset.ToString("o")):
//  [x] CreatedAtUtc row string equals VO.ToString() ("o" round-trip format)
//
// CORRUPTED-ROW GUARDING (ToTicket() wraps raw parse failures, hides raw value):
//  [x] invalid Status string → CorruptedTicketRowException naming "status" + row key,
//      NOT echoing the raw bad value, inner = original ArgumentException
//  [x] malformed CreatedAtUtc → CorruptedTicketRowException naming "created_at_utc"
// ========================================================================

public sealed class TicketRowMappingTests
{
    [Fact]
    public void RoundTrip_AllFieldsPopulated()
    {
        var original = FullTicket();

        var restored = TicketRow.FromTicket(original).ToTicket();

        restored.GitHubRepository.Value.Should().Be(original.GitHubRepository.Value);
        restored.GitHubIssueNumber.Value.Should().Be(original.GitHubIssueNumber.Value);
        restored.TicketTitle.Value.Should().Be(original.TicketTitle.Value);
        restored.TicketStatus.Value.Should().Be(original.TicketStatus.Value);
        restored.AgentId!.Value.Should().Be(original.AgentId!.Value);
        restored.RetryCount.Value.Should().Be(original.RetryCount.Value);
        restored.GitHubUrl.Value.Should().Be(original.GitHubUrl.Value);
        restored.CreatedAtUtc.Value.Should().Be(original.CreatedAtUtc.Value);
        restored.UpdatedAtUtc.Value.Should().Be(original.UpdatedAtUtc.Value);
        restored.ClosedAtUtc!.Value.Should().Be(original.ClosedAtUtc!.Value);
    }

    [Fact]
    public void RoundTrip_NullAgent()
    {
        var original = new Ticket(
            new GitHubRepository("AskmethatFR/agent-dashboard"),
            new GitHubIssueNumber(7),
            new TicketTitle("No agent"),
            TicketStatusValue.Created,
            null,
            new Retry(0),
            new GitHubUrl("https://github.com/AskmethatFR/agent-dashboard/issues/7"),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            null);

        var row = TicketRow.FromTicket(original);
        var restored = row.ToTicket();

        row.Agent.Should().BeNull();
        restored.AgentId.Should().BeNull();
    }

    [Fact]
    public void RoundTrip_NullClosedAtUtc()
    {
        var full = FullTicket();

        var open = new Ticket(
            full.GitHubRepository,
            full.GitHubIssueNumber,
            full.TicketTitle,
            full.TicketStatus,
            full.AgentId,
            full.RetryCount,
            full.GitHubUrl,
            full.CreatedAtUtc,
            full.UpdatedAtUtc,
            null);

        var row = TicketRow.FromTicket(open);
        var restored = row.ToTicket();

        row.ClosedAtUtc.Should().BeNull();
        restored.ClosedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(TicketStatusValue.Created)]
    [InlineData(TicketStatusValue.Specified)]
    [InlineData(TicketStatusValue.InDevelopment)]
    [InlineData(TicketStatusValue.InReview)]
    [InlineData(TicketStatusValue.InQa)]
    [InlineData(TicketStatusValue.AwaitingValidation)]
    [InlineData(TicketStatusValue.Done)]
    [InlineData(TicketStatusValue.Escalated)]
    public void RoundTrip_EveryStatusValue(TicketStatusValue status)
    {
        var original = new Ticket(
            new GitHubRepository("AskmethatFR/agent-dashboard"),
            new GitHubIssueNumber(11),
            new TicketTitle("Status round-trip"),
            new TicketStatus(status),
            new AgentId("pm"),
            new Retry(0),
            new GitHubUrl("https://github.com/AskmethatFR/agent-dashboard/issues/11"),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            null);

        var row = TicketRow.FromTicket(original);
        var restored = row.ToTicket();

        row.Status.Should().Be(status.ToString());
        restored.TicketStatus.Value.Should().Be(status);
    }

    [Fact]
    public void FromTicket_FlattensValueObjectsToPrimitives()
    {
        var ticket = FullTicket();

        var row = TicketRow.FromTicket(ticket);

        row.Repo.Should().Be("AskmethatFR/agent-dashboard");
        row.GitHubIssueNumber.Should().Be(42);
        row.Title.Should().Be("Full ticket");
        row.Status.Should().Be(nameof(TicketStatusValue.InReview));
        row.Agent.Should().Be("dev-a");
        row.RetryCount.Should().Be(2);
        row.GitHubUrl.Should().Be("https://github.com/AskmethatFR/agent-dashboard/issues/42");
    }

    [Fact]
    public void FromTicket_SerializesTimestampsInRoundTripFormat()
    {
        var ticket = FullTicket();

        var row = TicketRow.FromTicket(ticket);

        row.CreatedAtUtc.Should().Be(ticket.CreatedAtUtc.ToString());
        row.UpdatedAtUtc.Should().Be(ticket.UpdatedAtUtc.ToString());
        row.ClosedAtUtc.Should().Be(ticket.ClosedAtUtc!.ToString());
    }

    [Fact]
    public void ToTicket_InvalidStatus_ThrowsCorruptedRowNamingStatusColumn()
    {
        var corrupted = WellFormedRow();
        corrupted.Status = "not-a-status";

        var act = () => corrupted.ToTicket();

        var thrown = act.Should().Throw<CorruptedTicketRowException>().Which;
        thrown.Message.Should().Contain("status");
        thrown.Message.Should().Contain("AskmethatFR/agent-dashboard");
        thrown.Message.Should().Contain("42");
        thrown.Message.Should().NotContain("not-a-status");
        thrown.InnerException.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public void ToTicket_MalformedCreatedAtUtc_ThrowsCorruptedRowNamingCreatedAtColumn()
    {
        var corrupted = WellFormedRow();
        corrupted.CreatedAtUtc = "garbage";

        var act = () => corrupted.ToTicket();

        var thrown = act.Should().Throw<CorruptedTicketRowException>().Which;
        thrown.Message.Should().Contain("created_at_utc");
        thrown.Message.Should().Contain("AskmethatFR/agent-dashboard");
        thrown.Message.Should().Contain("42");
        thrown.Message.Should().NotContain("garbage");
        thrown.InnerException.Should().BeOfType<FormatException>();
    }

    private static TicketRow WellFormedRow() => TicketRow.FromTicket(FullTicket());

    private static Ticket FullTicket()
    {
        var created = new DateTimeOffset(2026, 5, 22, 9, 0, 0, TimeSpan.Zero);
        return new Ticket(
            new GitHubRepository("AskmethatFR/agent-dashboard"),
            new GitHubIssueNumber(42),
            new TicketTitle("Full ticket"),
            new TicketStatus(TicketStatusValue.InReview),
            new AgentId("dev-a"),
            new Retry(2),
            new GitHubUrl("https://github.com/AskmethatFR/agent-dashboard/issues/42"),
            created,
            created.AddHours(1),
            created.AddHours(2));
    }
}
