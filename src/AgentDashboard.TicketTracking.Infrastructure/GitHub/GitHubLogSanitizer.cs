using System.Text.RegularExpressions;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

internal static partial class GitHubLogSanitizer
{
    internal static string Sanitize(string message)
    {
        message = GhpTokenPattern().Replace(message, "[REDACTED_TOKEN]");
        message = PatTokenPattern().Replace(message, "[REDACTED_TOKEN]");
        message = AuthorizationHeaderPattern().Replace(message, "Authorization: [REDACTED]");
        return message;
    }

    [GeneratedRegex("ghp_[A-Za-z0-9]{36}")]
    private static partial Regex GhpTokenPattern();

    [GeneratedRegex("github_pat_[A-Za-z0-9_]{22,}")]
    private static partial Regex PatTokenPattern();

    [GeneratedRegex("Authorization:.*")]
    private static partial Regex AuthorizationHeaderPattern();
}
