namespace AgentDashboard.TicketTracking.Domain.Abstractions;

public interface IEntity<out TId> where TId : notnull
{
    TId Id { get; }
}
