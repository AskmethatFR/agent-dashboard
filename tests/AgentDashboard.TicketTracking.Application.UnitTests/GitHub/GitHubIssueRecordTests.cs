using AgentDashboard.TicketTracking.TestShared.Factories;

namespace AgentDashboard.TicketTracking.Application.GitHub;

public class GitHubIssueRecordTests
{
    private static readonly DateTimeOffset FixedCreatedAt = new(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset FixedUpdatedAt = new(2024, 1, 15, 11, 0, 0, TimeSpan.Zero);
    private static readonly string FixedHtmlUrl = "https://github.com/AskmethatFR/agent-dashboard/issues/42";

    [Fact]
    public void CreateRecord_WithAllProperties()
    {
        var labels = new List<string> { "bug", "critical" };
        var createdAt = FixedCreatedAt;

        var record = new GitHubIssueRecordBuilder()
            .WithNumber(42)
            .WithTitle("Fix critical bug")
            .WithLabels(labels)
            .WithCreatedAt(createdAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        record.Number.Should().Be(42);
        record.Title.Should().Be("Fix critical bug");
        record.Labels.Should().BeEquivalentTo(labels);
        record.CreatedAt.Should().Be(createdAt);
        record.HtmlUrl.Should().Be(FixedHtmlUrl);
        record.UpdatedAt.Should().Be(FixedUpdatedAt);
        record.ClosedAt.Should().BeNull();
    }

    [Fact]
    public void EqualRecords_WithSameAllProperties_AreEqual()
    {
        var labels = new List<string> { "bug", "critical" };
        var createdAt = FixedCreatedAt;

        var record1 = new GitHubIssueRecordBuilder()
            .WithNumber(42)
            .WithTitle("Fix critical bug")
            .WithLabels(labels)
            .WithCreatedAt(createdAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        var record2 = new GitHubIssueRecordBuilder()
            .WithNumber(42)
            .WithTitle("Fix critical bug")
            .WithLabels(labels)
            .WithCreatedAt(createdAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        record1.Should().Be(record2);
        record1.Should().BeEquivalentTo(record2);
    }

    [Fact]
    public void EqualRecords_WithDifferentCreatedAt_AreNotEqual()
    {
        var labels = new List<string> { "bug", "critical" };
        var createdAt1 = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var createdAt2 = new DateTimeOffset(2024, 1, 16, 10, 30, 0, TimeSpan.Zero);

        var record1 = new GitHubIssueRecordBuilder()
            .WithNumber(42)
            .WithTitle("Fix critical bug")
            .WithLabels(labels)
            .WithCreatedAt(createdAt1)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        var record2 = new GitHubIssueRecordBuilder()
            .WithNumber(42)
            .WithTitle("Fix critical bug")
            .WithLabels(labels)
            .WithCreatedAt(createdAt2)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        record1.Should().NotBe(record2);
        record1.Should().NotBeEquivalentTo(record2);
    }

    [Fact]
    public void HashCode_ForEqualRecords_IsConsistent()
    {
        var labels = new List<string> { "bug", "critical" };

        var record1 = new GitHubIssueRecordBuilder()
            .WithNumber(42)
            .WithTitle("Fix critical bug")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        var record2 = new GitHubIssueRecordBuilder()
            .WithNumber(42)
            .WithTitle("Fix critical bug")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        record1.GetHashCode().Should().Be(record2.GetHashCode());
    }

    [Fact]
    public void Record_WithMinimumDateTimeOffset_CreatedAt()
    {
        var minDate = DateTimeOffset.MinValue;

        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(new List<string>())
            .WithCreatedAt(minDate)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        record.CreatedAt.Should().Be(minDate);
    }

    [Fact]
    public void Record_WithUtcCreatedAt()
    {
        var utcDate = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);

        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(new List<string>())
            .WithCreatedAt(utcDate)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        record.CreatedAt.Should().Be(utcDate);
        record.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Record_WithEmptyLabels()
    {
        var emptyLabels = Array.Empty<string>();

        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(emptyLabels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        record.Labels.Should().BeEmpty();
    }

    [Fact]
    public void Equality_WithNull_IsFalse()
    {
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(new List<string>())
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        (record == null).Should().BeFalse();
        (null == record).Should().BeFalse();
    }

    [Fact]
    public void Equality_ObeysSymmetry()
    {
        var labels = new List<string> { "test" };

        var record1 = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        var record2 = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        (record1 == record2).Should().BeTrue();
        (record2 == record1).Should().BeTrue();
    }

    [Fact]
    public void Equality_ObeysTransitivity()
    {
        var labels = new List<string> { "test" };

        var record1 = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        var record2 = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        var record3 = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        (record1 == record2).Should().BeTrue();
        (record2 == record3).Should().BeTrue();
        (record1 == record3).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentNumber_AreNotEqual()
    {
        var labels = new List<string> { "test" };

        var record1 = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        var record2 = new GitHubIssueRecordBuilder()
            .WithNumber(2)
            .WithTitle("Test")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        record1.Should().NotBe(record2);
    }

    [Fact]
    public void Equality_WithDifferentTitle_AreNotEqual()
    {
        var labels = new List<string> { "test" };

        var record1 = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test 1")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        var record2 = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test 2")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        record1.Should().NotBe(record2);
    }

    [Fact]
    public void Equality_WithDifferentLabels_AreNotEqual()
    {
        var record1 = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(new List<string> { "label1" })
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        var record2 = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(new List<string> { "label2" })
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        record1.Should().NotBe(record2);
    }

    [Fact]
    public void Deconstruct_ExtractsAllProperties()
    {
        var labels = new List<string> { "bug", "critical" };
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(42)
            .WithTitle("Fix critical bug")
            .WithLabels(labels)
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsOpen()
            .Build();

        var (number, title, extractedLabels, extractedCreatedAt, htmlUrl, updatedAt, closedAt) = record;

        number.Should().Be(42);
        title.Should().Be("Fix critical bug");
        extractedLabels.Should().BeEquivalentTo(labels);
        extractedCreatedAt.Should().Be(FixedCreatedAt);
        htmlUrl.Should().Be(FixedHtmlUrl);
        updatedAt.Should().Be(FixedUpdatedAt);
        closedAt.Should().BeNull();
    }

    [Fact]
    public void Record_WithClosedAt()
    {
        var closedAt = new DateTimeOffset(2024, 1, 20, 10, 0, 0, TimeSpan.Zero);
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("Test")
            .WithLabels(new List<string>())
            .WithCreatedAt(FixedCreatedAt)
            .WithHtmlUrl(FixedHtmlUrl)
            .WithUpdatedAt(FixedUpdatedAt)
            .AsClosed(closedAt)
            .Build();

        record.ClosedAt.Should().Be(closedAt);
    }
}
