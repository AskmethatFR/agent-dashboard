# Project Documentation Graph — INDEX

> **How to use this file**
> - **Human / AI looking for an answer**: scan the table, find the node whose Title/Type matches, open its `Path`, then follow its `[[links]]` edges.
> - **Agent about to implement / decide**: this index + the relevant node(s) are the **source of truth**. Do not re-decide what a node already settles. If no node covers your question, the graph is *silent* — decide, then add/extend a node (Architect for technical, PM for functional) and a row here in the same cycle.
> - **Owners**: `architect` owns every `technical` row, `pm` owns every `functional` row. Nobody edits the other lane's nodes without routing through that owner.

## Conventions

- **ID**: kebab-case, unique, stable (rename = breaking the graph). Referenced elsewhere as `[[id]]`.
- **Type**: `technical` (Architect-owned) or `functional` (PM-owned).
- **Status**: `draft` | `current` | `superseded` | `deprecated`.
- **Updated**: ISO date `YYYY-MM-DD` of last substantive change.
- **Links**: `[[id]]` of every node this one points to (dependencies, related decisions, feature↔ADR).

## Technical nodes (owner: architect)

| ID | Title | Status | Updated | Links | Path |
|---|---|---|---|---|---|
| `adr-001` | BoardSnapshot as a read-only projection, not an aggregate | current | 2026-05-27 | `[[architecture-overview]]` | `docs/adr/ADR-001-board-as-snapshot-not-aggregate.md` |
| `adr-003` | Cortex.Mediator over MediatR for in-process CQRS dispatch | current | 2026-05-27 | `[[architecture-overview]]` | `docs/adr/ADR-003-cortex-mediator-over-mediatr.md` |
| `adr-005` | v1.0 dogfooding scope and hardcoded GitHub target repository | current | 2026-05-22 | `[[architecture-overview]]`, `[[ticket-tracking-write-side]]` | `docs/adr/ADR-005-dogfooding-scope-and-hardcoded-repo.md` |
| `adr-006` | Blazor.Redux async dispatch wiring for fire-and-forget reducers | current | 2026-05-27 | `[[architecture-overview]]` | `docs/adr/ADR-006-blazor-redux-async-dispatch.md` |
| `adr-008` | TicketTracking is a downstream Conformist read model of GitHub | current | 2026-05-31 | `[[architecture-overview]]`, `[[ticket-tracking-write-side]]`, `[[adr-005]]` | `docs/adr/ADR-008-ticket-tracking-is-downstream-conformist-read-model.md` |
| `adr-009` | Database migration strategy — hand-rolled SQL for SQLite | superseded | 2026-05-30 | `[[adr-010]]` | `docs/adr/ADR-009-database-migration-strategy-sqlite.md` |
| `adr-010` | EF Core code-first for the SQLite write-side | current | 2026-05-31 | `[[ticket-tracking-write-side]]`, `[[adr-009]]`, `[[adr-005]]`, `[[adr-008]]` | `docs/adr/ADR-010-ef-core-code-first-write-side.md` |
| `adr-011` | Label-mapping warnings surfaced as data, logged in Infrastructure | current | 2026-05-31 | `[[ticket-tracking-write-side]]`, `[[adr-014]]` | `docs/adr/ADR-011-label-mapping-warnings-as-data.md` |
| `adr-012` | Test single-consumer mappers at the slice boundary, not in isolated unit suites | current | 2026-05-31 | `[[ticket-tracking-write-side]]`, `[[adr-011]]`, `[[adr-010]]`, `[[adr-013]]` | `docs/adr/ADR-012-test-mappers-at-slice-boundary.md` |
| `adr-013` | The read-side projection is a first-class Application use case behind a port | current | 2026-05-31 | `[[adr-012]]`, `[[adr-008]]`, `[[adr-001]]`, `[[mutation-testing-strategy]]`, `[[adr-014]]` | `docs/adr/ADR-013-read-side-projection-is-an-application-use-case.md` |
| `adr-014` | The read-side projection degrades per label and never throws on label input | current | 2026-05-31 | `[[adr-011]]`, `[[adr-013]]`, `[[adr-008]]` | `docs/adr/ADR-014-read-side-projection-degrades-on-malformed-labels.md` |
| `architecture-overview` | System shape, bounded contexts, layers | current | 2026-05-30 | `[[ticket-tracking-write-side]]`, `[[adr-003]]`, `[[adr-008]]`, `[[adr-010]]` | `docs/technical/architecture.md` |
| `mutation-testing-strategy` | Mutation testing (Stryker.NET) — per-bounded-context scoring strategy | current | 2026-05-31 | `[[adr-012]]`, `[[adr-013]]` | `docs/mutation-testing.md` |
| `ticket-tracking-write-side` | TicketTracking write-side — EF Core code-first persistence | current | 2026-05-31 | `[[architecture-overview]]`, `[[adr-005]]`, `[[adr-008]]`, `[[adr-009]]`, `[[adr-010]]`, `[[adr-011]]`, `[[adr-012]]` | `docs/technical/ticket-tracking-write-side.md` |
| `localization` | JSON-based localization with AspNetCore.Localizer.Json | current | 2026-06-04 | `[[architecture-overview]]` | `docs/technical/localization.md` |

## Functional nodes (owner: pm)

| ID | Title | Status | Updated | Links | Path |
|---|---|---|---|---|---|
| `feature-catalog` | All capabilities → behavior → acceptance | current | 2026-05-31 | `[[ticket-ingestion-acceptance]]`, `[[glossary]]` | `docs/functional/features.md` |
| `glossary` | Ubiquitous language — TicketTracking domain terms | current | 2026-05-31 | `[[feature-catalog]]`, `[[ticket-ingestion-acceptance]]`, `[[ticket-tracking-write-side]]`, `[[adr-008]]`, `[[adr-005]]` | `docs/functional/glossary.md` |
| `ticket-ingestion-acceptance` | GitHub Issues → Ticket write model — acceptance (AC1..AC10) | current | 2026-05-31 | `[[feature-catalog]]`, `[[glossary]]`, `[[ticket-tracking-write-side]]`, `[[adr-011]]`, `[[adr-010]]`, `[[adr-005]]`, `[[adr-008]]`, `[[adr-012]]` | `docs/functional/ticket-ingestion-acceptance.md` |

## Graph health (maintained by the owners at end of each cycle)

- [x] Every registered node has a row here; every row points to an existing file.
- [x] No dangling `[[id]]` edge (every referenced ID exists as a row).
- [x] No orphan node (every node is reachable from `architecture-overview` or `feature-catalog`).
- [x] `Updated` reflects the last cycle that touched the node.

> **Coverage note (2026-05-31, #6 follow-up):** The Ticket-identity shrink (`GitHubIssueNumber` alone, `GitHubRepository` VO deleted) amended ADR-008 + ADR-010 and added `[[adr-005]]` edges, so **ADR-005 is now registered** as a node. ADR-002, ADR-004, ADR-007 still exist under `docs/adr/` but are not yet registered — backfill them as future cycles touch their areas.
>
> **Coverage note (2026-05-31, #45 — read-side projection):** The read-side projection was promoted to an Application use case (`IBoardProjection.Project` / `BoardProjection`, `GitHubBoardMapper` deleted), closing the dominant Application mutation gap (64.20% → 85.80%). **ADR-013** is now registered and **amends ADR-012** (read-side slice boundary is the Application projection use case, not the Infrastructure reader). The pre-existing `docs/mutation-testing.md` prose is now registered as the **`mutation-testing-strategy`** node so ADRs can cite `[[mutation-testing-strategy]]`.
>
> **Coverage note (2026-05-31, #52 — read-side degradation):** The read-side projection now **degrades per label and never throws on label input** — a malformed/attacker-influenceable upstream label is skipped to its default and surfaced as a `ProjectionWarning`, so one poisoned label can no longer blank the board (OWASP A04, MEDIUM, resolved; residual out-of-range `retry` throw closed in the retry). `IBoardProjection.Project` now returns `BoardProjectionResult` (public Application contract change). **ADR-014** is now registered — it **applies ADR-011** (warnings-as-data, with a read-side-dedicated `ProjectionWarning`), **hardens ADR-013** (the projection use case), and is **built on ADR-008** (Conformist, attacker-influenceable labels). ADR-011 and ADR-013 gained a back-link `[[adr-014]]` (ADR-011 `Updated` bumped to 2026-05-31). Non-security follow-up: the incomplete fine-grained PAT redaction regex in `GitHubLogSanitizer` (pre-existing) — separate ticket.
