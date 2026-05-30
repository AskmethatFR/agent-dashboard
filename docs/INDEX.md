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
| `adr-006` | Blazor.Redux async dispatch wiring for fire-and-forget reducers | current | 2026-05-27 | `[[architecture-overview]]` | `docs/adr/ADR-006-blazor-redux-async-dispatch.md` |
| `adr-008` | TicketTracking is a downstream Conformist read model of GitHub | current | 2026-05-27 | `[[architecture-overview]]`, `[[ticket-tracking-write-side]]` | `docs/adr/ADR-008-ticket-tracking-is-downstream-conformist-read-model.md` |
| `adr-009` | Database migration strategy — hand-rolled SQL for SQLite | superseded | 2026-05-30 | `[[adr-010]]` | `docs/adr/ADR-009-database-migration-strategy-sqlite.md` |
| `adr-010` | EF Core code-first for the SQLite write-side | current | 2026-05-30 | `[[ticket-tracking-write-side]]`, `[[adr-009]]` | `docs/adr/ADR-010-ef-core-code-first-write-side.md` |
| `adr-011` | Label-mapping warnings surfaced as data, logged in Infrastructure | current | 2026-05-30 | `[[ticket-tracking-write-side]]` | `docs/adr/ADR-011-label-mapping-warnings-as-data.md` |
| `architecture-overview` | System shape, bounded contexts, layers | current | 2026-05-30 | `[[ticket-tracking-write-side]]`, `[[adr-003]]`, `[[adr-008]]`, `[[adr-010]]` | `docs/technical/architecture.md` |
| `ticket-tracking-write-side` | TicketTracking write-side — EF Core code-first persistence | current | 2026-05-30 | `[[architecture-overview]]`, `[[adr-008]]`, `[[adr-009]]`, `[[adr-010]]`, `[[adr-011]]` | `docs/technical/ticket-tracking-write-side.md` |

## Functional nodes (owner: pm)

| ID | Title | Status | Updated | Links | Path |
|---|---|---|---|---|---|
| `feature-catalog` | All capabilities → behavior → acceptance | current | 2026-05-30 | `[[ticket-ingestion-acceptance]]`, `[[glossary]]` | `docs/functional/features.md` |
| `glossary` | Ubiquitous language — TicketTracking domain terms | current | 2026-05-30 | `[[feature-catalog]]`, `[[ticket-ingestion-acceptance]]`, `[[ticket-tracking-write-side]]`, `[[adr-008]]` | `docs/functional/glossary.md` |
| `ticket-ingestion-acceptance` | GitHub Issues → Ticket write model — acceptance (AC1..AC10) | current | 2026-05-30 | `[[feature-catalog]]`, `[[glossary]]`, `[[ticket-tracking-write-side]]`, `[[adr-011]]`, `[[adr-010]]` | `docs/functional/ticket-ingestion-acceptance.md` |

## Graph health (maintained by the owners at end of each cycle)

- [x] Every registered node has a row here; every row points to an existing file.
- [x] No dangling `[[id]]` edge (every referenced ID exists as a row).
- [x] No orphan node (every node is reachable from `architecture-overview` or `feature-catalog`).
- [x] `Updated` reflects the last cycle that touched the node.

> **Coverage note (2026-05-30, #6):** This cycle bootstrapped the graph scoped to the TicketTracking write-side (PR #44). ADR-002, ADR-004, ADR-005, ADR-007 exist under `docs/adr/` but are not yet registered as nodes — backfill them as future cycles touch their areas.
