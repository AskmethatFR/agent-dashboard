namespace AgentDashboard.TicketTracking.Application.Boards;

/// <summary>
/// Describes a label-mapping anomaly on the read-side (board projection).
/// Carries only taxonomy-bounded data: issue number and offending label string.
/// Compiler-generated record equality — all members are value-comparable primitives.
/// </summary>
public sealed record ProjectionWarning
{
    private ProjectionWarning(
        ProjectionWarningKind kind,
        long issueNumber,
        string offendingLabel)
    {
        Kind = kind;
        IssueNumber = issueNumber;
        OffendingLabel = offendingLabel;
    }

    public ProjectionWarningKind Kind { get; }
    public long IssueNumber { get; }
    public string OffendingLabel { get; }

    public static ProjectionWarning MalformedLabel(long issueNumber, string offendingLabel) =>
        new(ProjectionWarningKind.MalformedLabel, issueNumber, offendingLabel);
}
