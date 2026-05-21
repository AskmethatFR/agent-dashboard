namespace AgentDashboard.TicketTracking.Domain.Agents;

public sealed record Agent
{
    public AgentId Id { get; }
    public string Name { get; }
    public string Glyph { get; }
    public string Role { get; }

    public Agent(AgentId id, string name, string glyph, string role)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Agent name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(glyph))
            throw new ArgumentException("Agent glyph cannot be empty.", nameof(glyph));
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Agent role cannot be empty.", nameof(role));

        Id = id;
        Name = name;
        Glyph = glyph;
        Role = role;
    }
}
