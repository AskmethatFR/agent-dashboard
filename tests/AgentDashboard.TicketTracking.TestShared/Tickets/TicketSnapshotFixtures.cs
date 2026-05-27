using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.TestShared.Tickets;

public static class TicketSnapshotFixtures
{
    public static TicketSnapshot Default { get; } = Open();

    public static TicketSnapshot Open(
        int id = 1,
        string columnId = "CREATED",
        string title = "any title",
        string agentId = "DA",
        int retry = 0,
        TimeSpan? age = null,
        bool thinking = false,
        TicketFreshness freshness = TicketFreshness.Neutral) =>
        TicketSnapshot.Open(
            new TicketId(id),
            new BoardColumnId(columnId),
            new TicketTitle(title),
            new AgentId(agentId),
            new Retry(retry),
            new Age(age ?? TimeSpan.FromMinutes(10)),
            thinking,
            freshness);

    public static TicketSnapshot InCrossReview(
        int id = 1,
        string columnId = "CREATED",
        string title = "any title",
        string agentId = "DA",
        string coAgentId = "DB",
        int retry = 0,
        TimeSpan? age = null,
        bool thinking = false,
        TicketFreshness freshness = TicketFreshness.Neutral,
        string? escalationTarget = null) =>
        TicketSnapshot.InCrossReview(
            new TicketId(id),
            new BoardColumnId(columnId),
            new TicketTitle(title),
            new AgentId(agentId),
            new AgentId(coAgentId),
            new Retry(retry),
            new Age(age ?? TimeSpan.FromMinutes(10)),
            thinking,
            freshness,
            escalationTarget is null ? null : new AgentId(escalationTarget));

    public static TicketSnapshot Escalated(
        int id = 1,
        string columnId = "CREATED",
        string title = "any title",
        string agentId = "DA",
        string escalationTarget = "PM",
        int retry = 0,
        TimeSpan? age = null,
        bool thinking = false,
        TicketFreshness freshness = TicketFreshness.Neutral,
        string? coAgentId = null) =>
        TicketSnapshot.Escalated(
            new TicketId(id),
            new BoardColumnId(columnId),
            new TicketTitle(title),
            new AgentId(agentId),
            new AgentId(escalationTarget),
            new Retry(retry),
            new Age(age ?? TimeSpan.FromMinutes(10)),
            thinking,
            freshness,
            coAgentId is null ? null : new AgentId(coAgentId));
}
