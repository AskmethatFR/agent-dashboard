using System.Globalization;
using System.Text;
using AgentDashboard.TicketTracking.Application;
using AgentDashboard.TicketTracking.Infrastructure;
using AgentDashboard.Web.Components;
using AgentDashboard.Web.Endpoints;
using AgentDashboard.Web.Store;
using Microsoft.AspNetCore.Localization;
using AspNetCore.Localizer.Json.Extensions;
using AspNetCore.Localizer.Json.JsonOptions;
using Blazor.Redux;
using Blazor.Redux.Core;
using Blazor.Redux.Extensions;
using Blazor.Redux.Interfaces;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrEmpty(builder.Configuration["DATA_PATH"]))
{
    builder.Configuration["DATA_PATH"] = Path.Combine(builder.Environment.ContentRootPath, "data");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure JSON-based localization
builder.Services.AddJsonLocalization(options =>
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

builder.Services.AddTicketTrackingApplication();
builder.Services.AddTicketTrackingInfrastructure();
builder.Services.AddTicketTrackingGitHubIngestion(builder.Configuration);

// State management - Blazor.Redux
builder.Services.AddBlazorRedux(new BlazorReduxOption
{
    Slices = [BoardSlice.Initial],
    ReplayLastAction = false,
    SnapshotStrategy = SnapshotStrategy.DeepCopy,
    EffectsCancellationStrategy = EffectsCancellationStrategy.None
});

// Register reducers
builder.Services.AddScoped<IReducer<BoardSlice, LoadBoardAction>, LoadBoardReducer>();
builder.Services.AddScoped<IReducer<BoardSlice, LoadBoardSuccessAction>, LoadBoardSuccessReducer>();
builder.Services.AddScoped<IReducer<BoardSlice, LoadBoardFailureAction>, LoadBoardFailureReducer>();

// Register async reducer
builder.Services.AddScoped<IAsyncReducer<BoardSlice, LoadBoardAction>, LoadBoardAsyncReducer>();

// Register BoardCacheMonitor for live refresh
builder.Services.AddScoped<BoardCacheMonitor>();
builder.Services.AddHostedService<BoardCacheMonitorHostService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseAntiforgery();

// Configure request localization with query string and cookie support
var supportedCultures = new[] { "en-US", "fr-FR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures)
    .AddInitialRequestCultureProvider(new QueryStringRequestCultureProvider());
app.UseRequestLocalization(localizationOptions);

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Health check endpoint for Docker HEALTHCHECK
app.MapHealthz();

app.Run();

// Expose Program for WebApplicationFactory<Program> in test projects.
public partial class Program;
