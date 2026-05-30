namespace AgentDashboard.TicketTracking.Application.GitHub;

/// <summary>
/// Kinds of warning surfaced while mapping a GitHub issue's labels to a ticket.
/// </summary>
public enum MappingWarningKind
{
    /// <summary>The issue carried more than one recognized status:* label.</summary>
    MultipleStatusLabels,

    /// <summary>The issue carried no recognized status:* label.</summary>
    MissingStatusLabel
}
