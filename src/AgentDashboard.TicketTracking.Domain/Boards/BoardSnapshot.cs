using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.Boards;

public sealed record BoardSnapshot
{
    public IReadOnlyList<BoardColumn> Columns { get; }
    public IReadOnlyList<Ticket> Tickets { get; }
    public IReadOnlyList<Agent> Agents { get; }

    public BoardSnapshot(
        IReadOnlyList<BoardColumn> columns,
        IReadOnlyList<Ticket> tickets,
        IReadOnlyList<Agent> agents)
    {
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(tickets);
        ArgumentNullException.ThrowIfNull(agents);

        var columnIds = columns.Select(c => c.Id).ToHashSet();
        var agentIds = agents.Select(a => a.Id).ToHashSet();
        var seenTicketIds = new HashSet<TicketId>();

        foreach (var ticket in tickets)
        {
            if (!seenTicketIds.Add(ticket.Id))
                throw new ArgumentException(
                    $"Duplicate ticket id {ticket.Id} in snapshot.",
                    nameof(tickets));
            if (!columnIds.Contains(ticket.ColumnId))
                throw new ArgumentException(
                    $"Ticket {ticket.Id} references unknown column {ticket.ColumnId}.",
                    nameof(tickets));
            if (!agentIds.Contains(ticket.AgentId))
                throw new ArgumentException(
                    $"Ticket {ticket.Id} references unknown agent {ticket.AgentId}.",
                    nameof(tickets));
            if (ticket.CoAgentId is not null && !agentIds.Contains(ticket.CoAgentId))
                throw new ArgumentException(
                    $"Ticket {ticket.Id} references unknown co-agent {ticket.CoAgentId}.",
                    nameof(tickets));
            if (ticket.EscalationTarget is not null && !agentIds.Contains(ticket.EscalationTarget))
                throw new ArgumentException(
                    $"Ticket {ticket.Id} references unknown escalation target {ticket.EscalationTarget}.",
                    nameof(tickets));
        }

        Columns = columns;
        Tickets = tickets;
        Agents = agents;
    }
}
