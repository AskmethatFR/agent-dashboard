using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.TestShared.Tickets;

public static class TicketFixtures
{
    public static Ticket Default { get; } = Open();

    public static Ticket Open(
        int id = 1,
        string columnId = "CREATED",
        string title = "any title",
        string agentId = "DA",
        int retry = 0,
        TimeSpan? age = null,
        bool thinking = false,
        TicketFreshness freshness = TicketFreshness.Neutral) =>
        Ticket.Open(
            new TicketId(id),
            new BoardColumnId(columnId),
            new TicketTitle(title),
            new AgentId(agentId),
            new Retry(retry),
            new Age(age ?? TimeSpan.FromMinutes(10)),
            thinking,
            freshness);

    public static Ticket InCrossReview(
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
        Ticket.InCrossReview(
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

    public static Ticket Escalated(
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
        Ticket.Escalated(
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
