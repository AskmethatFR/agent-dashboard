---
id: "ticket-tracking-write-side"
type: "technical"
owner: "architect"
status: "current"
updated: "2026-05-30"
links:
  - "architecture-overview"
  - "adr-008"
  - "adr-009"
  - "adr-010"
  - "adr-011"
answers:
  - "How is a Ticket persisted to SQLite?"
  - "Where does the persistence snapshot/mapping live, and why is it NOT a Domain snapshot?"
  - "How is the schema bootstrapped idempotently across concurrent constructions?"
  - "What are the upsert semantics (insert vs update, created_at_utc preservation)?"
  - "What happens when a persisted row fails to rehydrate?"
  - "How are ambiguous label-mapping situations surfaced and logged?"
decided_in:
  - "#6"
---

# TicketTracking Write-Side

> **One-liner**: The EF Core code-first write path that persists a domain `Ticket` to the SQLite cache — its mapping, idempotent bootstrap, upsert semantics, corruption handling, and label-mapping warnings.
> **Links**: [[architecture-overview]] [[adr-010]] [[adr-011]] [[adr-008]] [[adr-009]] — the ADRs carry the full rationale; this node is the navigable summary.

## Context

The `TicketTracking` context is a downstream Conformist read model of GitHub ([[adr-008]]); SQLite is a local cache. Issue #6 delivered its **write side**: the poller maps GitHub issues to domain `Ticket`s and persists them. The original strategy (hand-rolled SQL, [[adr-009]]) was **superseded** by EF Core code-first ([[adr-010]], owner-mandated stack alignment). This node summarizes the resulting design; do not re-derive it — follow the linked ADRs for rationale.

Code lives in `src/AgentDashboard.TicketTracking.Infrastructure/Tickets/` (adapter + `Persistence/`) and the port in `src/AgentDashboard.TicketTracking.Application/Ports/ITicketWriteRepository.cs`.

## Decision / Specification

### Persistence stack and boundary

| Concern | Decision | Where | ADR |
|---|---|---|---|
| Write engine | EF Core **code-first**, no migrations, no `.Design` package | Infrastructure | [[adr-010]] |
| Schema creation | `Database.EnsureCreated()` (guarded — see bootstrap below) | Infrastructure | [[adr-010]] |
| Port | `ITicketWriteRepository.SaveAsync(Ticket, CancellationToken)` — EF-unaware | Application | [[adr-010]] |
| Adapter | `SqliteTicketWriteRepository` | Infrastructure | [[adr-010]] |
| Read engine | Dapper (unchanged, EPIC-2 — not in this PR) | Infrastructure | — |
| Storage location | single SQLite file under `DATA_PATH` (default `/data`); prod default unchanged | Infrastructure | [[adr-010]] |

### Mapping — the `ca-snapshot` deviation (deliberate, owner-decided)

The bidirectional Ticket↔row mapping is folded into the single Infrastructure POCO `TicketRow`:

| Direction | Member | Notes |
|---|---|---|
| Domain → row | `TicketRow.FromTicket(Ticket)` | mutable POCO so the EF change tracker can update in place; carries no invariants |
| Row → Domain | `TicketRow.ToTicket()` | rehydration; parse failures raise `CorruptedTicketRowException` |

**Deviation from the `ca-snapshot` neurone**: the neurone would place the persistence snapshot + mapping in Domain (a `Ticket.ToSnapshot()`/`FromSnapshot()` pair). Per owner directive ([[adr-010]] amendment, 2026-05-30) the persistence snapshot is treated as a pure **Infrastructure concern**. **There is no Domain snapshot type and no separate mapper** — `TicketRow` self-maps. This keeps Domain free of any persistence-shaped projection. (Note: `Domain/Tickets/TicketSnapshot.cs` and `Domain/Boards/BoardSnapshot.cs` are *read* projections for the board, unrelated to the write-side persistence snapshot — see [[adr-001]].)

### Idempotent schema bootstrap

`EnsureCreated()` alone is **not idempotent** across concurrently-constructed repositories. The guard stack ([[adr-010]]):

| Mechanism | Purpose |
|---|---|
| per-data-source static lock (`ConcurrentDictionary<string, object>`) | serialize schema creation across repository instances in-process |
| `IRelationalDatabaseCreator.HasTables()` guard | create tables only when absent |
| `PRAGMA wal_checkpoint(TRUNCATE)` | flush schema from the `-wal` sidecar into the main DB file so it is visible |
| WAL journal mode | concurrent read/write tolerance |

### DbContext lifetime and upsert semantics

| Aspect | Decision |
|---|---|
| Context factory | `PooledDbContextFactory<TicketTrackingDbContext>` |
| Context lifetime | one context created + disposed **per `SaveAsync`** — the singleton repository never shares a mutable change tracker |
| Upsert | find-then-add-or-update on key `(GitHubRepository, GitHubIssueNumber)` |
| Invariant on update | **`created_at_utc` is preserved** (only mutable fields updated) |

### Failure & ambiguity handling

| Situation | Behavior | Reference |
|---|---|---|
| Row fails to rehydrate (`ToTicket` parse error) | throw `CorruptedTicketRowException` naming the **column** + `(repo, #issue)` key; **does not leak the raw value** | [[adr-010]] |
| Multiple `status:*` labels / unrecognized label | `GitHubIssueToTicketMapper.Map` (Application) returns `MappingResult` = `Ticket` + `MappingWarning[]`; the row is still produced | [[adr-011]] |
| Warning surfacing | Infrastructure poller (`GitHubIssuesPoller`) logs each warning via sanitized `LoggerMessage` **EventId 202** | [[adr-011]] |
| Hyphenated `status:*` labels | parse fixed (e.g. `status:in-development`) | #6 |

## Consequences / Constraints

- **MUST**: keep `ITicketWriteRepository` EF-unaware; all EF Core usage stays inside `SqliteTicketWriteRepository`/`Persistence/`.
- **MUST**: preserve `created_at_utc` on update; never reset it on re-ingest.
- **MUST**: surface label ambiguity as `MappingWarning` data — never throw, never silently drop ([[adr-011]]).
- **MUST NOT**: add a Domain persistence snapshot or a standalone mapper — `TicketRow` self-maps (the [[adr-010]] deviation from `ca-snapshot`).
- **MUST NOT**: add EF migrations or the `.Design` package — schema is `EnsureCreated()` only.
- **Out of scope**: the read-side query path (Dapper, EPIC-2); board read projections ([[adr-001]]).

## Open questions / Gaps

- [ ] No automated check enforces the "no EF in Application" boundary (relies on review + layering discipline).
- [ ] `CorruptedTicketRowException` recovery policy (skip vs halt poll cycle) is implementation-current; not yet an explicit documented policy.
