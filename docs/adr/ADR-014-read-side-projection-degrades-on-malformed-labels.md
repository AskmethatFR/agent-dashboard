# ADR-014: The read-side projection degrades per label and never throws on label input

## Status
Accepted

- **Date**: 2026-05-31
- **Deciders**: Project Manager + Project Architect
- **Tags**: clean-architecture, cqrs, read-model, security, resilience
- **Amends / relates**: [[adr-011]], [[adr-013]], [[adr-008]]

## Context

`BoardProjection` is the read-side Application use case behind `IBoardProjection.Project`
([[adr-013]]) — a pure function from upstream GitHub issue records to the read-side
`BoardSnapshot`. Labels are **Conformist** data sourced from upstream GitHub
([[adr-008]]); on a public repository they are **attacker-influenceable** (anyone can
open an issue and apply labels the dashboard then ingests).

The previous implementation validated every record **up-front** and **threw**
`InvalidOperationException` on the first malformed label. A single malformed label —
`status:foo`, `agent:bob`, `retry:4` — therefore aborted the **whole** projection:
`BoardProjection.Project` threw → `BoardSnapshotUpdater` threw → the read-model cache was
never updated → **the board blanked for every user**.

The Security review flagged this as **OWASP A04 — Insecure Design (MEDIUM)**: a
denial-of-service reachable by a *single* poisoned upstream label (DoS-by-one-bad-label).
The Security review of the **first implementation** of the fix found a **residual throw
path**: a `retry:N` label whose `N` parsed successfully but fell *outside*
`[0, Retry.MaxBeforeEscalation]` was rejected by `IsMalformed` (good) but the same
out-of-range value still flowed into `new Retry(...)`, which threw — re-opening the exact
DoS this ADR closes. That path was closed in the retry (see Decision).

## Decision

**The read-side projection degrades per label and never throws on label input.**

- For each record, a malformed label is **skipped**, its category **falls back** to the
  existing default, the record **still renders**, and the anomaly is surfaced as **data**:
  - `status:*` unrecognized → **CREATED**
  - `agent:*` unrecognized → **pm**
  - `co-agent:*` / `escalation-target:*` unrecognized → **null** → resolves to the
    record's `agentId`
  - `retry:N` malformed or out-of-range → **0**
- `IBoardProjection.Project` now returns
  **`BoardProjectionResult(BoardSnapshot Snapshot, IReadOnlyList<ProjectionWarning> Warnings)`**
  instead of a bare `BoardSnapshot`. This is a **public Application API contract change** —
  hence this ADR (CLAUDE.md §6).
- New Application type **`ProjectionWarning`** — an immutable, **taxonomy-bounded** value
  object carrying only the **issue number** and the **offending label** (never the raw
  record, body, URL, or any token). It is **dedicated to the read-side**, *not* the
  write-side `MappingWarning`: `MappingWarning` is shape-bound to the write-side
  `GitHubIssueToTicketMapper` ([[adr-011]]), and the two are **independent CQRS slices**
  ([[adr-013]]). This applies [[adr-011]]'s **warnings-as-data** pattern to the read-side
  with its own type rather than coupling the two slices.
- **Out-of-range parseable `retry` values are rejected in BOTH places**: `IsMalformed`
  (→ a `ProjectionWarning`) **and** `MapRetryLabel` (→ fallback `0`). No out-of-range
  integer can reach `new Retry(...)` — this closes the residual throw path the Security
  retry found.
- **The Application layer stays logging-free.** Infrastructure (`BoardSnapshotUpdater`)
  drains `result.Warnings` and logs them **sanitized** through the shared
  `GitHubLogSanitizer` (`EventId 210`). `IBoardSnapshotUpdater.Update` is **unchanged** —
  caching/scheduling remains a distinct responsibility from projecting (SRP, [[adr-013]]).

## Consequences

### Positive

- The board **can no longer be blanked by one poisoned upstream label** — OWASP A04
  resolved, including the residual out-of-range `retry` throw path.
- The projection's behavior is **fully asserted** by `BoardProjectionShould`: what was a
  throw path is now **asserted degradation behavior** (skip + fallback + warning),
  exercisable at the owning Application boundary ([[adr-013]]).
- Anomalies are **observable as structured data** and logged sanitized in Infrastructure,
  keeping Application free of `Microsoft.Extensions.Logging` ([[adr-011]] pattern).

### Negative / minor (accepted)

- A **new result type** (`BoardProjectionResult`) and a **new warning type**
  (`ProjectionWarning`).
- `IBoardProjection.Project`'s **return-type change** — a public Application contract
  change. Justified (resilience + A04), hence this ADR.

### Neutral

- **No runtime dependency added**; CPM and the single-`docker run` promise are untouched.
- **No UI surfacing of warnings this cycle** — log-only. Surfacing warnings in the board
  UI is deliberately deferred.
- A residual **non-security** item is routed to a follow-up ticket: the fine-grained PAT
  redaction regex in `GitHubLogSanitizer` is **incomplete** (pre-existing, not introduced
  here) — separate ticket.

## Links
- Applies [[adr-011]] (warnings-as-data pattern; this is the read-side counterpart with a
  dedicated `ProjectionWarning` type, since the slices are independent).
- Hardens [[adr-013]] (the read-side projection use case — its throw-on-malformed path
  becomes asserted degradation behavior).
- Built on [[adr-008]] (TicketTracking is a downstream Conformist read model of GitHub;
  labels are attacker-influenceable on a public repo).
- Issue #52 / EPIC-2.
- Code paths:
  - `src/AgentDashboard.TicketTracking.Application/Boards/BoardProjection.cs`
  - `src/AgentDashboard.TicketTracking.Application/Boards/BoardProjectionResult.cs`
  - `src/AgentDashboard.TicketTracking.Application/Boards/ProjectionWarning.cs`
  - `src/AgentDashboard.TicketTracking.Infrastructure/Boards/BoardSnapshotUpdater.cs`
  - `src/AgentDashboard.TicketTracking.Infrastructure/GitHub/GitHubLogSanitizer.cs`
