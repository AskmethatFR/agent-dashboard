using AgentDashboard.TicketTracking.Domain.Agents;

namespace AgentDashboard.TicketTracking.TestShared.Agents;

public static class AgentFixtures
{
    public static Agent DevA { get; } = new(
        new AgentId("DA"),
        new AgentName("DevA"),
        new AgentGlyph("Da"),
        new AgentRole("Developer A"));

    public static Agent DevB { get; } = new(
        new AgentId("DB"),
        new AgentName("DevB"),
        new AgentGlyph("Db"),
        new AgentRole("Developer B"));

    public static Agent Pm { get; } = new(
        new AgentId("PM"),
        new AgentName("PM"),
        new AgentGlyph("PM"),
        new AgentRole("Project Manager"));

    public static Agent Build(
        string id = "DA",
        string name = "DevA",
        string glyph = "Da",
        string role = "Developer A") =>
        new(new AgentId(id), new AgentName(name), new AgentGlyph(glyph), new AgentRole(role));
}
