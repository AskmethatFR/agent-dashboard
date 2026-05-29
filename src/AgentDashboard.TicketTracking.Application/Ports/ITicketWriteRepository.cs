namespace AgentDashboard.TicketTracking.Application.Ports;

using AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// Port for writing Ticket entities to persistence.
/// This is a write-only port - read operations are handled separately in the read model.
/// </summary>
public interface ITicketWriteRepository
{
    /// <summary>
    /// Saves a ticket to the repository.
    /// This operation is idempotent - if a ticket with the same (RepositorySource, GitHubIssueNumber) 
    /// already exists, it will be updated.
    /// </summary>
    /// <param name="ticket">The ticket to save.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveAsync(Ticket ticket, CancellationToken cancellationToken);
}
