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
// DEGRADATION (ADR-014 — security-driven, A04):
// [✓] 22a. Project_WithMalformedLabel_DoesNotThrow (Theory)
// [✓] 22b. Project_WithMalformedLabel_EmitsMalformedLabelWarning (Theory)
// [✓] 22c. Project_WithMalformedStatus_FallsBackToCreatedAndRenders
// [✓] 22d. Project_WithMalformedAgent_FallsBackToPmAndRenders
// [✓] 22e. Project_WithMalformedRetry_FallsBackToZeroAndRenders
// [✓] 22f. Project_OneBadAmongMany_RendersValidTicketsAndWarnsOnlyForBad (THE security test)
// [✓] 22g. Project_AllValid_EmitsNoWarnings
// [✓] 22h. Project_MultipleBadLabelsOnOneRecord_EmitsOneWarningPerBadLabel
// [✓] 22i. Project_WithUnknownNonTeamPrefix_RendersWithNoWarning (Theory)
//
// DEGRADATION — OUT-OF-RANGE RETRY (B1 fix — security A04):
// [✓] 22j. Project_WithOutOfRangeRetryLabel_DoesNotThrow (Theory: retry:4, retry:999, retry:-1)
// [✓] 22k. Project_WithOutOfRangeRetryLabel_EmitsMalformedLabelWarning (Theory: retry:4, retry:999)
// [✓] 22l. Project_WithOutOfRangeRetry_FallsBackToZeroAndRenders
//
// B2 — UNTESTED BRANCHES (co-agent and escalation-target invalid arms):
// [✓] 22m. Project_InReview_WithInvalidCoAgent_FallsBackAndWarns
// [✓] 22n. Project_Escalated_WithInvalidEscalationTarget_FallsBackAndWarns
//
// B3 — PASS-THROUGH (bare word, no colon):
// [✓] 22o. Project_WithBareWordLabel_RendersWithNoWarning
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

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].ColumnId.Value.Should().Be(expectedColumnId);
    }

    // -------------------------------------------------------------------------
    // 2. Escalated → CREATED fallback
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithStatusEscalated_FallsBackToCreatedColumn()
    {
        var record = BuildRecord(1, "status:escalated");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].ColumnId.Value.Should().Be("CREATED");
    }

    // -------------------------------------------------------------------------
    // 3. No status → CREATED fallback
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithNoStatusLabel_FallsBackToCreatedColumn()
    {
        var record = BuildRecordNoLabels(1);

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].ColumnId.Value.Should().Be("CREATED");
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

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].AgentId.Value.Should().Be(expectedAgentId);
    }

    // -------------------------------------------------------------------------
    // 5. No agent → pm fallback
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithNoAgentLabel_DefaultsToPm()
    {
        var record = BuildRecord(1, "status:created");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].AgentId.Value.Should().Be("pm");
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

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].Retry.Value.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // 6b. Two retry labels → first wins
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithTwoRetryLabels_TakesFirstOne()
    {
        var record = BuildRecordWithLabels(1, "retry:1", "retry:3");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].Retry.Value.Should().Be(1);
    }

    // -------------------------------------------------------------------------
    // 7. No retry → 0
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithNoRetryLabel_DefaultsToZero()
    {
        var record = BuildRecordNoLabels(1);

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].Retry.Value.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // 8. in-review + agent, no co-agent → IsInCrossReview true AND coAgent defaults to agent
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_InReview_WithoutCoAgent_IsInCrossReviewTrueAndCoAgentDefaultsToAgent()
    {
        var record = BuildRecordWithLabels(1, "status:in-review", "agent:dev-a");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        var ticket = result.Snapshot.Tickets[0];
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

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].CoAgentId!.Value.Should().Be("dev-b");
    }

    // -------------------------------------------------------------------------
    // 10. escalated + retry:3 + escalation-target → IsEscalated true
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_Escalated_WithRetry3_AndEscalationTarget_IsEscalatedTrue()
    {
        var record = BuildRecordWithLabels(1, "status:escalated", "agent:dev-a", "retry:3", "escalation-target:pm");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].IsEscalated.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // 11. escalated + retry:2 → NOT escalated (boundary: retry < 3)
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_Escalated_WithRetry2_IsEscalatedFalse()
    {
        var record = BuildRecordWithLabels(1, "status:escalated", "agent:dev-a", "retry:2");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].IsEscalated.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // 12. escalated + retry:3 without escalation-target → target defaults to agentId
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_Escalated_WithRetry3_WithoutEscalationTarget_TargetDefaultsToAgentId()
    {
        var record = BuildRecordWithLabels(1, "status:escalated", "agent:dev-a", "retry:3");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        var ticket = result.Snapshot.Tickets[0];
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

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        var ticket = result.Snapshot.Tickets[0];
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

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].Freshness.Should().Be(TicketFreshness.Fresh);
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

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].Freshness.Should().Be(TicketFreshness.Stale);
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

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].Freshness.Should().Be(TicketFreshness.Neutral);
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

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].Freshness.Should().Be(TicketFreshness.Stale);
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

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].Age.Value.Should().Be(TimeSpan.FromHours(5));
    }

    // -------------------------------------------------------------------------
    // 19. IsThinking false for all
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_AnyTicket_IsThinkingFalse()
    {
        var record = BuildRecordNoLabels(1);

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].IsThinking.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // 20. Empty records → 7 columns, 6 agents, 0 tickets
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_EmptyRecords_ReturnsSevenColumnsAndSixAgentsAndZeroTickets()
    {
        var result = _sut.Project(new List<GitHubIssueRecord>(), AsOf);

        result.Snapshot.Columns.Should().HaveCount(7);
        result.Snapshot.Agents.Should().HaveCount(6);
        result.Snapshot.Tickets.Should().BeEmpty();
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

        var result = _sut.Project(records, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(3);
        result.Snapshot.Tickets.Select(t => t.Id.Value).Should().OnlyHaveUniqueItems();
    }

    // -------------------------------------------------------------------------
    // 22a. Degradation: malformed label → does NOT throw (replaces old throw-test)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("status:foo")]
    [InlineData("agent:bob")]
    [InlineData("retry:xyz")]
    [InlineData("status:")]
    [InlineData("foo:")]
    [InlineData(":bar")]
    public void Project_WithMalformedLabel_DoesNotThrow(string malformedLabel)
    {
        var record = BuildRecord(1, malformedLabel);

        var act = () => _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        act.Should().NotThrow();
    }

    // -------------------------------------------------------------------------
    // 22b. Malformed label → exactly one ProjectionWarning (MalformedLabel)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("status:foo")]
    [InlineData("agent:bob")]
    [InlineData("retry:xyz")]
    [InlineData("status:")]
    [InlineData("foo:")]
    [InlineData(":bar")]
    public void Project_WithMalformedLabel_EmitsMalformedLabelWarning(string malformedLabel)
    {
        var record = BuildRecord(1, malformedLabel);

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Warnings.Should().ContainSingle();
        var warning = result.Warnings[0];
        warning.Kind.Should().Be(ProjectionWarningKind.MalformedLabel);
        warning.IssueNumber.Should().Be(1);
        warning.OffendingLabel.Should().Be(malformedLabel);
    }

    // -------------------------------------------------------------------------
    // 22c. Malformed status → CREATED fallback + warning + renders
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithMalformedStatus_FallsBackToCreatedAndRenders()
    {
        var record = BuildRecord(1, "status:foo");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(1);
        result.Snapshot.Tickets[0].ColumnId.Value.Should().Be("CREATED");
        result.Warnings.Should().ContainSingle(w => w.Kind == ProjectionWarningKind.MalformedLabel);
    }

    // -------------------------------------------------------------------------
    // 22d. Malformed agent → pm fallback + warning + renders
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithMalformedAgent_FallsBackToPmAndRenders()
    {
        var record = BuildRecord(1, "agent:bob");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(1);
        result.Snapshot.Tickets[0].AgentId.Value.Should().Be("pm");
        result.Warnings.Should().ContainSingle(w => w.Kind == ProjectionWarningKind.MalformedLabel);
    }

    // -------------------------------------------------------------------------
    // 22e. Malformed retry → 0 fallback + warning + renders
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithMalformedRetry_FallsBackToZeroAndRenders()
    {
        var record = BuildRecord(1, "retry:xyz");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(1);
        result.Snapshot.Tickets[0].Retry.Value.Should().Be(0);
        result.Warnings.Should().ContainSingle(w => w.Kind == ProjectionWarningKind.MalformedLabel);
    }

    // -------------------------------------------------------------------------
    // 22f. THE security test: one bad among many → valid records render, warn only for bad
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_OneBadAmongMany_RendersValidTicketsAndWarnsOnlyForBad()
    {
        var records = new List<GitHubIssueRecord>
        {
            BuildRecordWithLabels(1, "status:created"),
            BuildRecordWithLabels(2, "status:foo"),
            BuildRecordWithLabels(3, "status:done")
        };

        var result = _sut.Project(records, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(3);
        result.Warnings.Should().ContainSingle();
        result.Warnings[0].IssueNumber.Should().Be(2);
        result.Warnings[0].Kind.Should().Be(ProjectionWarningKind.MalformedLabel);
    }

    // -------------------------------------------------------------------------
    // 22g. All valid → zero warnings
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_AllValid_EmitsNoWarnings()
    {
        var records = new List<GitHubIssueRecord>
        {
            BuildRecordWithLabels(1, "status:created", "agent:pm"),
            BuildRecordWithLabels(2, "status:in-review", "agent:dev-a"),
            BuildRecordWithLabels(3, "status:done", "agent:dev-b")
        };

        var result = _sut.Project(records, AsOf);

        result.Warnings.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // 22h. Multiple bad labels on one record → one warning per bad label
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_MultipleBadLabelsOnOneRecord_EmitsOneWarningPerBadLabel()
    {
        var record = BuildRecordWithLabels(1, "status:foo", "retry:xyz");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(1);
        result.Snapshot.Tickets[0].ColumnId.Value.Should().Be("CREATED");
        result.Snapshot.Tickets[0].Retry.Value.Should().Be(0);
        result.Warnings.Should().HaveCount(2);
        result.Warnings.Should().Contain(w => w.OffendingLabel == "status:foo");
        result.Warnings.Should().Contain(w => w.OffendingLabel == "retry:xyz");
    }

    // -------------------------------------------------------------------------
    // 22i. Unknown non-team prefix → renders, no warning
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("epic:ingestion")]
    [InlineData("size:L")]
    [InlineData("type:feat")]
    [InlineData("size:large")]
    public void Project_WithUnknownNonTeamPrefix_RendersWithNoWarning(string label)
    {
        var record = BuildRecord(1, label);

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(1);
        result.Warnings.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // 23. Structural: exactly 7 column IDs + 6 agent IDs
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_ColumnAndAgentIds_MatchExpectedSets()
    {
        var result = _sut.Project(new List<GitHubIssueRecord>(), AsOf);

        result.Snapshot.Columns.Select(c => c.Id.Value).Should().BeEquivalentTo(
            new List<string> { "CREATED", "SPECIFIED", "IN_DEVELOPMENT", "IN_REVIEW", "IN_QA", "AWAITING_VALIDATION", "DONE" });

        result.Snapshot.Agents.Select(a => a.Id.Value).Should().BeEquivalentTo(
            new List<string> { "pm", "architect", "dev-a", "dev-b", "qa", "security" });
    }

    // -------------------------------------------------------------------------
    // Mutation-killing targeted cases
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_InReview_IsThinkingFalse()
    {
        var record = BuildRecordWithLabels(1, "status:in-review", "agent:dev-a");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].IsThinking.Should().BeFalse();
    }

    [Fact]
    public void Project_Escalated_IsThinkingFalse()
    {
        var record = BuildRecordWithLabels(1, "status:escalated", "agent:dev-a", "retry:3");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].IsThinking.Should().BeFalse();
    }

    [Fact]
    public void Project_InReview_AndEscalated_WithRetry3_AndEscalationTarget_TargetIsPreserved()
    {
        var record = BuildRecordWithLabels(1, "status:in-review", "status:escalated", "agent:dev-a", "retry:3", "escalation-target:pm");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        var ticket = result.Snapshot.Tickets[0];
        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeTrue();
        ticket.EscalationTarget!.Value.Should().Be("pm");
    }

    [Fact]
    public void Project_Escalated_WithRetry3_AndExplicitEscalationTarget_TargetIsNotAgentId()
    {
        var record = BuildRecordWithLabels(1, "status:escalated", "agent:dev-a", "retry:3", "escalation-target:pm");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].EscalationTarget!.Value.Should().Be("pm");
    }

    [Fact]
    public void Project_DoneTicket_WithAgeExactly24h_IsStale()
    {
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("24h done ticket")
            .WithLabels("status:done")
            .WithCreatedAt(AsOf.AddHours(-24))
            .WithHtmlUrl(AnyUrl)
            .WithUpdatedAt(AsOf.AddHours(-24))
            .AsOpen()
            .Build();

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].Freshness.Should().Be(TicketFreshness.Stale);
    }

    [Fact]
    public void Project_WithUnknownPrefixLabel_DoesNotThrow()
    {
        var record = BuildRecord(1, "size:large");

        var act = () => _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        act.Should().NotThrow();
    }

    [Fact]
    public void Project_WithValidPrefixButInvalidValue_FallsBackToDefaultAndWarns()
    {
        // status:not-valid → CREATED column + one MalformedLabel warning (no throw)
        var record = BuildRecord(1, "status:not-valid");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets[0].ColumnId.Value.Should().Be("CREATED");
        result.Warnings.Should().ContainSingle(w =>
            w.Kind == ProjectionWarningKind.MalformedLabel &&
            w.OffendingLabel == "status:not-valid");
    }

    [Fact]
    public void Project_InReview_NotEscalated_WithEscalationTarget_EscalationTargetIsNull()
    {
        var record = BuildRecordWithLabels(1, "status:in-review", "agent:dev-a", "retry:0", "escalation-target:pm");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        var ticket = result.Snapshot.Tickets[0];
        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeFalse();
        ticket.EscalationTarget.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // 22j/22k. Out-of-range retry → does NOT throw + emits warning (B1)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("retry:4")]
    [InlineData("retry:999")]
    [InlineData("retry:-1")]
    public void Project_WithOutOfRangeRetryLabel_DoesNotThrow(string retryLabel)
    {
        var record = BuildRecord(1, retryLabel);

        var act = () => _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("retry:4")]
    [InlineData("retry:999")]
    public void Project_WithOutOfRangeRetryLabel_EmitsMalformedLabelWarning(string retryLabel)
    {
        var record = BuildRecord(1, retryLabel);

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Warnings.Should().ContainSingle();
        var warning = result.Warnings[0];
        warning.Kind.Should().Be(ProjectionWarningKind.MalformedLabel);
        warning.OffendingLabel.Should().Be(retryLabel);
    }

    // -------------------------------------------------------------------------
    // 22l. Out-of-range retry → 0 fallback + renders (B1)
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithOutOfRangeRetry_FallsBackToZeroAndRenders()
    {
        var record = BuildRecordWithLabels(1, "status:created", "agent:pm", "retry:4");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(1);
        result.Snapshot.Tickets[0].Retry.Value.Should().Be(0);
        result.Warnings.Should().ContainSingle(w =>
            w.Kind == ProjectionWarningKind.MalformedLabel &&
            w.OffendingLabel == "retry:4");
    }

    // -------------------------------------------------------------------------
    // 22m. Invalid co-agent → null fallback + warning (B2)
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_InReview_WithInvalidCoAgent_FallsBackAndWarns()
    {
        var record = BuildRecordWithLabels(1, "status:in-review", "agent:dev-a", "co-agent:unknown");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(1);
        var ticket = result.Snapshot.Tickets[0];
        ticket.IsInCrossReview.Should().BeTrue();
        // co-agent:unknown → null → defaults to agentId (dev-a) per MapToTicket logic
        ticket.CoAgentId!.Value.Should().Be("dev-a");
        result.Warnings.Should().ContainSingle(w =>
            w.Kind == ProjectionWarningKind.MalformedLabel &&
            w.OffendingLabel == "co-agent:unknown");
    }

    // -------------------------------------------------------------------------
    // 22n. Invalid escalation-target → null fallback + warning (B2)
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_Escalated_WithInvalidEscalationTarget_FallsBackAndWarns()
    {
        var record = BuildRecordWithLabels(1, "status:escalated", "agent:dev-a", "retry:3", "escalation-target:badvalue");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(1);
        var ticket = result.Snapshot.Tickets[0];
        ticket.IsEscalated.Should().BeTrue();
        // escalation-target:badvalue → null → falls back to agentId (dev-a) per MapToTicket logic
        ticket.EscalationTarget!.Value.Should().Be("dev-a");
        result.Warnings.Should().ContainSingle(w =>
            w.Kind == ProjectionWarningKind.MalformedLabel &&
            w.OffendingLabel == "escalation-target:badvalue");
    }

    // -------------------------------------------------------------------------
    // 22o. Bare word label (no colon) → renders, no warning (B3)
    // -------------------------------------------------------------------------

    [Fact]
    public void Project_WithBareWordLabel_RendersWithNoWarning()
    {
        var record = BuildRecord(1, "bug");

        var result = _sut.Project(new List<GitHubIssueRecord> { record }, AsOf);

        result.Snapshot.Tickets.Should().HaveCount(1);
        result.Warnings.Should().BeEmpty();
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
