using AgentDashboard.TicketTracking.Domain.Abstractions;

namespace AgentDashboard.TicketTracking.Domain.Agents;

public sealed record Agent : IEntity<AgentId>
{
    public AgentId Id { get; }
    public AgentName Name { get; }
    public AgentGlyph Glyph { get; }
    public AgentRole Role { get; }

    public Agent(AgentId id, AgentName name, AgentGlyph glyph, AgentRole role)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(glyph);
        ArgumentNullException.ThrowIfNull(role);

        Id = id;
        Name = name;
        Glyph = glyph;
        Role = role;
    }
}
