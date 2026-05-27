namespace AgentDashboard.TicketTracking.Domain.Abstractions;

public interface IAggregateRoot<out TId> : IEntity<TId> where TId : notnull
{
}
