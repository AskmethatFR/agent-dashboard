using AgentDashboard.TicketTracking.Application;
using AgentDashboard.TicketTracking.Infrastructure;
using AgentDashboard.Web.Components;
using AgentDashboard.Web.Endpoints;
using AgentDashboard.Web.Store;
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Health check endpoint for Docker HEALTHCHECK
app.MapHealthz();

app.Run();

// Expose Program for WebApplicationFactory<Program> in test projects.
public partial class Program;
