# ADR-011: Label-Mapping Warnings Surfaced as Data, Logged in Infrastructure

## Status
Accepted

## Context

`GitHubIssueToTicketMapper.Map(...)` (Application layer) resolves a GitHub issue's
labels onto a `Ticket`. Issue #6 acceptance criteria require the mapper to **surface
warnings** for two ambiguous-label situations while still producing a row:

- **AC3** — an issue carrying more than one recognized `status:*` label. Selection keeps
  the existing "latest in the state machine wins" rule; a warning must name the issue,
  the full conflicting `status:*` set, and the selected label.
- **AC4** — an issue carrying no recognized `status:*` label (including only-unrecognized
  values such as `status:foo`, which is an unparseable status and therefore *effectively
  missing*). The mapper keeps the existing default to `status:created`; a warning must
  name the issue.

AC5 (highest `retry:N` wins, default 0) has **no** warning requirement.

The mapper is a **pure static function** with no dependencies. The natural place to emit
log lines would be an injected `ILogger`, but that would force a logging dependency into
the Application csproj and convert the mapper to an instance with DI — both undesirable
(Clean Architecture keeps Application free of `Microsoft.Extensions.Logging`; CLAUDE.md §2
freezes the dependency set; KISS/YAGNI argue against DI conversion of a pure function).

## Decision

**Warnings are returned as data; Infrastructure logs them.**

- `GitHubIssueToTicketMapper.Map(...)` now returns a `MappingResult`
  (record: `Ticket Ticket`, `IReadOnlyList<MappingWarning> Warnings`) instead of a bare
  `Ticket`. This is a **public Application API contract change** — hence this ADR
  (CLAUDE.md §6).
- New Application types under `AgentDashboard.TicketTracking.Application/GitHub/`:
  - `MappingWarningKind` enum: `MultipleStatusLabels`, `MissingStatusLabel`.
  - `MappingWarning` — an immutable **value object** (equality by value, including the
    label sequence). It carries only taxonomy-bounded data: the issue number, the
    conflicting `status:*` label set, and the selected label. It is **not** an entity,
    aggregate, or `Entity<TId>` — no identity, no lifecycle. Constructed via the
    `MultipleStatusLabels(...)` / `MissingStatusLabel(...)` factory methods.
  - `MappingResult` — the `Ticket` + warnings tuple.
- The mapper stays **pure and static**. No `ILogger`, no DI, no logging package in the
  Application csproj.
- `GitHubIssuesPoller.PollOnceAsync` (Infrastructure) consumes `result.Ticket` (persisted
  as before) and drains `result.Warnings`, formatting each into a message and logging it
  through a new source-generated `LoggerMessage` method (`EventId = 202`,
  `Level = Warning`).

### Secret-safety
The warning data is taxonomy-bounded (issue number + `status:*`/label strings) and never
includes the raw issue record, body, URL, or any token. The assembled warning text is
nonetheless routed through the poller's existing token-redaction, refactored from
`SanitizeExceptionMessage(Exception)` into a shared `Sanitize(string)` reused by both the
failure path and the new warning path. `GitHubIssuesPollerTests.NeverLogTheGitHubToken`
stays green and now also covers the warning log path.

### Latent bug fixed in scope
The pre-existing label→enum resolution used `Enum.TryParse` on the raw suffix, which never
matched hyphenated labels (`status:in-development`, `status:in-qa`, `status:in-review`,
`status:awaiting-validation`) — the most common workflow states. Those silently fell
through to the `status:created` default. The shared `TryParseStatusValue` helper now strips
the hyphen before parsing, so selection and recognition work on the real label taxonomy
(`docs/labels.md`). Without this fix AC3 (`status:in-qa` + `status:done`) could not select
correctly. The read-side `GitHubBoardMapper` already handled hyphens via an explicit value
set; this aligns the write-side mapper with the same taxonomy.

## Consequences

- **Positive.** Application stays logging-free and the mapper stays a pure, easily unit-
  tested function (Chicago-school, real records + real VOs, table-driven). Warnings are
  asserted as structured data (kind + carried fields), decoupled from the poller's exact
  wording. The hyphenated-status bug is fixed, so status selection is now correct for the
  full taxonomy.
- **Negative / trade-off.** Every `Map(...)` caller must now unwrap `.Ticket`. There is one
  caller (the poller); the change is contained.
- **Message wording lives in Infrastructure.** The exact warning strings (EventId 202) are
  an Infrastructure formatting concern; tests at the Application layer never couple to them.

## Links
- Issue #6 (TicketTracking write model — remaining acceptance criteria AC3/AC4/AC5/AC10).
- Supersedes nothing. Related: ADR-005 (hardcoded repo), ADR-010 (EF write-side, untouched here).
