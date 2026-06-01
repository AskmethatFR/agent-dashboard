using AgentDashboard.TicketTracking.Infrastructure.GitHub;

namespace AgentDashboard.TicketTracking.Infrastructure.UnitTests.GitHub;

public sealed class GitHubLogSanitizerTests
{
    [Fact]
    public void RedactFineGrainedPat_WithFull82CharToken()
    {
        // Fine-grained PAT format: github_pat_<22 alphanum>_<59 base62>
        // Total length: 12 (github_pat_) + 22 + 1 (_) + 59 = 94 chars
        const string fineGrainedPat = "github_pat_11ABCDEFGHIJKLMNOPQRSTUV_UVwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456";
        const string message = $"Token: {fineGrainedPat}";

        var sanitized = GitHubLogSanitizer.Sanitize(message);

        sanitized.Should().NotContain(fineGrainedPat);
        sanitized.Should().NotContain("github_pat_11");
        sanitized.Should().NotContain("UVwxyz");
        sanitized.Should().Contain("[REDACTED_TOKEN]");
    }

    [Fact]
    public void RedactFineGrainedPat_WithMinimumLength()
    {
        // Minimum fine-grained PAT: github_pat_ + 22 chars + _ + 1 char
        const string minFineGrainedPat = "github_pat_1234567890123456789012_a";
        const string message = $"Token: {minFineGrainedPat}";

        var sanitized = GitHubLogSanitizer.Sanitize(message);

        sanitized.Should().NotContain(minFineGrainedPat);
        sanitized.Should().Contain("[REDACTED_TOKEN]");
    }

    [Fact]
    public void RedactClassicPat_WithGhpPrefix()
    {
        const string classicPat = "ghp_0123456789abcdef0123456789abcdef01234";
        const string message = $"Token: {classicPat} in logs";

        var sanitized = GitHubLogSanitizer.Sanitize(message);

        sanitized.Should().NotContain(classicPat);
        sanitized.Should().NotContain("ghp_012");
        sanitized.Should().Contain("[REDACTED_TOKEN]");
    }

    [Fact]
    public void RedactAuthorizationHeader()
    {
        const string message = "Authorization: Bearer ghp_secret1234567890";

        var sanitized = GitHubLogSanitizer.Sanitize(message);

        sanitized.Should().NotContain("ghp_secret");
        sanitized.Should().Contain("Authorization: [REDACTED]");
    }

    [Fact]
    public void PreserveNonTokenText()
    {
        const string message = "This is a normal log message with no tokens";

        var sanitized = GitHubLogSanitizer.Sanitize(message);

        sanitized.Should().Be(message);
    }
}
