# ADR-001: BoardSnapshot as a read-only projection, not an aggregate

- Status: Accepted
- Date: 2026-05-21
- Deciders: Architect (slice 1.5 of issue #10)
- Context tags: ddd, aggregate, read-model, projection

## Context and Problem Statement

`agent-dashboard` is an observability cockpit for an agentic team. It is
**read-only by design** (see CLAUDE.md §1) — agents auto-coordinate via
GitHub Issues; the dashboard displays the board, it never mutates ticket
state. State changes happen out of band (via the agentic team workflow);
the dashboard only projects them for human consumption.

In slice 1 of issue #10 the domain layer introduced a `Board` record
holding columns, tickets, and agents, with referential-integrity checks
across the three collections. The naming "Board" implies an aggregate
root: a transactional consistency boundary owning lifecycle invariants
for the entities inside it.

This implication is false in our context. The dashboard never owns the
ticket lifecycle — GitHub Issues does. There is no "save the board"
transaction; there is only "render what GitHub currently says". A name
that suggests an aggregate root invites future contributors to attach
mutating behavior, which would silently violate the read-only design.

We must decide how to name and frame this type so that its read-only,
projection nature is unambiguous, both for humans and for the agents
reading the code via `ubiquitous-language`.

## Decision Drivers

- The dashboard is strictly read-only (CLAUDE.md §1). Naming must reflect
  this and discourage mutation-adding refactors.
- The type has no identity (`BoardId`), no behavior beyond construction,
  and value-equality semantics — it is by-value, like a DTO inside the
  domain.
- The referential-integrity checks remain useful: they guarantee that the
  projection is internally consistent at the moment it is materialised.
- We want a single source of truth for the polled state at a point in
  time; the name should convey "this is a snapshot at time T".
- Per `ddd-aggregate` (Vernon's rules): aggregates protect invariants
  over their state, are accessed only through the root, and define a
  transactional boundary. None of these apply here.

## Considered Options

- Option A: Keep the name `Board` and document via comments that it is
  not an aggregate.
- Option B: Rename to `BoardSnapshot` — a value type representing the
  board as observed at a point in time, with referential integrity
  enforced at construction.
- Option C: Move to the Application layer as a query result DTO and keep
  no domain type.

## Decision Outcome

Chosen option: **Option B — `BoardSnapshot`** — because it accurately
conveys the read-only, point-in-time nature of the type while keeping
the referential-integrity invariant inside the Domain layer where the
ubiquitous-language lives.

### Consequences

- Good: the name itself prevents the "let me add a mutating method to
  the Board" anti-pattern. New contributors reading `BoardSnapshot` will
  not look for behavior.
- Good: the type stays in the Domain layer. The referential-integrity
  rules (ticket→column, ticket→agent, ticket→co-agent, ticket→escalation
  target, ticket-id uniqueness) are domain knowledge and remain tested
  in `Domain.UnitTests`.
- Good: aligns with CLAUDE.md §1 "read-only by design" and with the
  team's ubiquitous language (a snapshot is what the polling layer
  produces every 10 minutes).
- Neutral: the type accepts an empty snapshot (`new BoardSnapshot([], [],
  [])` is valid). This matches reality — at startup the polling layer
  has not yet fetched anything; presenting an empty board is correct.
- Bad: callers built on top of `Board` (none yet in slice 1) must adopt
  the new name. Cost is zero today; non-zero if we had Application/Web
  callers. Best paid now, before slice 2.

## Pros and Cons of the Options

### Option A — Keep `Board` with documentation

- Good: zero churn.
- Bad: documentation rots faster than names. The next contributor sees
  `Board` and assumes aggregate root semantics.
- Bad: violates `ddd-ubiquitous-language` (names should be self-evident).

### Option B — Rename to `BoardSnapshot`

- Good: name carries the semantics.
- Good: aligns with `ca-snapshot` neurone (snapshot pattern for state
  externalisation without identity).
- Bad: small one-time rename cost.

### Option C — Move to Application layer as a query result DTO

- Good: arguably even more honest — the type is a read-model.
- Bad: forces the referential-integrity rules to live outside the
  Domain layer, which is where they were written and tested. Splitting
  them across layers loses cohesion.
- Bad: premature — slice 2 (Application/MediatR) is the right place to
  introduce a query-result DTO, possibly built **from** a
  `BoardSnapshot`. Doing it now would speculate about a layer not yet
  in scope (cc-yagni).

## References

- `src/AgentDashboard.TicketTracking.Domain/Boards/BoardSnapshot.cs`
- `tests/AgentDashboard.TicketTracking.Domain.UnitTests/Boards/BoardSnapshotTests.cs`
- CLAUDE.md §1 ("read-only by design"), §3 (repo layout)
- Knowledge base: `~/.claude/knowledge-base/ddd/aggregate.md`,
  `~/.claude/knowledge-base/clean-architecture/snapshot.md`,
  `~/.claude/knowledge-base/ddd/ubiquitous-language.md`
- GitHub Issue: #10
- Related ADRs: none yet (this is the first ADR)
