namespace AgentDashboard.TicketTracking.Application.GitHub;

using AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// The outcome of mapping a GitHub issue to a ticket: the mapped <see cref="Ticket"/>
/// plus any label-mapping warnings surfaced as data for Infrastructure to log.
/// </summary>
/// <param name="Ticket">The mapped ticket (always produced).</param>
/// <param name="Warnings">The warnings detected during mapping; empty when none.</param>
public sealed record MappingResult(Ticket Ticket, IReadOnlyList<MappingWarning> Warnings);
