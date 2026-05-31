---
id: "ticket-tracking-write-side"
type: "technical"
owner: "architect"
status: "current"
updated: "2026-05-31"
links:
  - "architecture-overview"
  - "adr-005"
  - "adr-008"
  - "adr-009"
  - "adr-010"
  - "adr-011"
  - "adr-012"
answers:
  - "How is a Ticket persisted to SQLite?"
  - "Where does the persistence snapshot/mapping live, and why is it NOT a Domain snapshot?"
  - "How is the schema bootstrapped idempotently across concurrent constructions?"
  - "What are the upsert semantics (insert vs update, created_at_utc preservation)?"
  - "What happens when a persisted row fails to rehydrate?"
  - "How are ambiguous label-mapping situations surfaced and logged?"
  - "What is the Ticket's persistence identity / upsert key, and why is it not composite with the repo?"
  - "Where is the GitHub-issue→Ticket mapping tested, and why is there no isolated mapper unit suite?"
decided_in:
  - "#6"
---

# TicketTracking Write-Side

> **One-liner**: The EF Core code-first write path that persists a domain `Ticket` to the SQLite cache — its mapping, idempotent bootstrap, upsert semantics, corruption handling, and label-mapping warnings.
> **Links**: [[architecture-overview]] [[adr-005]] [[adr-010]] [[adr-011]] [[adr-008]] [[adr-009]] [[adr-012]] — the ADRs carry the full rationale; this node is the navigable summary.

## Context

The `TicketTracking` context is a downstream Conformist read model of GitHub ([[adr-008]]); SQLite is a local cache. Issue #6 delivered its **write side**: the poller maps GitHub issues to domain `Ticket`s and persists them. The original strategy (hand-rolled SQL, [[adr-009]]) was **superseded** by EF Core code-first ([[adr-010]], owner-mandated stack alignment). This node summarizes the resulting design; do not re-derive it — follow the linked ADRs for rationale.

Code lives in `src/AgentDashboard.TicketTracking.Infrastructure/Tickets/` (adapter + `Persistence/`) and the port in `src/AgentDashboard.TicketTracking.Application/Ports/ITicketWriteRepository.cs`.

## Decision / Specification

### Ticket identity / schema key — `GitHubIssueNumber` alone (#6, [[adr-005]] alignment)

| Aspect | Decision |
|---|---|
| Domain identity | `Ticket`'s identity is `GitHubIssueNumber` **alone** — not the composite `(GitHubRepository, GitHubIssueNumber)` |
| `GitHubRepository` VO | **deleted** from `TicketTracking.Domain` — the repo is a v1.0 Infrastructure deployment detail ([[adr-005]]), already carried by `GitHubPollingOptions`; a second Domain carrier was redundant per YAGNI |
| Table `tickets` PK | `github_issue_number` alone (was the composite `(repo, github_issue_number)`) |
| `repo` column | **dropped**, with its index — read by no consumer; `github_url` already records issue provenance |
| Multi-repo identity | deferred to #29 (clean-slate design once v1.0 is DONE) |

This aligns the code with [[adr-005]] (repo = Infrastructure deployment constant) and amends both [[adr-008]] (identity) and [[adr-010]] (PK + upsert key). The bounded context's category is unchanged: `TicketTracking` stays a downstream Conformist read model ([[adr-008]]).

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
| Upsert | find-then-add-or-update on key `GitHubIssueNumber` alone ([[adr-005]] alignment, #6) |
| Invariant on update | **`created_at_utc` is preserved** (only mutable fields updated) |

### Failure & ambiguity handling

| Situation | Behavior | Reference |
|---|---|---|
| Row fails to rehydrate (`ToTicket` parse error) | throw `CorruptedTicketRowException` naming the **column** + `#<number>` key; **does not leak the raw value** | [[adr-010]] |
| Multiple `status:*` labels / unrecognized label | `GitHubIssueToTicketMapper.Map` (Application) returns `MappingResult` = `Ticket` + `MappingWarning[]`; the row is still produced | [[adr-011]] |
| Warning surfacing | Infrastructure poller (`GitHubIssuesPoller`) logs each warning via sanitized `LoggerMessage` **EventId 202** | [[adr-011]] |
| Hyphenated `status:*` labels | parse fixed (e.g. `status:in-development`) | #6 |

### How the mapping is tested ([[adr-012]])

`GitHubIssueToTicketMapper` is a single-consumer, never-reused mapper (its only production
caller is `GitHubIssuesPoller`). Its behavior is verified **at the slice boundary**, in
`GitHubIssuesPollerTests`, asserting the observable outcome of the slice (warning logged,
SQLite row persisted), with a minimum representative `[Theory]` set of cases that could
break the mapping. AC3/AC4/AC10 + happy-path warning+mapping coverage was moved there in
PR #44, and the isolated `GitHubIssueToTicketMapperTests` suite was **removed** (it was
surtest — duplicate coverage that froze the mapper's internal structure). No AC was lost.
See [[adr-012]] for the rule and rationale; the read-side equivalent (`GitHubBoardMapper`)
is tracked in #45.

## Consequences / Constraints

- **MUST**: keep `ITicketWriteRepository` EF-unaware; all EF Core usage stays inside `SqliteTicketWriteRepository`/`Persistence/`.
- **MUST**: preserve `created_at_utc` on update; never reset it on re-ingest.
- **MUST**: surface label ambiguity as `MappingWarning` data — never throw, never silently drop ([[adr-011]]).
- **MUST NOT**: add a Domain persistence snapshot or a standalone mapper — `TicketRow` self-maps (the [[adr-010]] deviation from `ca-snapshot`).
- **MUST NOT**: add EF migrations or the `.Design` package — schema is `EnsureCreated()` only.
- **MUST NOT**: reintroduce an isolated `GitHubIssueToTicketMapperTests` suite — the mapping is verified at the poller slice boundary ([[adr-012]]).
- **Out of scope**: the read-side query path (Dapper, EPIC-2); board read projections ([[adr-001]]).

## Open questions / Gaps

- [ ] No automated check enforces the "no EF in Application" boundary (relies on review + layering discipline).
- [ ] `CorruptedTicketRowException` recovery policy (skip vs halt poll cycle) is implementation-current; not yet an explicit documented policy.
