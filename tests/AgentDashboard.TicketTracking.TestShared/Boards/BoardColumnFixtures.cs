using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.TestShared.Boards;

public static class BoardColumnFixtures
{
    public static BoardColumn Created { get; } =
        new(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));

    public static BoardColumn InDevelopment { get; } =
        new(new BoardColumnId("IN_DEVELOPMENT"), new BoardColumnLabel("In Development"));

    public static BoardColumn InQa { get; } =
        new(new BoardColumnId("IN_QA"), new BoardColumnLabel("In QA"));

    public static BoardColumn Done { get; } =
        new(new BoardColumnId("DONE"), new BoardColumnLabel("Done"));

    public static BoardColumn Build(string id = "CREATED", string label = "Created") =>
        new(new BoardColumnId(id), new BoardColumnLabel(label));
}
