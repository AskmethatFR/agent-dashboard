namespace AgentDashboard.TicketTracking.Domain.Agents;

public sealed record Agent
{
    public const int MaxNameLength = 128;
    public const int MaxGlyphLength = 8;
    public const int MaxRoleLength = 64;

    public AgentId Id { get; }
    public string Name { get; }
    public string Glyph { get; }
    public string Role { get; }

    public Agent(AgentId id, string name, string glyph, string role)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Agent name cannot be empty.", nameof(name));
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Agent name cannot exceed {MaxNameLength} characters.", nameof(name));
        if (string.IsNullOrWhiteSpace(glyph))
            throw new ArgumentException("Agent glyph cannot be empty.", nameof(glyph));
        if (glyph.Length > MaxGlyphLength)
            throw new ArgumentException($"Agent glyph cannot exceed {MaxGlyphLength} characters.", nameof(glyph));
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Agent role cannot be empty.", nameof(role));
        if (role.Length > MaxRoleLength)
            throw new ArgumentException($"Agent role cannot exceed {MaxRoleLength} characters.", nameof(role));

        Id = id;
        Name = name;
        Glyph = glyph;
        Role = role;
    }
}
