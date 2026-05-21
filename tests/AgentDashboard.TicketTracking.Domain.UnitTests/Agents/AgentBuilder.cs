using AgentDashboard.TicketTracking.Domain.Agents;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

internal sealed class AgentBuilder
{
    private AgentId _id = new("DA");
    private AgentName _name = new("DevA");
    private AgentGlyph _glyph = new("Da");
    private AgentRole _role = new("Developer A");

    public AgentBuilder WithId(AgentId id) { _id = id; return this; }
    public AgentBuilder WithId(string id) { _id = new AgentId(id); return this; }
    public AgentBuilder WithName(string name) { _name = new AgentName(name); return this; }
    public AgentBuilder WithGlyph(string glyph) { _glyph = new AgentGlyph(glyph); return this; }
    public AgentBuilder WithRole(string role) { _role = new AgentRole(role); return this; }

    public Agent Build() => new(_id, _name, _glyph, _role);
}
