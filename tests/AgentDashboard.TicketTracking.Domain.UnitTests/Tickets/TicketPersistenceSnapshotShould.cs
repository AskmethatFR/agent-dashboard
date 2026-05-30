namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

using System;
using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Tickets;
using FluentAssertions;
using Xunit;

// Test List for TicketPersistenceSnapshot (Snapshot pattern, Domain↔persistence boundary):
// ========================================================================
// ROUND-TRIP (FromSnapshot(ToSnapshot(t)) == t, field-by-field):
//  [x] full ticket (every field populated) round-trips
//  [x] null Agent round-trips to null
//  [x] null ClosedAtUtc round-trips to null
//  [x] every TicketStatusValue round-trips (enum-name ⇄ Parse)
//
// FIELD MAPPING (ToSnapshot flattens VO → primitive, exact column shape):
//  [x] ToSnapshot maps each VO to its primitive (repo, number, title, status name,
//      agent, retry, url, created/updated/closed as "o" ISO-8601)
//
// TIMESTAMP BYTE-COMPATIBILITY (must match old on-disk format = DateTimeOffset.ToString("o")):
//  [x] CreatedAtUtc snapshot string equals VO.ToString() ("o" round-trip format)
// ========================================================================

public sealed class TicketPersistenceSnapshotShould
{
    [Fact]
    public void RoundTrip_AllFieldsPopulated()
    {
        var original = FullTicket();

        var restored = Ticket.FromSnapshot(original.ToSnapshot());

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

        var snapshot = original.ToSnapshot();
        var restored = Ticket.FromSnapshot(snapshot);

        snapshot.Agent.Should().BeNull();
        restored.AgentId.Should().BeNull();
    }

    [Fact]
    public void RoundTrip_NullClosedAtUtc()
    {
        var original = FullTicket();

        var open = new Ticket(
            original.GitHubRepository,
            original.GitHubIssueNumber,
            original.TicketTitle,
            original.TicketStatus,
            original.AgentId,
            original.RetryCount,
            original.GitHubUrl,
            original.CreatedAtUtc,
            original.UpdatedAtUtc,
            null);

        var snapshot = open.ToSnapshot();
        var restored = Ticket.FromSnapshot(snapshot);

        snapshot.ClosedAtUtc.Should().BeNull();
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

        var snapshot = original.ToSnapshot();
        var restored = Ticket.FromSnapshot(snapshot);

        snapshot.Status.Should().Be(status.ToString());
        restored.TicketStatus.Value.Should().Be(status);
    }

    [Fact]
    public void ToSnapshot_FlattensValueObjectsToPrimitives()
    {
        var ticket = FullTicket();

        var snapshot = ticket.ToSnapshot();

        snapshot.Repo.Should().Be("AskmethatFR/agent-dashboard");
        snapshot.GitHubIssueNumber.Should().Be(42);
        snapshot.Title.Should().Be("Full ticket");
        snapshot.Status.Should().Be(nameof(TicketStatusValue.InReview));
        snapshot.Agent.Should().Be("dev-a");
        snapshot.RetryCount.Should().Be(2);
        snapshot.GitHubUrl.Should().Be("https://github.com/AskmethatFR/agent-dashboard/issues/42");
    }

    [Fact]
    public void ToSnapshot_SerializesTimestampsInRoundTripFormat()
    {
        var ticket = FullTicket();

        var snapshot = ticket.ToSnapshot();

        snapshot.CreatedAtUtc.Should().Be(ticket.CreatedAtUtc.ToString());
        snapshot.UpdatedAtUtc.Should().Be(ticket.UpdatedAtUtc.ToString());
        snapshot.ClosedAtUtc.Should().Be(ticket.ClosedAtUtc!.ToString());
    }

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
