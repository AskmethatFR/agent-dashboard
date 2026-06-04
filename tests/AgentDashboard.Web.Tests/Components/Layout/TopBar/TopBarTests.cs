using AgentDashboard.TicketTracking.Application;
using AgentDashboard.TicketTracking.Infrastructure;
using AgentDashboard.Web.Store;
using AspNetCore.Localizer.Json.Extensions;
using AspNetCore.Localizer.Json.JsonOptions;
using Blazor.Redux;
using Blazor.Redux.Core;
using Blazor.Redux.Extensions;
using Blazor.Redux.Interfaces;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using System.Globalization;
using System.Text;
using Xunit;
using TopBarComponent = AgentDashboard.Web.Components.Layout.TopBar.TopBar;

namespace AgentDashboard.Web.Tests.Components.Layout.TopBar;

public sealed class TopBarTests
{
    [Fact]
    public void Should_RenderAllFourRegions()
    {
        using var ctx = BuildContext();

        var cut = ctx.Render<TopBarComponent>();

        cut.Find(".brand").Should().NotBeNull();
        cut.Find("nav[aria-label='primary']").Should().NotBeNull();
        cut.Find(".live-clock").Should().NotBeNull();
        cut.Find("button[aria-label='Refresh tickets now']").Should().NotBeNull();
    }

    private static BunitContext BuildContext()
    {
        var ctx = new BunitContext();
        
        // Configure JSON localization
        ctx.Services.AddJsonLocalization(options =>
        {
            options.ResourcesPath = "i18n";
            options.UseEmbeddedResources = false;
            options.SupportedCultureInfos = new HashSet<CultureInfo>
            {
                new CultureInfo("en-US"),
                new CultureInfo("fr-FR")
            };
            options.LocalizationMode = LocalizationMode.I18n;
            options.CacheDuration = TimeSpan.FromMinutes(30);
            options.FileEncoding = Encoding.UTF8;
            options.IgnoreJsonErrors = false;
        });
        
        ctx.Services.AddSingleton<TimeProvider>(
            new FakeTimeProvider(new DateTimeOffset(2026, 5, 22, 10, 30, 0, TimeSpan.Zero)));
        ctx.Services.AddTicketTrackingApplication();
        ctx.Services.AddTicketTrackingInfrastructure();

        ctx.Services.AddBlazorRedux(new BlazorReduxOption
        {
            Slices = [BoardSlice.Initial],
            ReplayLastAction = false,
            SnapshotStrategy = SnapshotStrategy.DeepCopy,
            EffectsCancellationStrategy = EffectsCancellationStrategy.None,
        });

        ctx.Services.AddScoped<IReducer<BoardSlice, LoadBoardAction>, LoadBoardReducer>();
        ctx.Services.AddScoped<IReducer<BoardSlice, LoadBoardSuccessAction>, LoadBoardSuccessReducer>();
        ctx.Services.AddScoped<IReducer<BoardSlice, LoadBoardFailureAction>, LoadBoardFailureReducer>();
        ctx.Services.AddScoped<IAsyncReducer<BoardSlice, LoadBoardAction>, LoadBoardAsyncReducer>();

        return ctx;
    }
}
