# ADR-006: Blazor.Redux async dispatch wiring for fire-and-forget reducers

- Status: Accepted
- Date: 2026-05-23
- Deciders: Project Architect (EPIC-2 board wiring escalation)
- Context tags: architecture, state-management, blazor, testing, wiring

## Context and Problem Statement

EPIC-2 (#15 board components) wired `Blazor.Redux` 0.1.0 into `Home.razor`
and `TopBar/RefreshButton.razor` to drive a `LoadBoardAction` through a
`BoardSlice`. The action is handled by `LoadBoardAsyncReducer` which
calls `IMediator.SendQueryAsync<GetBoardQuery, BoardDto>(…)` via
Cortex.Mediator.

A second-Developer test refactor (replacing `FakeDispatcher` with the
real `Store` / `Dispatcher` / reducers per
`feedback_blazor_redux_tests_real_store.md`) was correctly written but
**five tests failed** — exposing a wiring bug that had been masked by
the previous fake-based tests. Reproduction: opening `/` showed the
`Loading…` placeholder forever, and clicking refresh never re-issued a
`GetBoardQuery`.

Decompilation of `Blazor.Redux.dll` 0.1.0 (in
`~/.nuget/packages/blazor.redux/0.1.0/lib/net10.0/`) reveals the root
cause:

```csharp
// Blazor.Redux.Dispatching.Dispatcher.Dispatch (sync, IDispatcher)
//   → ExecutePipeline → ActionStream.Publish + ApplyReducers(IReducer<,>)
//   → NEVER calls IAsyncReducer<,>.

// Blazor.Redux.Dispatching.AsyncDispatcher.DispatchAsync (async, IAsyncDispatcher)
//   → ExecutePipelineAsync → ActionStream.Publish + ApplySyncReducerIfExists
//   + ApplyAsyncReducers(IAsyncReducer<,>).
```

`IDispatcher.Dispatch` is intentionally **sync-only**. The lib ships a
**separate** `IAsyncDispatcher.DispatchAsync` (already registered by
`AddBlazorRedux`) whose responsibility is to run async reducers and
return a `Task` the caller can await.

Our `LoadBoardAction` is dispatched via the sync `IDispatcher`. Result:
the sync `LoadBoardReducer` flips `IsLoading = true`, the action is
published on the `ActionStream`, but `LoadBoardAsyncReducer.ReduceAsync`
— the only place the mediator query runs — is **never invoked**.

The fake-based tests masked this because `FakeDispatcher` recorded the
intent ("an action was dispatched") instead of exercising the real
pipeline.

## Decision Drivers

- Use the library API the way its author designed it — `IAsyncDispatcher`
  exists exactly for this case.
- Keep the codebase Chicago-school testable end-to-end with the real
  `Store` / `Dispatcher` / `AsyncDispatcher` / reducers wired
  (`feedback_blazor_redux_tests_real_store.md`).
- No new runtime dependency, no middleware, no custom effect — the lib
  already provides the right primitive.
- Preserve the Blazor Server interactive UX: the `RefreshButton` should
  remain responsive while the query is in flight.
- Minimise the diff: do not redesign actions, slices, or reducers.

## Considered Options

### Option A — Switch to `IAsyncDispatcher.DispatchAsync` (chosen)

- Inject `IAsyncDispatcher` instead of `IDispatcher` in `Home.razor` and
  `RefreshButton.razor`.
- Call `await Dispatcher.DispatchAsync<BoardSlice, LoadBoardAction>(new LoadBoardAction())`.
- In `Home.OnInitializedAsync`, await the dispatch — the page already
  subscribes to the slice, so the first render shows the loading state
  and the second the loaded board.
- In `RefreshButton.HandleClick`, await the dispatch inside the
  existing busy/cooldown logic. The button stays `aria-busy="true"`
  while the async reducer runs, then resets.
- Keep both `IReducer<BoardSlice, LoadBoardAction>` (LoadBoardReducer)
  and `IAsyncReducer<BoardSlice, LoadBoardAction>` (LoadBoardAsyncReducer)
  registered — `AsyncDispatcher.ExecutePipelineAsync` runs the sync one
  first (setting `IsLoading = true`), then the async one (which fetches
  the board and sets `IsLoading = false`).

### Option B — Wire an `IEffect` listening on the action stream

- Sync `Dispatch` already publishes to the `ActionStream`. We could
  implement `IEffect.Handle(IObservable<IAction>, IObservable<RootStateSnapshot>)`
  that filters `LoadBoardAction`, calls the mediator, and dispatches a
  `LoadBoardSuccessAction` / `LoadBoardFailureAction`.
- Rejected: adds Rx-style indirection for a single concern; doubles the
  number of action types we actually use; the `Success` / `Failure`
  actions are already present but unused — switching to async dispatch
  in option A also makes them unused (the async reducer owns the
  outcome), but option A removes them naturally in a follow-up
  `refactor` slice. Option B keeps them and grows the surface.

### Option C — Replace the async reducer with a sync one that fires-and-forgets

- Sync reducer kicks off `Task.Run(async () => …)` and discards the
  task. Rejected: exception-swallowing, no way to plumb cancellation
  through circuit lifetime, indistinguishable from a bug. Violates
  `cc-no-comments`/`cc-yagni` by replacing a working idiomatic
  primitive (`IAsyncReducer`) with a manual one.

### Option D — Custom hosted `EffectsPipeline` pump

- `Blazor.Redux.dll` ships no `IHostedService` (verified by string-scan
  of the DLL: zero occurrences of `AddHostedService` / `IHostedService`).
  The pipeline is started lazily by both `Dispatcher` and
  `AsyncDispatcher` ctors via `_effectsPipeline.EnsureStarted()`. So a
  hosted pump would duplicate behaviour the lib already handles.
  Rejected.

## Decision

**Option A — adopt `IAsyncDispatcher` for `LoadBoardAction`.**

`AddBlazorRedux(…)` already registers `IAsyncDispatcher → AsyncDispatcher`
(scoped) — verified by decompilation of
`Blazor.Redux.Extensions.ServiceCollectionExtensions.AddBlazorRedux`.
No DI change is required other than removing the now-duplicate
`services.AddScoped<IDispatcher, Dispatcher>()` line in `Program.cs`,
which was a leftover from the initial wiring and is redundant with the
library-provided registration.

## Concrete Changes (file by file)

### `src/AgentDashboard.Web/Program.cs`

- **Remove** the manual `services.AddScoped<IDispatcher, Dispatcher>()`
  registration (duplicate of what `AddBlazorRedux` already registers;
  also registers `IAsyncDispatcher` for free).
- **Keep** all explicit reducer registrations
  (`IReducer<BoardSlice, …>` and `IAsyncReducer<BoardSlice, LoadBoardAction>`),
  because `AddBlazorRedux(...).AddReducers(options.Assembly)` defaults
  to `Assembly.GetCallingAssembly()` which resolves to `Blazor.Redux.dll`
  itself (not our Web assembly) and therefore finds zero of our
  reducers. The explicit registrations stay.
- The `BlazorReduxOption` construction itself is unchanged.

### `src/AgentDashboard.Web/Components/Pages/Home.razor`

- Replace `@inject IDispatcher Dispatcher` with `@inject IAsyncDispatcher Dispatcher`.
- Replace `OnInitialized` (sync) with `OnInitializedAsync` (async) so
  the page can await the dispatch.
- Replace `Dispatcher.Dispatch<BoardSlice, LoadBoardAction>(new LoadBoardAction())`
  with `await Dispatcher.DispatchAsync<BoardSlice, LoadBoardAction>(new LoadBoardAction())`
  in both `OnInitializedAsync` and the `Retry` button's `LoadBoard`
  handler (also becomes `async Task LoadBoard()`).
- The `Store.ObserveSlice<BoardSlice>().Subscribe(…)` subscription stays
  exactly as it is — the async reducer publishes to the slice, the
  subscription fires, `InvokeAsync(StateHasChanged)` updates the UI.

### `src/AgentDashboard.Web/Components/Layout/TopBar/RefreshButton.razor`

- Replace `@inject IDispatcher Dispatcher` with `@inject IAsyncDispatcher Dispatcher`.
- Change `HandleClick` from `void` to `async Task HandleClick()`.
- Inside the busy/cooldown guard, replace the sync `Dispatch` call with
  `await Dispatcher.DispatchAsync<BoardSlice, LoadBoardAction>(new LoadBoardAction())`.
- Keep the existing `_isBusy` / `_cooldownUntil` / `TimeProvider` logic
  unchanged. The `try { … } finally { _isBusy = false; }` block now wraps
  an awaited dispatch — `aria-busy` flips to `true` for the duration of
  the query and back to `false` after.
- Do **not** reintroduce the `try { … } catch (Exception) { }` swallow that
  the second Dev correctly removed: `LoadBoardAsyncReducer` already
  catches exceptions internally and writes them to `slice.Error`.

### `src/AgentDashboard.Web/Store/BoardReducers.cs`

- No change. `LoadBoardAsyncReducer.ReduceAsync` is correct as written;
  it just needed to be reached.

### Tests

- `tests/AgentDashboard.Web.Tests/Pages/HomeShould.cs` — no change. The
  `BuildContext` already wires the real Store/Dispatcher/Reducers and
  the in-memory infra (`AddTicketTrackingApplication` +
  `AddTicketTrackingInfrastructure`). Tests observe column labels and
  counts via `WaitForState(…)` which already accommodates an async
  dispatch.
- `tests/AgentDashboard.Web.Tests/Components/Layout/TopBar/RefreshButtonShould.cs`
  — no change required. `CountingBoardReader` wraps `StubBoardReader`
  via `IBoardReader`, and `Click()` followed by
  `WaitForState(() => reader.CallCount == 1, …)` will now see the call
  because the async reducer actually runs.
- `tests/AgentDashboard.Web.Tests/Components/Layout/TopBar/TopBarShould.cs`
  — no change required.
- No new test file. No `FakeDispatcher` reintroduced.

## Acceptance Criteria (Definition of Done for this slice)

1. `dotnet build AgentDashboard.slnx` green, treat-warnings-as-errors
   passing.
2. `dotnet test AgentDashboard.slnx` green — full suite, including the
   five previously-failing tests in `RefreshButtonShould` /
   `TopBarShould` / `HomeShould`.
3. `dotnet run --project src/AgentDashboard.Web` works locally; opening
   `/` shows the 7 columns (Created, Specified, In Development, In
   Review, In Qa, Awaiting Validation, Done) with seeded counts
   `[2, 1, 2, 1, 1, 0, 2]`.
4. Clicking the `RefreshButton` triggers a fresh
   `IBoardReader.GetCurrentAsync` call (observable in tests via the
   `CountingBoardReader`).
5. No `FakeDispatcher` exists in the test project (already removed by
   the second Dev — must not be reintroduced).
6. Conventional Commit (`fix(team-board): …` or similar) on the
   `feat/issue-15-team-board-components` branch, trailer
   `Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>`
   present.
7. No new package added to `Directory.Packages.props`.
8. No silenced warnings, no XML-doc bloat, no comments unless the
   *why* is non-obvious (`cc-no-comments`).

## Out of Scope (explicitly deferred)

- Error UI rendering from `slice.Error` (the page currently shows the
  raw error string + a Retry button — sufficient for this slice; richer
  UI is EPIC-2 follow-up).
- Removing the unused `LoadBoardSuccessAction` / `LoadBoardFailureAction`
  and their sync reducers. They are no longer reachable via the chosen
  flow (the async reducer owns success and failure). A clean-up
  `refactor` slice will follow, immediately after a follow-up `feat`
  slice that introduces another action — never as a standalone
  refactor train (per `use-case-driven-design`).
- AgentChip integration into `Home.razor` (slice 15.x).
- Polling-driven board refresh (EPIC-1 cadence already lands a
  `RefreshNow` trigger via `IBoardRefreshTrigger`; the action stream
  hook-up is a separate slice).
- SignalR push or any change to the live-update mechanism.
- Removing the explicit reducer registrations in `Program.cs` in favour
  of a hand-rolled assembly scan (the explicit list is fine for a
  three-reducer slice; revisit when reducer count > ~6).

## Notes for the implementing Developer

- The `BlazorReduxOption.Assembly` property defaults to `null`; the
  library then uses `Assembly.GetCallingAssembly()` which resolves to
  `Blazor.Redux.dll` itself when called from inside `AddBlazorRedux`.
  This is why the auto-scan of reducers does not find ours. Setting
  `Assembly = typeof(BoardSlice).Assembly` in `BlazorReduxOption` would
  enable auto-registration, but the explicit list is clearer for a
  small slice and avoids reflection at startup. Leave the explicit
  registrations as-is for this fix.
- `AsyncDispatcher.ExecutePipelineAsync` runs the sync reducer **first**
  (so `IsLoading = true` flips before the await), then awaits all
  registered `IAsyncReducer`s sequentially. The slice update happens
  once at the end of the pipeline — the `Loading…` flash will only be
  visible if any sync reducer in the chain runs before the async one.
  This matches the existing UX.
- Do not introduce `ConfigureAwait(false)` in Razor components: Blazor
  Server's `SynchronizationContext` is needed for `StateHasChanged` to
  marshal correctly. `ConfigureAwait(false)` inside `LoadBoardAsyncReducer`
  is fine (and already there) because the reducer does not touch UI.
