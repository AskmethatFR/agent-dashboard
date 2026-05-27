namespace AgentDashboard.TicketTracking.Application.GitHub;

public class GitHubIssueRecordTests
{
    // Test List (Kent Beck's Test List technique)
    // ==========================================
    // Happy path cases:
    // 1. Create record with all properties including CreatedAt
    // 2. Equality with same CreatedAt
    // 3. Equality with different CreatedAt
    // 4. HashCode consistency with equality
    //
    // Boundary cases:
    // 5. CreatedAt with minimum DateTimeOffset
    // 6. CreatedAt with maximum DateTimeOffset
    // 7. CreatedAt with UTC date
    // 8. Empty labels list
    //
    // Invalid inputs:
    // 9. Null labels (should compile error with record, but test for runtime behavior)
    //
    // Error cases: N/A for record
    //
    // State transitions: N/A (immutable record)
    //
    // Equality contract:
    // - Positive equality (same values)
    // - Negative equality (different values)
    // - Null comparison
    // - Cross-type comparison
    // - Symmetry
    // - Transitivity
    // - GetHashCode consistency

    [Fact]
    public void CreateRecord_WithAllProperties_IncludingCreatedAt()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string> { "bug", "critical" };

        // Act
        var record = new GitHubIssueRecord(
            Number: 42,
            Title: "Fix critical bug",
            Labels: labels,
            CreatedAt: createdAt);

        // Assert
        record.Number.Should().Be(42);
        record.Title.Should().Be("Fix critical bug");
        record.Labels.Should().BeEquivalentTo(labels);
        record.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void EqualRecords_WithSameCreatedAt_AreEqual()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string> { "bug", "critical" };

        var record1 = new GitHubIssueRecord(42, "Fix critical bug", labels, createdAt);
        var record2 = new GitHubIssueRecord(42, "Fix critical bug", labels, createdAt);

        // Assert
        record1.Should().Be(record2);
        record1.Should().BeEquivalentTo(record2);
    }

    [Fact]
    public void EqualRecords_WithDifferentCreatedAt_AreNotEqual()
    {
        // Arrange
        var createdAt1 = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var createdAt2 = new DateTimeOffset(2024, 1, 16, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string> { "bug", "critical" };

        var record1 = new GitHubIssueRecord(42, "Fix critical bug", labels, createdAt1);
        var record2 = new GitHubIssueRecord(42, "Fix critical bug", labels, createdAt2);

        // Assert
        record1.Should().NotBe(record2);
        record1.Should().NotBeEquivalentTo(record2);
    }

    [Fact]
    public void HashCode_ForEqualRecords_IsConsistent()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string> { "bug", "critical" };

        var record1 = new GitHubIssueRecord(42, "Fix critical bug", labels, createdAt);
        var record2 = new GitHubIssueRecord(42, "Fix critical bug", labels, createdAt);

        // Assert
        record1.GetHashCode().Should().Be(record2.GetHashCode());
    }

    [Fact]
    public void Record_WithMinimumDateTimeOffset_CreatedAt()
    {
        // Arrange
        var minDate = DateTimeOffset.MinValue;
        var labels = new List<string>();

        // Act
        var record = new GitHubIssueRecord(1, "Test", labels, minDate);

        // Assert
        record.CreatedAt.Should().Be(minDate);
    }

    [Fact]
    public void Record_WithUtcCreatedAt()
    {
        // Arrange
        var utcDate = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string>();

        // Act
        var record = new GitHubIssueRecord(1, "Test", labels, utcDate);

        // Assert
        record.CreatedAt.Should().Be(utcDate);
        record.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Record_WithEmptyLabels()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var emptyLabels = Array.Empty<string>();

        // Act
        var record = new GitHubIssueRecord(1, "Test", emptyLabels, createdAt);

        // Assert
        record.Labels.Should().BeEmpty();
    }

    [Fact]
    public void Equality_WithNull_IsFalse()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string>();
        var record = new GitHubIssueRecord(1, "Test", labels, createdAt);

        // Assert
        // Note: Records in C# do not equal null by default
        (record == null).Should().BeFalse();
        (null == record).Should().BeFalse();
    }

    [Fact]
    public void Equality_ObeysSymmetry()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string> { "test" };

        var record1 = new GitHubIssueRecord(1, "Test", labels, createdAt);
        var record2 = new GitHubIssueRecord(1, "Test", labels, createdAt);

        // Assert
        (record1 == record2).Should().BeTrue();
        (record2 == record1).Should().BeTrue();
    }

    [Fact]
    public void Equality_ObeysTransitivity()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string> { "test" };

        var record1 = new GitHubIssueRecord(1, "Test", labels, createdAt);
        var record2 = new GitHubIssueRecord(1, "Test", labels, createdAt);
        var record3 = new GitHubIssueRecord(1, "Test", labels, createdAt);

        // Assert
        (record1 == record2).Should().BeTrue();
        (record2 == record3).Should().BeTrue();
        (record1 == record3).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentNumber_AreNotEqual()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string> { "test" };

        var record1 = new GitHubIssueRecord(1, "Test", labels, createdAt);
        var record2 = new GitHubIssueRecord(2, "Test", labels, createdAt);

        // Assert
        record1.Should().NotBe(record2);
    }

    [Fact]
    public void Equality_WithDifferentTitle_AreNotEqual()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string> { "test" };

        var record1 = new GitHubIssueRecord(1, "Test 1", labels, createdAt);
        var record2 = new GitHubIssueRecord(1, "Test 2", labels, createdAt);

        // Assert
        record1.Should().NotBe(record2);
    }

    [Fact]
    public void Equality_WithDifferentLabels_AreNotEqual()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);

        var record1 = new GitHubIssueRecord(1, "Test", new List<string> { "label1" }, createdAt);
        var record2 = new GitHubIssueRecord(1, "Test", new List<string> { "label2" }, createdAt);

        // Assert
        record1.Should().NotBe(record2);
    }

    [Fact]
    public void Deconstruct_ExtractsAllProperties()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var labels = new List<string> { "bug", "critical" };
        var record = new GitHubIssueRecord(42, "Fix critical bug", labels, createdAt);

        // Act
        var (number, title, extractedLabels, extractedCreatedAt) = record;

        // Assert
        number.Should().Be(42);
        title.Should().Be("Fix critical bug");
        extractedLabels.Should().BeEquivalentTo(labels);
        extractedCreatedAt.Should().Be(createdAt);
    }
}
