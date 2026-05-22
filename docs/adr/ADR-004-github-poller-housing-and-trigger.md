# ADR-004: GitHub Issues poller housing, on-demand trigger surface, and time abstraction

- Status: Accepted
- Date: 2026-05-22
- Deciders: Project Architect (issue #5 tech spec)
- Context tags: architecture, ingestion, hosting, testing

## Context and Problem Statement

EPIC-1 (#1) introduces the first vertical slice of GitHub ingestion via
issue #5: a hosted `BackgroundService` that authenticates against the
GitHub Issues API, polls the configured repo on a configurable cadence,
exposes an on-demand "refresh now" trigger (used later by the manual
refresh button in #14), and emits a structured log line summarising
each poll.

Three architectural questions had to be answered before any production
code landed:

1. **Where does the poller live?** It needs Octokit, which must stay
   out of Application and Domain. The repo already organises adapters
   by folder inside `AgentDashboard.TicketTracking.Infrastructure`
   (e.g. `Boards/StubBoardReader.cs`).
2. **How is the on-demand "refresh now" surface exposed?** ADR-003
   chose Cortex.Mediator as the in-process CQRS bus. The refresh
   trigger is a control signal, not a CQRS message — it carries no
   request payload and produces no result. Should it ride on
   Cortex.Mediator anyway?
3. **How is time abstracted for deterministic tests of the polling
   cadence and trigger semantics?** A custom `IClock` was the
   convention five years ago; .NET 8+ ships `TimeProvider` with a
   first-party `FakeTimeProvider` for tests.

A fourth, smaller question is options validation: `Microsoft.Extensions.Options`
typed binding is on the roadmap for EPIC-3, but this slice needs to
fail-fast on missing `GITHUB_TOKEN` / `GITHUB_REPO` env vars today.

## Decision Drivers

- Keep Octokit out of Domain and Application — `ca-layering` and the
  ports-and-adapters pattern of `IBoardReader` already in place.
- Minimise project-graph churn: the MVP-distribution promise (single
  `docker run`) discourages splintering Infrastructure into N projects
  per adapter.
- Decouple the manual-refresh trigger from CQRS infrastructure so the
  bus-factor risk recorded in ADR-003 cannot propagate to the refresh
  path.
- Determinism in tests: cadence and "trigger does not reset cadence"
  semantics must be assertable without flaky `Thread.Sleep` or
  wall-clock waits.
- YAGNI for the options story: one boot validation, two env vars, one
  clamp rule — no need for the typed-options pipeline today.

## Considered Options

### A — Poller location

- **A1.** Inside `AgentDashboard.TicketTracking.Infrastructure` under
  a `GitHub/` folder, alongside the existing `Boards/StubBoardReader.cs`.
- **A2.** A new sibling project `AgentDashboard.TicketTracking.Infrastructure.GitHub`,
  referenced separately from Web.

### B — On-demand trigger surface

- **B1.** A Cortex.Mediator notification published by the UI and
  consumed by an in-process notification handler that calls the
  poller.
- **B2.** A dedicated `IBoardRefreshTrigger` interface (Application
  port) with one `TriggerNowAsync(CancellationToken)` method, backed
  in Infrastructure by a singleton owning an internal
  `Channel<RefreshSignal>` read by the poller loop.
- **B3.** A raw `System.Threading.Channels.Channel<T>` injected
  everywhere a trigger is needed.

### C — Time abstraction

- **C1.** A custom `IClock` / `ISystemClock` interface with a fake
  implementation in the test project.
- **C2.** The framework `TimeProvider` (System.TimeProvider, .NET 8+)
  with `Microsoft.Extensions.TimeProvider.Testing.FakeTimeProvider` in
  the integration test project.

### D — Options binding

- **D1.** Inline `IConfiguration` reads in a small `GitHubPollingOptionsFactory`
  that throws `InvalidOperationException` on missing/empty env vars
  and clamps `POLL_INTERVAL_SECONDS` below 300s with a warning log.
- **D2.** Typed `IOptions<GitHubPollingOptions>` with
  `IValidateOptions<>` and `OptionsBuilder.Validate(...)`.

## Decision Outcome

- **A1** — poller in `Infrastructure/GitHub/`. No new project. The
  existing project graph (`Web -> Infrastructure -> Application ->
  Domain`) stays linear. Adapter co-location follows the precedent set
  by `Boards/StubBoardReader.cs`. Future slices (#8 ETag/retry, the
  SQLite write adapter) will land in sibling folders of the same
  project.
- **B2** — `IBoardRefreshTrigger`. The interface lives in
  `Application/Ports/`, the implementation in `Infrastructure/GitHub/`
  with a private `Channel.CreateBounded<RefreshSignal>(capacity: 1)`
  using `FullMode = DropWrite` so multiple concurrent clicks coalesce
  into one extra poll. The poller awaits `Task.WhenAny(timerTask,
  channelReadTask)` — when the channel signals, the loop polls
  immediately but does **not** advance the next scheduled deadline.
  This satisfies AC #8 of #5: "fires exactly once and the next
  scheduled poll still happens at the originally planned time."
  This **explicitly differentiates a control signal from a CQRS
  message**: notifications in Cortex.Mediator (cf. ADR-003) carry
  domain events or commands, not "please run this hosted-service work
  loop now". Smuggling control signals through the mediator would
  dilute its purpose and create one more code path to audit on every
  PR touching ingestion. The interface keeps the UI caller (#14)
  unaware of both Octokit and Cortex.Mediator.
- **C2** — `TimeProvider`. Registered as `TimeProvider.System` in
  production via the DI extension; overridden with `FakeTimeProvider`
  in integration tests via `WebApplicationFactory<Program>.ConfigureServices`.
  The poller computes deadlines as `_timeProvider.GetUtcNow() +
  interval` and uses `Task.Delay(interval, _timeProvider, ct)` so that
  `FakeTimeProvider.Advance(...)` deterministically unblocks the
  scheduled tick. No custom `IClock` is introduced — the framework
  owns this seam from .NET 8 onwards.
- **D1** — inline validation. `GitHubPollingOptionsFactory.FromConfiguration`
  reads `GITHUB_TOKEN`, `GITHUB_REPO`, `POLL_INTERVAL_SECONDS` from
  `IConfiguration`, validates the repo against the strict regex
  `^[A-Za-z0-9._-]+/[A-Za-z0-9._-]+$`, clamps the interval below 300s
  to 300s with a `LogWarning`, and defaults missing intervals to 600s.
  The factory is invoked from a singleton factory registered via the
  DI extension, so the validation runs at the host's first resolution
  of `GitHubPollingOptions` (which happens during hosted-service
  start). This lets `WebApplicationFactory.ConfigureAppConfiguration`
  inject overrides before validation runs — a constraint surfaced
  while wiring the host-boot fail-fast test.

### Structured log shape (note, not a separate ADR)

The poller emits one `LogInformation` per poll with named placeholders
`{repo}`, `{issue_count}`, `{duration_ms}`, `{next_poll_in_seconds}`,
implemented as a source-generated `[LoggerMessage]` partial method.
Placeholder casing follows the documented contract (snake_case, per
AC #3 of issue #5), which is the standard shape for log-aggregation
sinks. The CA1727 (Pascal-case placeholders) rule is lowered to
suggestion in `.editorconfig` to record that choice.

## Consequences

- **Good.** Linear project graph preserved. Refresh trigger is a thin
  Application port — substitutable in tests, swappable in
  implementation without touching the UI, free of Cortex.Mediator
  coupling. Cadence and "trigger does not reset cadence" semantics
  are deterministic in tests via `FakeTimeProvider`. The structured
  log contract is testable through the `RecordingLogger`'s
  per-entry `State` dictionary.
- **Neutral.** A second `IBoardRefreshTrigger` consumer will appear
  in #14 (manual-refresh Razor button). The interface is intentionally
  one-method-wide so callers depend only on what they need (ISP).
- **Bad.** The poller is `internal`, which forces `InternalsVisibleTo`
  for the new `Infrastructure.UnitTests` and
  `Infrastructure.IntegrationTests` projects. The trade-off (no
  accidental public surface on a hosted service) is worth the small
  boilerplate.
- **Bad.** Inline `IConfiguration` validation duplicates a tiny slice
  of what `IOptions<T>` will eventually provide. EPIC-3 will migrate.

## References

- `src/AgentDashboard.TicketTracking.Application/Ports/IGitHubIssuesClient.cs`
- `src/AgentDashboard.TicketTracking.Application/Ports/IBoardRefreshTrigger.cs`
- `src/AgentDashboard.TicketTracking.Application/GitHub/GitHubIssueRecord.cs`
- `src/AgentDashboard.TicketTracking.Infrastructure/GitHub/GitHubIssuesPoller.cs`
- `src/AgentDashboard.TicketTracking.Infrastructure/GitHub/BoardRefreshTrigger.cs`
- `src/AgentDashboard.TicketTracking.Infrastructure/GitHub/OctokitGitHubIssuesClient.cs`
- `src/AgentDashboard.TicketTracking.Infrastructure/GitHub/GitHubPollingOptions.cs`
- `src/AgentDashboard.TicketTracking.Infrastructure/GitHub/GitHubPollingOptionsFactory.cs`
- `src/AgentDashboard.TicketTracking.Infrastructure/DependencyInjection.cs`
- `tests/AgentDashboard.TicketTracking.Infrastructure.UnitTests/GitHub/GitHubPollingOptionsFactoryShould.cs`
- `tests/AgentDashboard.TicketTracking.Infrastructure.IntegrationTests/GitHub/GitHubIssuesPollerShould.cs`
- `tests/AgentDashboard.Web.Tests/Hosting/HostBootShould.cs`
- [ADR-001 — BoardSnapshot as a read-only projection, not an aggregate](./ADR-001-board-as-snapshot-not-aggregate.md)
- [ADR-002 — String constraints extracted into dedicated Value Objects](./ADR-002-string-constraints-as-value-objects.md)
- [ADR-003 — Cortex.Mediator over MediatR for in-process CQRS dispatch](./ADR-003-cortex-mediator-over-mediatr.md)
- Issue #5 — chore(ingestion): GitHub Issues polling worker skeleton
- Issue #8 — resilient polling (ETag, retry) — built on top of this slice
- Issue #14 — manual refresh button (UI consumer of `IBoardRefreshTrigger`)
