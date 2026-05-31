# ADR-013: The read-side projection is a first-class Application use case behind a port

## Status
Accepted

- **Date**: 2026-05-31
- **Deciders**: Project Manager (owner) + Project Architect
- **Tags**: clean-architecture, cqrs, read-model, testing, mutation-testing
- **Amends**: [[adr-012]]

## Context

`TicketTracking` is a downstream **Conformist, read-side-only** read model of GitHub
([[adr-008]]). That ADR named
`Application/GitHub/GitHubBoardMapper.MapToBoardSnapshot(records, now)` the **canonical
CQRS projection** of the bounded context: a pure function from upstream issue records to
the read-side `BoardSnapshot`.

But that projection was a **static class living in Application whose only production
caller was `BoardSnapshotUpdater` in Infrastructure**. Two problems followed:

1. **No Application-owned behavioral entry point.** The projection — the most
   business-meaningful read-side transformation in the codebase — had no port and no
   Application-level caller. It could only be reached *through* Infrastructure, fused
   with the cache-update concern. Its behavior was therefore only exercisable via
   integration tests, not at the layer that actually owns it.
2. **Misattributed test effectiveness.** Per [[mutation-testing-strategy]], the honest
   per-bounded-context Application mutation score sat at **64.20%**, and the residual gap
   was **concentrated in `GitHubBoardMapper.cs`** — precisely because the projection's
   coverage lived in `Infrastructure.IntegrationTests`, blind to the owning layer and
   coupled to the caching path.

This is the read-side counterpart of the boundary question [[adr-012]] settled for the
write-side. [[adr-012]] had *deferred* the read-side to issue #45 and provisionally
placed its boundary at the Infrastructure reader. Investigation under #45 showed that
boundary was wrong: the correct boundary is an Application use case that did not yet
exist.

## Decision

**The read-side projection is a first-class Application use case behind a port.**

Concretely:

- Introduce `IBoardProjection.Project(records, asOf)` — an Application **port** —
  implemented by `BoardProjection` (Application, `sealed`, **stateless**).
- `BoardProjection` **absorbs and replaces** `GitHubBoardMapper`, which is **deleted**.
  It references only the Domain (`BoardSnapshot` and its parts) and the Application
  `GitHubIssueRecord` — no Infrastructure dependency.
- `BoardSnapshotUpdater` is reduced to **project-then-cache**: it calls
  `IBoardProjection.Project(...)` and hands the result to the cache. It no longer holds
  any projection knowledge.
- `IBoardSnapshotUpdater` is **unchanged** — caching/scheduling is a distinct
  responsibility from projecting (SRP); the port split is preserved.
- The projection is **verified by a behavioral `[Theory]` at the Application boundary**
  (`BoardProjectionShould`), asserting the produced `BoardSnapshot` for the representative
  set of records — consistent with [[adr-012]]'s slice-boundary principle, now applied at
  the *correct* (Application) boundary.
- The isolated `GitHubBoardMapperTests` suite is **deleted** with **no acceptance
  criterion lost**.
- Mutation effectiveness is measured **per bounded context**; the Application target is
  **≥ 80%**, **achieved 85.80%** (up from the 64.20% honest baseline).

## Consequences

### Positive

- The projection is a **testable, mockable, Application-owned use case** sitting behind a
  port — reachable and assertable at the layer that owns it, not through Infrastructure.
- **Infrastructure holds no projection knowledge** — `BoardSnapshotUpdater` only projects
  then caches. The CQRS read-side responsibilities (project vs. materialize/cache) are
  cleanly separated ([[adr-001]] reinforced: the snapshot stays a replace-only read model).
- Mutation score is **attributed to the owning layer**, closing the dominant Application
  gap and raising the honest score to 85.80%.
- The projection can be **reshaped** (new columns, derived fields) behind a stable port
  without disturbing callers.

### Negative / minor (accepted)

- One **extra port** (`IBoardProjection`) and its **DI registration**. Justified: it is
  the seam that makes the use case Application-owned and independently verifiable.
- A **stateless instance** replaces a static class. Marginally more ceremony than a static
  method; justified by mockability and the dependency-inversion seam (a static call cannot
  be a port).

### Neutral

- **No runtime behavior change** — the projection produces the same `BoardSnapshot` for
  the same inputs; this is a structural relocation, not a functional one.
- CPM, the single-`docker run` promise, and the frozen dependency set are **untouched**.
- Stryker remains a **global tool**, not a package reference — see
  [[mutation-testing-strategy]].

## Links
- Amends [[adr-012]] (read-side slice boundary is the Application projection use case,
  not the Infrastructure reader).
- Builds on [[adr-008]] (which named `MapToBoardSnapshot` the canonical CQRS projection of
  this read-side-only bounded context).
- Reinforces [[adr-001]] (BoardSnapshot is a read-only projection, not an aggregate).
- References [[mutation-testing-strategy]] (per-BC scoring; Application ≥ 80%, achieved
  85.80%).
- Issue #45 / EPIC-2.
- Code paths:
  - `src/AgentDashboard.TicketTracking.Application/Boards/BoardProjection.cs`
  - `src/AgentDashboard.TicketTracking.Application/Ports/IBoardProjection.cs`
