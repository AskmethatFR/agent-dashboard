namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

using System.Globalization;
using AgentDashboard.TicketTracking.Domain.Tickets;

public sealed class TimestampUtcTests
{
    // Test List:
    // 1. Happy path: valid DateTimeOffset creates TimestampUtc
    // 2. Boundary: minimum DateTimeOffset value
    // 3. Boundary: maximum DateTimeOffset value
    // 4. Boundary: current UTC time
    // 5. Invalid: non-UTC offset rejected
    // 6. Equality: same value equals
    // 7. Equality: different value not equals
    // 8. Equality: equals null returns false
    // 9. Equality: GetHashCode consistency
    // 10. ToString: returns ISO 8601 string

    [Fact]
    public void Ctor_WithValidDateTimeOffset_ReturnsTimestampUtc()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);

        // Act
        var timestamp = new TimestampUtc(dateTime);

        // Assert
        Assert.Equal(dateTime, timestamp.Value);
    }

    [Fact]
    public void Ctor_WithMinimumDateTimeOffset_ReturnsTimestampUtc()
    {
        // Arrange
        var minDate = DateTimeOffset.MinValue;

        // Act
        var timestamp = new TimestampUtc(minDate);

        // Assert
        Assert.Equal(minDate, timestamp.Value);
    }

    [Fact]
    public void Ctor_WithMaximumDateTimeOffset_ReturnsTimestampUtc()
    {
        // Arrange
        var maxDate = DateTimeOffset.MaxValue;

        // Act
        var timestamp = new TimestampUtc(maxDate);

        // Assert
        Assert.Equal(maxDate, timestamp.Value);
    }

    [Fact]
    public void Ctor_WithUtcNow_ReturnsTimestampUtc()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        var now = DateTimeOffset.UtcNow;
        var after = DateTimeOffset.UtcNow;

        // Act
        var timestamp = new TimestampUtc(now);

        // Assert
        Assert.InRange(timestamp.Value, before, after);
    }

    [Fact]
    public void Ctor_WithNonUtcOffset_ThrowsArgumentException()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.FromHours(2));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new TimestampUtc(dateTime));
        Assert.Equal("value", ex.ParamName);
        Assert.Contains("UTC", ex.Message);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var timestamp1 = new TimestampUtc(dateTime);
        var timestamp2 = new TimestampUtc(dateTime);

        // Act & Assert
        Assert.Equal(timestamp1, timestamp2);
        Assert.True(timestamp1 == timestamp2);
        Assert.False(timestamp1 != timestamp2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var dateTime1 = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var dateTime2 = new DateTimeOffset(2024, 1, 16, 10, 30, 0, TimeSpan.Zero);
        var timestamp1 = new TimestampUtc(dateTime1);
        var timestamp2 = new TimestampUtc(dateTime2);

        // Act & Assert
        Assert.NotEqual(timestamp1, timestamp2);
        Assert.False(timestamp1 == timestamp2);
        Assert.True(timestamp1 != timestamp2);
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var timestamp = new TimestampUtc(dateTime);

        // Act & Assert
        Assert.NotNull(timestamp);
        Assert.False(timestamp.Equals(null));
        Assert.False(null == timestamp);
        Assert.False(timestamp == null);
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var timestamp = new TimestampUtc(dateTime);
        var otherTimestamp = new TimestampUtc(new DateTimeOffset(2024, 1, 16, 10, 30, 0, TimeSpan.Zero));

        // Act & Assert
        // Two TimestampUtc instances with different values should not be equal
        Assert.NotEqual(timestamp, otherTimestamp);
        Assert.False(timestamp.Equals(otherTimestamp));
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var timestamp1 = new TimestampUtc(dateTime);
        var timestamp2 = new TimestampUtc(dateTime);

        // Act & Assert
        Assert.Equal(timestamp1.GetHashCode(), timestamp2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValue_ReturnsDifferentHashCode()
    {
        // Arrange
        var dateTime1 = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var dateTime2 = new DateTimeOffset(2024, 1, 16, 10, 30, 0, TimeSpan.Zero);
        var timestamp1 = new TimestampUtc(dateTime1);
        var timestamp2 = new TimestampUtc(dateTime2);

        // Act & Assert
        var hash1 = timestamp1.GetHashCode();
        var hash2 = timestamp2.GetHashCode();
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ToString_ReturnsIso8601String()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var timestamp = new TimestampUtc(dateTime);
        var expected = dateTime.ToString("o");

        // Act
        var result = timestamp.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToString_RoundtripsCorrectly()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.Zero);
        var timestamp = new TimestampUtc(dateTime);

        // Act
        var str = timestamp.ToString();
        var parsed = DateTimeOffset.Parse(str, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(dateTime, parsed);
    }

    [Fact]
    public void ImplicitConversion_FromDateTimeOffset_ToTimestampUtc_Works()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);

        // Act
        TimestampUtc timestamp = dateTime;

        // Assert
        Assert.Equal(dateTime, timestamp.Value);
    }

    [Fact]
    public void ImplicitConversion_FromTimestampUtc_ToDateTimeOffset_Works()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var timestamp = new TimestampUtc(dateTime);

        // Act
        DateTimeOffset result = timestamp;

        // Assert
        Assert.Equal(dateTime, result);
    }
}
