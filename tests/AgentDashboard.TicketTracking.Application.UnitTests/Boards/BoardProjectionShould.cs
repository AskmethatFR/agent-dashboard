// TEST LIST (TDD by Example — Kent Beck's Test List technique)
// ========================================================================
//
// STATUS → COLUMN:
// [✓] 1. Project_WithStatusLabel_PlacesTicketInCorrectColumn (Theory over all 7 valid statuses)
//
// STATUS FALLBACK:
// [✓] 2. Project_WithStatusEscalated_FallsBackToCreatedColumn
// [✓] 3. Project_WithNoStatusLabel_FallsBackToCreatedColumn
//
// AGENT → AGENT ID:
// [✓] 4. Project_WithAgentLabel_MapsAllSixAgents (Theory)
//
// AGENT FALLBACK:
// [✓] 5. Project_WithNoAgentLabel_DefaultsToPm
//
// RETRY MAPPING:
// [✓] 6a. Project_WithRetryLabel_MapsRetryValue (Theory 0/1/2/3)
// [✓] 6b. Project_WithTwoRetryLabels_TakesFirstOne
// [✓] 7. Project_WithNoRetryLabel_DefaultsToZero
//
// CROSS-REVIEW:
// [✓] 8. Project_InReview_WithoutCoAgent_IsInCrossReviewTrueAndCoAgentDefaultsToAgent
// [✓] 9. Project_InReview_WithExplicitCoAgent_UsesExplicitCoAgent
//
// ESCALATION:
// [✓] 10. Project_Escalated_WithRetry3_AndEscalationTarget_IsEscalatedTrue
// [✓] 11. Project_Escalated_WithRetry2_IsEscalatedFalse  (boundary: retry < 3)
// [✓] 12. Project_Escalated_WithRetry3_WithoutEscalationTarget_TargetDefaultsToAgentId
// [✓] 13. Project_InReview_AndEscalated_WithRetry3_BothFlagsTrue
//
// FRESHNESS:
// [✓] 14. Project_DoneTicket_WithAgeLessThan24h_IsFresh
// [✓] 15. Project_AnyTicket_WithAgeAtWarningThreshold_IsStale
// [✓] 16. Project_AnyTicket_WithAgeBelowWarningThreshold_IsNeutral
// [✓] 17. Project_DoneTicket_WithAgeOver24h_IsStale  (boundary: >24h not fresh)
// [✓] 18. Project_AgeComputedFromCreatedAt
//
// THINKING FLAG:
// [✓] 19. Project_AnyTicket_IsThinkingFalse
//
// STRUCTURAL:
// [✓] 20. Project_EmptyRecords_ReturnsSevenColumnsAndSixAgentsAndZeroTickets
// [✓] 21. Project_ThreeRecords_ReturnsThreeUniqueTickets
// [✓] 23. Project_ColumnAndAgentIds_MatchExpectedSets
//
// VALIDATION:
// [✓] 22. Project_WithMalformedLabel_ThrowsInvalidOperationException (Theory)
//
// ========================================================================

using AgentDashboard.TicketTracking.Application.Boards;
using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.TestShared.Factories;

namespace AgentDashboard.TicketTracking.Application.UnitTests.Boards;

public sealed class BoardProjectionShould
{
    private static readonly DateTimeOffset AsOf = new(2026, 05, 27, 12, 0, 0, TimeSpan.Zero);
    private static readonly string AnyUrl = "https://github.com/AskmethatFR/agent-dashboard/issues/1";

    private readonly BoardProjection _sut = new();

    // -------------------------------------------------------------------------
    // 1. Status → Column (all 7 valid statuses)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("status:created", "CREATED")]
    [InlineData("status:specified", "SPECIFIED")]
    [InlineData("status:in-development", "IN_DEVELOPMENT")]
    [InlineData("status:in-review", "IN_REVIEW")]
    [InlineData("status:in-qa", "IN_QA")]
    [InlineData("status:awaiting-validation", "AWAITING_VALIDATION")]
    [InlineData("status:done", "DONE")]
    public void Project_WithStatusLabel_PlacesTicketInCorrectColumn(string statusLabel, string expectedColumnId)
    {
        var record = BuildRecord(1, statusLabel);

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].ColumnId.Value.Should().Be(expectedColumnId);
    }

    // -------------------------------------------------------------------------
    // 2. Escalated → CREATED fallback
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithStatusEscalated_FallsBackToCreatedColumn()
    {
        var record = BuildRecord(1, "status:escalated");

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].ColumnId.Value.Should().Be("CREATED");
    }

    // -------------------------------------------------------------------------
    // 3. No status → CREATED fallback
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithNoStatusLabel_FallsBackToCreatedColumn()
    {
        var record = BuildRecordNoLabels(1);

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].ColumnId.Value.Should().Be("CREATED");
    }

    // -------------------------------------------------------------------------
    // 4. Agent → agentId (all 6)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("agent:pm", "pm")]
    [InlineData("agent:architect", "architect")]
    [InlineData("agent:dev-a", "dev-a")]
    [InlineData("agent:dev-b", "dev-b")]
    [InlineData("agent:qa", "qa")]
    [InlineData("agent:security", "security")]
    public void Project_WithAgentLabel_MapsAllSixAgents(string agentLabel, string expectedAgentId)
    {
        var record = BuildRecord(1, agentLabel);

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].AgentId.Value.Should().Be(expectedAgentId);
    }

    // -------------------------------------------------------------------------
    // 5. No agent → pm fallback
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithNoAgentLabel_DefaultsToPm()
    {
        var record = BuildRecord(1, "status:created");

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].AgentId.Value.Should().Be("pm");
    }

    // -------------------------------------------------------------------------
    // 6a. Retry 0/1/2/3
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("retry:0", 0)]
    [InlineData("retry:1", 1)]
    [InlineData("retry:2", 2)]
    [InlineData("retry:3", 3)]
    public void Project_WithRetryLabel_MapsRetryValue(string retryLabel, int expected)
    {
        var record = BuildRecord(1, retryLabel);

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].Retry.Value.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // 6b. Two retry labels → first wins
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithTwoRetryLabels_TakesFirstOne()
    {
        var record = BuildRecordWithLabels(1, "retry:1", "retry:3");

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].Retry.Value.Should().Be(1);
    }

    // -------------------------------------------------------------------------
    // 7. No retry → 0
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithNoRetryLabel_DefaultsToZero()
    {
        var record = BuildRecordNoLabels(1);

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].Retry.Value.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // 8. in-review + agent, no co-agent → IsInCrossReview true AND coAgent defaults to agent
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_InReview_WithoutCoAgent_IsInCrossReviewTrueAndCoAgentDefaultsToAgent()
    {
        var record = BuildRecordWithLabels(1, "status:in-review", "agent:dev-a");

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        var ticket = snapshot.Tickets[0];
        ticket.IsInCrossReview.Should().BeTrue();
        ticket.CoAgentId!.Value.Should().Be("dev-a");
    }

    // -------------------------------------------------------------------------
    // 9. in-review + explicit co-agent → uses that co-agent
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_InReview_WithExplicitCoAgent_UsesExplicitCoAgent()
    {
        var record = BuildRecordWithLabels(1, "status:in-review", "agent:dev-a", "co-agent:dev-b");

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].CoAgentId!.Value.Should().Be("dev-b");
    }

    // -------------------------------------------------------------------------
    // 10. escalated + retry:3 + escalation-target → IsEscalated true
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_Escalated_WithRetry3_AndEscalationTarget_IsEscalatedTrue()
    {
        var record = BuildRecordWithLabels(1, "status:escalated", "agent:dev-a", "retry:3", "escalation-target:pm");

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].IsEscalated.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // 11. escalated + retry:2 → NOT escalated (boundary: retry < 3)
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_Escalated_WithRetry2_IsEscalatedFalse()
    {
        var record = BuildRecordWithLabels(1, "status:escalated", "agent:dev-a", "retry:2");

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].IsEscalated.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // 12. escalated + retry:3 without escalation-target → target defaults to agentId
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_Escalated_WithRetry3_WithoutEscalationTarget_TargetDefaultsToAgentId()
    {
        var record = BuildRecordWithLabels(1, "status:escalated", "agent:dev-a", "retry:3");

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        var ticket = snapshot.Tickets[0];
        ticket.IsEscalated.Should().BeTrue();
        ticket.EscalationTarget!.Value.Should().Be("dev-a");
    }

    // -------------------------------------------------------------------------
    // 13. in-review + escalated + retry:3 → both flags true
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_InReview_AndEscalated_WithRetry3_BothFlagsTrue()
    {
        var record = BuildRecordWithLabels(1, "status:in-review", "status:escalated", "agent:dev-a", "retry:3", "escalation-target:pm");

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        var ticket = snapshot.Tickets[0];
        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // 14. Done + age<24h → Fresh
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_DoneTicket_WithAgeLessThan24h_IsFresh()
    {
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Done ticket")
            .WithLabels("status:done")
            .WithCreatedAt(AsOf.AddHours(-12))
            .WithHtmlUrl(AnyUrl)
            .WithUpdatedAt(AsOf.AddHours(-12))
            .AsOpen()
            .Build();

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].Freshness.Should().Be(TicketFreshness.Fresh);
    }

    // -------------------------------------------------------------------------
    // 15. age >= WarningThreshold → Stale
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_AnyTicket_WithAgeAtWarningThreshold_IsStale()
    {
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Stale ticket")
            .WithLabels(Array.Empty<string>())
            .WithCreatedAt(AsOf.Subtract(Age.WarningThreshold))
            .WithHtmlUrl(AnyUrl)
            .WithUpdatedAt(AsOf.Subtract(Age.WarningThreshold))
            .AsOpen()
            .Build();

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].Freshness.Should().Be(TicketFreshness.Stale);
    }

    // -------------------------------------------------------------------------
    // 16. age below threshold → Neutral
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_AnyTicket_WithAgeBelowWarningThreshold_IsNeutral()
    {
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Neutral ticket")
            .WithLabels(Array.Empty<string>())
            .WithCreatedAt(AsOf.AddMinutes(-30))
            .WithHtmlUrl(AnyUrl)
            .WithUpdatedAt(AsOf.AddMinutes(-30))
            .AsOpen()
            .Build();

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].Freshness.Should().Be(TicketFreshness.Neutral);
    }

    // -------------------------------------------------------------------------
    // 17. Done + age>24h → Stale (the <24h boundary)
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_DoneTicket_WithAgeOver24h_IsStale()
    {
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Old done ticket")
            .WithLabels("status:done")
            .WithCreatedAt(AsOf.AddDays(-2))
            .WithHtmlUrl(AnyUrl)
            .WithUpdatedAt(AsOf.AddDays(-2))
            .AsOpen()
            .Build();

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].Freshness.Should().Be(TicketFreshness.Stale);
    }

    // -------------------------------------------------------------------------
    // 18. Age computed from CreatedAt
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_AgeComputedFromCreatedAt()
    {
        var createdAt = AsOf.AddHours(-5);
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Age test")
            .WithLabels(Array.Empty<string>())
            .WithCreatedAt(createdAt)
            .WithHtmlUrl(AnyUrl)
            .WithUpdatedAt(createdAt)
            .AsOpen()
            .Build();

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].Age.Value.Should().Be(TimeSpan.FromHours(5));
    }

    // -------------------------------------------------------------------------
    // 19. IsThinking false for all
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_AnyTicket_IsThinkingFalse()
    {
        var record = BuildRecordNoLabels(1);

        var snapshot = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        snapshot.Tickets[0].IsThinking.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // 20. Empty records → 7 columns, 6 agents, 0 tickets
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_EmptyRecords_ReturnsSevenColumnsAndSixAgentsAndZeroTickets()
    {
        var snapshot = _sut.Project(new List<GitHubIssueRecord>(), AsOf);

        snapshot.Columns.Should().HaveCount(7);
        snapshot.Agents.Should().HaveCount(6);
        snapshot.Tickets.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // 21. 3 mixed records → 3 tickets with unique ids
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_ThreeRecords_ReturnsThreeUniqueTickets()
    {
        var records = new List<GitHubIssueRecord>
        {
            BuildRecordWithLabels(1, "status:created", "agent:pm"),
            BuildRecordWithLabels(2, "status:in-review", "agent:dev-a"),
            BuildRecordWithLabels(3, "status:done", "agent:dev-b")
        };

        var snapshot = _sut.Project(records, AsOf);

        snapshot.Tickets.Should().HaveCount(3);
        snapshot.Tickets.Select(t => t.Id.Value).Should().OnlyHaveUniqueItems();
    }

    // -------------------------------------------------------------------------
    // 22. ValidateLabels throws InvalidOperationException on malformed label
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("status:not-a-valid-status")]
    [InlineData("agent:not-a-valid-agent")]
    [InlineData("retry:not-an-int")]
    public void Project_WithMalformedLabel_ThrowsInvalidOperationException(string malformedLabel)
    {
        var record = BuildRecord(1, malformedLabel);

        var act = () => _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        act.Should().Throw<InvalidOperationException>();
    }

    // -------------------------------------------------------------------------
    // 23. Structural: exactly 7 column IDs + 6 agent IDs
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_ColumnAndAgentIds_MatchExpectedSets()
    {
        var snapshot = _sut.Project(new List<GitHubIssueRecord>(), AsOf);

        snapshot.Columns.Select(c => c.Id.Value).Should().BeEquivalentTo(
            new List<string> { "CREATED", "SPECIFIED", "IN_DEVELOPMENT", "IN_REVIEW", "IN_QA", "AWAITING_VALIDATION", "DONE" });

        snapshot.Agents.Select(a => a.Id.Value).Should().BeEquivalentTo(
            new List<string> { "pm", "architect", "dev-a", "dev-b", "qa", "security" });
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static GitHubIssueRecord BuildRecord(long number, string label)
        => new GitHubIssueRecordBuilder()
            .WithNumber(number)
            .WithTitle($"Issue #{number}")
            .WithLabels(label)
            .WithCreatedAt(AsOf)
            .WithHtmlUrl($"https://github.com/AskmethatFR/agent-dashboard/issues/{number}")
            .WithUpdatedAt(AsOf)
            .AsOpen()
            .Build();

    private static GitHubIssueRecord BuildRecordNoLabels(long number)
        => new GitHubIssueRecordBuilder()
            .WithNumber(number)
            .WithTitle($"Issue #{number}")
            .WithLabels(Array.Empty<string>())
            .WithCreatedAt(AsOf)
            .WithHtmlUrl($"https://github.com/AskmethatFR/agent-dashboard/issues/{number}")
            .WithUpdatedAt(AsOf)
            .AsOpen()
            .Build();

    private static GitHubIssueRecord BuildRecordWithLabels(long number, params string[] labels)
        => new GitHubIssueRecordBuilder()
            .WithNumber(number)
            .WithTitle($"Issue #{number}")
            .WithLabels(labels)
            .WithCreatedAt(AsOf)
            .WithHtmlUrl($"https://github.com/AskmethatFR/agent-dashboard/issues/{number}")
            .WithUpdatedAt(AsOf)
            .AsOpen()
            .Build();
}
