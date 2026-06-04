using AgentDashboard.Web.Components.Shared;
using AgentDashboard.Web.Tests.Infrastructure;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.SharedComponent;

public sealed class CultureSelectorTests : IClassFixture<BunitFixture>
{
    private readonly BunitFixture _ctx;

    public CultureSelectorTests(BunitFixture ctx) => _ctx = ctx;

    [Fact]
    public void Should_RenderBothCultureButtons()
    {
        var cut = _ctx.Context.Render<CultureSelector>();
        
        var buttons = cut.FindAll(".culture-btn");
        buttons.Should().HaveCount(2);
    }

    [Fact]
    public void Should_RenderCorrectCultureCodes()
    {
        var cut = _ctx.Context.Render<CultureSelector>();
        
        var buttons = cut.FindAll(".culture-btn");
        var buttonTexts = buttons.Select(b => b.TextContent.Trim()).ToList();
        
        buttonTexts.Should().Contain("EN");
        buttonTexts.Should().Contain("FR");
    }

    [Fact]
    public void Should_HaveActiveClass_OnCurrentCulture()
    {
        // Set current culture to fr-FR
        var originalCulture = System.Globalization.CultureInfo.CurrentUICulture;
        var cultureInfo = new System.Globalization.CultureInfo("fr-FR");
        System.Globalization.CultureInfo.CurrentUICulture = cultureInfo;
        
        try
        {
            var cut = _ctx.Context.Render<CultureSelector>();
            
            var frButton = cut.FindAll(".culture-btn").FirstOrDefault(b => b.TextContent.Trim() == "FR");
            frButton.Should().NotBeNull();
            frButton!.ClassList.Should().Contain("active");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Fact]
    public void Should_HaveContainerWithAriaAttributes()
    {
        var cut = _ctx.Context.Render<CultureSelector>();
        
        var selector = cut.Find(".culture-selector");
        selector.GetAttribute("role").Should().Be("region");
        selector.GetAttribute("aria-label").Should().Be("Language selector");
    }

    [Fact]
    public void Should_HaveButtonsWithTypeButton()
    {
        var cut = _ctx.Context.Render<CultureSelector>();
        
        var buttons = cut.FindAll(".culture-btn");
        foreach (var button in buttons)
        {
            button.GetAttribute("type").Should().Be("button");
        }
    }
}
