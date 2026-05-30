namespace AgentDashboard.TicketTracking.Application.GitHub;

/// <summary>
/// A value object describing a label-mapping anomaly detected for a GitHub issue.
/// Carries only taxonomy-bounded data (issue number, label strings) so it is safe to log.
/// </summary>
public sealed record MappingWarning
{
    private MappingWarning(
        MappingWarningKind kind,
        long issueNumber,
        IReadOnlyList<string> conflictingStatusLabels,
        string? selectedStatusLabel)
    {
        Kind = kind;
        IssueNumber = issueNumber;
        ConflictingStatusLabels = conflictingStatusLabels;
        SelectedStatusLabel = selectedStatusLabel;
    }

    /// <summary>The kind of anomaly detected.</summary>
    public MappingWarningKind Kind { get; }

    /// <summary>The GitHub issue number the warning relates to.</summary>
    public long IssueNumber { get; }

    /// <summary>
    /// The full set of recognized status:* labels found on the issue.
    /// Empty for <see cref="MappingWarningKind.MissingStatusLabel"/>.
    /// </summary>
    public IReadOnlyList<string> ConflictingStatusLabels { get; }

    /// <summary>
    /// The status:* label selected as latest in the state machine.
    /// Null for <see cref="MappingWarningKind.MissingStatusLabel"/>.
    /// </summary>
    public string? SelectedStatusLabel { get; }

    /// <summary>
    /// Creates a warning for an issue carrying more than one recognized status:* label.
    /// </summary>
    public static MappingWarning MultipleStatusLabels(
        long issueNumber,
        IReadOnlyList<string> conflictingStatusLabels,
        string selectedStatusLabel) =>
        new(MappingWarningKind.MultipleStatusLabels, issueNumber, conflictingStatusLabels, selectedStatusLabel);

    /// <summary>
    /// Creates a warning for an issue carrying no recognized status:* label.
    /// </summary>
    public static MappingWarning MissingStatusLabel(long issueNumber) =>
        new(MappingWarningKind.MissingStatusLabel, issueNumber, Array.Empty<string>(), null);

    /// <inheritdoc />
    public bool Equals(MappingWarning? other)
    {
        if (other is null)
        {
            return false;
        }

        return Kind == other.Kind
            && IssueNumber == other.IssueNumber
            && SelectedStatusLabel == other.SelectedStatusLabel
            && ConflictingStatusLabels.SequenceEqual(other.ConflictingStatusLabels);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Kind);
        hash.Add(IssueNumber);
        hash.Add(SelectedStatusLabel);
        foreach (var label in ConflictingStatusLabels)
        {
            hash.Add(label);
        }

        return hash.ToHashCode();
    }
}
