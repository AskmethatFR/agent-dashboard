using AgentDashboard.Web.Components.Shared;
using AgentDashboard.Web.Tests.Infrastructure;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.SharedComponent;

public sealed class RetryCounterTests : IClassFixture<BunitFixture>
{
    private readonly BunitFixture _ctx;

    public RetryCounterTests(BunitFixture ctx) => _ctx = ctx;

    [Theory]
    [InlineData(0, 3, "retry-safe")]  // Safe
    [InlineData(1, 3, "retry-safe")]  // Safe
    [InlineData(2, 3, "retry-warn")]   // Warning
    [InlineData(3, 3, "retry-danger")] // Danger
    public void Should_RenderCorrectClass_When_RetryCountChanges(
        int current, int max, string expectedClass)
    {
        var cut = _ctx.Context.Render<RetryCounter>(p => p
            .Add(c => c.Current, current)
            .Add(c => c.Max, max));

        cut.Find($".{expectedClass}").Should().NotBeNull();
    }

    [Theory]
    [InlineData(0, 3, "0", "/3")]  // Safe
    [InlineData(1, 3, "1", "/3")]  // Safe
    [InlineData(2, 3, "2", "/3")]   // Warning
    [InlineData(3, 3, "3", "/3")] // Danger
    public void Should_RenderCorrectValues_When_RetryCountChanges(
        int current, int max, string expectedValue, string expectedMax)
    {
        var cut = _ctx.Context.Render<RetryCounter>(p => p
            .Add(c => c.Current, current)
            .Add(c => c.Max, max));

        cut.Find(".retry-value").TextContent.Should().Be(expectedValue);
        cut.Find(".retry-max").TextContent.Should().Be(expectedMax);
    }

    [Fact]
    public void Should_UseDefaultMax_When_NotSpecified()
    {
        var cut = _ctx.Context.Render<RetryCounter>(p => p
            .Add(c => c.Current, 1));

        cut.Find(".retry-max").TextContent.Should().Be("/3");
    }

    [Fact]
    public void Should_HaveAriaLabel_ForAccessibility()
    {
        var cut = _ctx.Context.Render<RetryCounter>(p => p
            .Add(c => c.Current, 2)
            .Add(c => c.Max, 3));

        var span = cut.Find("span[aria-label]");
        span.GetAttribute("aria-label").Should().Be("Retry count warning: 2 of 3");
    }

    [Theory]
    [InlineData(0, "Retry count: 0 of 3")]
    [InlineData(1, "Retry count: 1 of 3")]
    [InlineData(2, "Retry count warning: 2 of 3")]
    [InlineData(3, "Retry count at maximum: 3 of 3")]
    public void Should_HaveCorrectAriaLabel_ForEachState(int current, string expectedLabel)
    {
        var cut = _ctx.Context.Render<RetryCounter>(p => p
            .Add(c => c.Current, current)
            .Add(c => c.Max, 3));

        var span = cut.Find("span[aria-label]");
        span.GetAttribute("aria-label").Should().Be(expectedLabel);
    }
}
