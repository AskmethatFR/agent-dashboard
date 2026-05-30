namespace AgentDashboard.TicketTracking.Infrastructure.Tickets.Persistence;

internal sealed class CorruptedTicketRowException : Exception
{
    internal CorruptedTicketRowException(string column, string repo, long gitHubIssueNumber, Exception inner)
        : base(
            $"Corrupted ticket row: column '{column}' could not be parsed for ticket ({repo}, #{gitHubIssueNumber}).",
            inner)
    {
    }
}
