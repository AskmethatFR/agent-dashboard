---
id: "architecture-overview"
type: "technical"
owner: "architect"
status: "current"
updated: "2026-05-30"
links:
  - "ticket-tracking-write-side"
  - "adr-003"
  - "adr-008"
  - "adr-010"
answers:
  - "What layers does the solution have and which way do dependencies point?"
  - "What bounded contexts exist and what is their relationship to GitHub?"
  - "Which technologies live in which layer (EF Core, Dapper, Octokit, Blazor)?"
  - "Where do persistence concerns live and why can't they leak into Domain/Application?"
decided_in:
  - "#6"
---

# Architecture Overview

> **One-liner**: The system shape — Clean Architecture layers, the `TicketTracking` bounded context, and which technology is allowed in which layer.
> **Links**: [[ticket-tracking-write-side]] [[adr-003]] [[adr-008]] [[adr-010]] — follow these for the detailed decisions.

## Context

`agent-dashboard` is a read-only observability cockpit for a 6-agent engineering team that collaborates through GitHub Issues. GitHub is the system of record; the dashboard maintains a **local SQLite cache** it polls and projects. Authoritative scope lives in `docs/mvp-brief.md`.

The codebase is organized by **Clean Architecture** layers inside a single bounded context (`TicketTracking`). This node is the spine of the technical graph: it states the layer rules and the technology-placement contract every other technical node and ADR depends on. The detailed write-side design delivered by issue #6 lives in [[ticket-tracking-write-side]].

## Decision / Specification

### Layers (dependency direction points inward — Domain knows nothing)

| Layer | Project | Knows about | Holds |
|---|---|---|---|
| Domain | `AgentDashboard.TicketTracking.Domain` | nothing | Entities, Value Objects, marker interfaces (`IEntity`, `IAggregateRoot`), `Ticket`, snapshots as read projections |
| Application | `AgentDashboard.TicketTracking.Application` | Domain | Use cases, CQRS queries, **ports** (`ITicketWriteRepository`, `IBoardReader`, `IGitHubIssuesClient`...), `GitHubIssueToTicketMapper` |
| Infrastructure | `AgentDashboard.TicketTracking.Infrastructure` | Application + Domain | EF Core write adapter, Dapper read adapter, Octokit client, poller, persistence POCOs |
| Web | `AgentDashboard.Web` | composes all | Blazor Server host, `Blazor.Redux` store, DI composition root |

**MUST**: dependencies only ever point inward. A port is defined in Application and implemented in Infrastructure (dependency inversion).

### Bounded contexts

| Context | Relationship to GitHub | Notes |
|---|---|---|
| `TicketTracking` | **Downstream Conformist read model** of GitHub ([[adr-008]]) | Categorically aggregate-less; SQLite is a cache, not the system of record. The only context in the MVP. |

### Technology placement contract

| Technology | Allowed only in | Decided in |
|---|---|---|
| EF Core (code-first) — **write-side** | Infrastructure | [[adr-010]] |
| Dapper — **read-side** | Infrastructure | EPIC-2 (not yet delivered) |
| Octokit (GitHub API) | Infrastructure | [[adr-008]] |
| `Cortex.Mediator` (CQRS dispatch) | Application + Web | [[adr-003]] |
| `Blazor.Redux` (UI state) | Web | [[adr-006]] |
| SQLite (single file under `DATA_PATH`) | Infrastructure | [[adr-009]] / [[adr-010]] |

## Consequences / Constraints

- **MUST**: keep ports in Application EF-unaware; EF Core types never appear in Application or Domain signatures.
- **MUST**: persist only via the write-side adapter; reads go through Dapper-backed read models (EPIC-2), never EF queries in Razor.
- **MUST NOT**: introduce a second persistence engine, message broker, or inbound webhook — the single-`docker run` promise is non-negotiable (`docs/mvp-brief.md`).
- **Out of scope**: the read-side query path (EPIC-2), additional bounded contexts (none in MVP).

## Open questions / Gaps

- [ ] Read-side (Dapper) adapter design is not yet documented — to be captured when EPIC-2 lands.
- [ ] Web/Blazor composition details (store wiring) are only partially covered by [[adr-006]].
