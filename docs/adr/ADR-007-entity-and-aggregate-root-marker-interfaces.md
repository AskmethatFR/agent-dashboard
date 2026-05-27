# ADR-007: `IEntity<TId>` and `IAggregateRoot<TId>` marker interfaces in the Domain layer

- Status: Accepted
- Date: 2026-05-27
- Deciders: Project owner (PM/orchestrator role)
- Context tags: ddd, entity, aggregate-root, type-system, marker-interface

## Context and Problem Statement

Until this ADR, every Domain type was declared `public sealed record`
with no marker interface, base class, namespace sub-folder, or naming
suffix to distinguish DDD natures. A reader of the Domain project
could not tell at a glance whether `Agent`, `BoardColumn`,
`BoardSnapshot`, or `TicketSnapshot` were Value Objects, Entities,
Aggregate Roots, or Snapshots — they all looked structurally
identical.

The project owner asked for a way to make the DDD nature of each type
**visible at the type system level**, motivated by the doctrinal
principle that *"an Aggregate Root is also an Entity"* — a relationship
that folders and namespaces cannot express because they do not
inherit.

The Project Architect was consulted twice. In both passes the
Architect recommended **against** introducing marker interfaces,
citing three concerns:

1. `IEntity<TId>` carrying an identity-equality contract would *lie*
   on a `sealed record`, because records generate by-value equality
   from the C# compiler, not identity-based equality.
2. `IAggregateRoot` applied to any current type would *contradict
   ADR-001*, which formally states the dashboard is read-only and has
   no aggregate roots today.
3. The infrastructure has *zero callers* in the current codebase —
   `cc-yagni` violation.

The project owner reviewed the Architect's argumentation and elected
to introduce the interfaces anyway, with a constrained scope that
**minimises** each concern.

## Decision Drivers

- **Visible DDD semantics at the type-system level**: a reader of
  `Agent : IEntity<AgentId>` knows immediately that `Agent` is
  conceptually an Entity, and a future `: IAggregateRoot<XId>` would
  signal an aggregate root.
- **Honest interface contract**: the interface must not promise
  semantics it cannot enforce on the current type shape (records).
- **ADR-001 must remain valid**: no current type may receive
  `IAggregateRoot`, because that would contradict ADR-001's "no
  aggregates today" stance. Introducing the interface without an
  implementer reserves the slot for a future supersession of ADR-001,
  without breaking it today.
- **Snapshots are categorically excluded** from the Entity /
  AggregateRoot family. A snapshot is a read-only projection at a
  point in time; it has no identity-based equality, no lifecycle, no
  transactional boundary, and tagging it with these interfaces would
  mislabel its semantic.
- **No surgical risk on existing records**: the interfaces must be
  implementable on `sealed record` without changing equality, without
  unsealing, and without introducing a base class.

## Considered Options

- **Option A — No marker interfaces.** Architect's recommendation. The
  DDD semantic is carried by naming (`*Snapshot` suffix) and by ADRs.
  Zero new files. Strict `cc-yagni`. Rejected by the project owner on
  the grounds that the doctrinal hierarchy *"AggregateRoot is-a
  Entity"* is not expressible in code without an inheritance
  relationship.
- **Option B — `Entity<TId>` abstract base class with identity-based
  equality.** Requires unsealing all candidate records (or converting
  to classes) and overriding `Equals` / `GetHashCode`. Would void
  ADR-002 §"`Ticket` equality stays structural" (before the rename to
  `TicketSnapshot` made that paragraph moot). Heavy surgical change
  for no current payoff. Rejected.
- **Option C — Marker interfaces typed by Id, no equality contract,
  no implementer for `IAggregateRoot` today.** Two empty-bodied
  interfaces in `Domain/Abstractions/` carrying only `TId Id { get; }`.
  `Agent` and `BoardColumn` implement `IEntity<TId>`. `IAggregateRoot`
  is created but no current type implements it (preserving ADR-001).

## Decision Outcome

Chosen option: **Option C — typed marker interfaces with no
implementer for `IAggregateRoot` today**.

### Interfaces introduced

```csharp
// src/AgentDashboard.TicketTracking.Domain/Abstractions/IEntity.cs
namespace AgentDashboard.TicketTracking.Domain.Abstractions;

public interface IEntity<out TId> where TId : notnull
{
    TId Id { get; }
}

// src/AgentDashboard.TicketTracking.Domain/Abstractions/IAggregateRoot.cs
namespace AgentDashboard.TicketTracking.Domain.Abstractions;

public interface IAggregateRoot<out TId> : IEntity<TId> where TId : notnull
{
}
```

### Applied to

| Type | Interface | Justification |
|---|---|---|
| `Agent` | `IEntity<AgentId>` | Has identity (`AgentId`); not a snapshot |
| `BoardColumn` | `IEntity<BoardColumnId>` | Has identity (`BoardColumnId`); not a snapshot |

### NOT applied to (and the rule that excludes them)

| Type | Reason |
|---|---|
| `BoardSnapshot` | Snapshot. Never receives Entity / AggregateRoot markers. |
| `TicketSnapshot` | Snapshot, despite carrying `TicketId`. Snapshots are not Entities. |

**Hard rule (non-negotiable)**: a `*Snapshot` type — or any type whose
nature is "a read-only projection at a point in time" — MUST NOT
implement `IEntity<TId>`, `IAggregateRoot<TId>`, or any future
identity-bearing DDD marker. Snapshots have by-value equality, no
lifecycle, no transactional boundary, and no command handler.

### Honest interface contract — addressing the Architect's "lie" concern

The Architect's first concern was that `IEntity<TId>` would *lie* on a
`sealed record` because records have by-value equality, not
identity-based equality. This ADR addresses the concern by **narrowing
the interface contract**:

`IEntity<TId>` promises one thing: the existence of `TId Id { get; }`.
It does **not** promise identity-based equality, an `IEquatable<T>`
contract, or any lifecycle method. A `sealed record Agent : IEntity<AgentId>`
satisfies the contract honestly — it exposes `Id`. The record's
by-value equality remains its native behaviour and is **not** contradicted
by the interface.

A consumer that needs identity-based comparison must call `.Id.Equals(...)`
explicitly. A consumer that wants snapshot-style equality (two `Agent`s
with same `Id` and different `Name` are unequal) gets that from the
record default. Both semantics coexist deliberately.

If a future ADR introduces an identity-based `Entity<TId>` *base class*
overriding `Equals`, that decision will supersede the relevant
paragraphs here and require unsealing the records — that change is
explicitly **out of scope** for ADR-007.

### `IAggregateRoot` reserved, ADR-001 preserved

`IAggregateRoot<TId>` is created but has **zero implementers in the
current codebase**. ADR-001 ("BoardSnapshot is not an aggregate")
remains valid: no current type is tagged as an aggregate root.
Introducing the interface today does not assert that aggregates exist;
it reserves the type-system slot for a future supersession of ADR-001
(when, and only when, the dashboard gains write-side command handlers
and transactional boundaries — which is currently out of scope per
CLAUDE.md §1).

When that day comes, a new ADR superseding ADR-001 must justify
*which* type becomes an aggregate root, *why* (which command handler
loads / mutates / saves it), and *how* invariants are enforced. Until
then, `IAggregateRoot<TId>` exists in code as a documented slot — not
as a marker to be applied opportunistically.

### Why this is not a `cc-yagni` violation in spirit

The Architect's third concern was YAGNI. The project owner's
counter-argument, accepted here: the interfaces serve a *current*
purpose — making the DDD nature of `Agent` and `BoardColumn` visible at
the type-system level for human readers and for future tooling
(analyzer rules, source generators, EF Core configurations scanning by
interface). The "no current caller" critique applies to consumers of
the contract at runtime; it does not apply to the interface's role as
*declarative documentation that the C# compiler verifies*.

`IAggregateRoot<TId>` is closer to YAGNI territory because it has no
current implementer. It is accepted here as a small, low-cost
investment in semantic completeness — the doctrinal hierarchy
*"AggregateRoot is-a Entity"* is now expressed in code, even if it
currently has no concrete target. Removing it later is a single-file
deletion if it proves useless; keeping it costs nothing.

## Pros and Cons of the Options

### Option A — No marker interfaces

- Good: zero new abstraction, strictest `cc-yagni`.
- Good: aligns with Architect's recommendation and the read-only
  constraint.
- Bad: the doctrinal hierarchy *"AggregateRoot is-a Entity"* is not
  expressible in code; only in ADRs and naming.
- Bad: future tooling (analyzers, EF Core configs) cannot dispatch on
  Entity / AggregateRoot category.

### Option B — `Entity<TId>` base class with identity-based equality

- Good: enforces identity-equality contract honestly.
- Bad: requires unsealing all candidate records → cascading refactor.
- Bad: voids ADR-002 §"`Ticket` equality stays structural" (now moot
  after the `TicketSnapshot` rename, but the principle remains for
  `Agent` and `BoardColumn`).
- Bad: introduces a base class with no current behaviour (anemic base
  smell).

### Option C — Typed marker interfaces (chosen)

- Good: makes DDD nature visible at the type-system level without
  changing equality semantics.
- Good: `IAggregateRoot<TId>` has no implementer → ADR-001 preserved.
- Good: ergonomic for future EF Core conventions (`modelBuilder.Entity<T>`
  scanning by interface) and for future analyzers.
- Good: snapshots are explicitly excluded by a hard rule, preventing
  the most likely future mistake.
- Bad: the doctrinal *"Entities have identity-based equality"* promise
  is not enforced by the interface — consumers must be aware that
  current implementers use record by-value equality. Documented here.
- Bad: `IAggregateRoot<TId>` has zero implementers today (mild
  `cc-yagni` signal, accepted).

## Consequences

### Positive

- A reader opening `Agent.cs` or `BoardColumn.cs` immediately sees
  `: IEntity<AgentId>` / `: IEntity<BoardColumnId>` — the DDD nature
  is no longer invisible.
- Future tooling can dispatch on `IEntity<>` / `IAggregateRoot<>`
  generically (e.g. EF Core configurations, repository constraints,
  source-generated mappers).
- The doctrinal hierarchy *"AggregateRoot is-a Entity"* is now
  expressed in code, even if its target set is currently empty.

### Negative

- Two new files, ~10 lines total. Negligible.
- The interfaces are *partial markers* — they expose `Id` but do not
  enforce equality semantics. A reader unfamiliar with this ADR might
  incorrectly assume `IEntity<TId>` implies identity-equality. This
  ADR is the source of truth on that point.
- `IAggregateRoot<TId>` is implementer-less today. If it remains so
  indefinitely (e.g. the dashboard never gains a write-side), it
  should be removed by a future ADR — not kept as ornament.

### Hard rule for future contributors

When adding a new Domain type:

- If it has an identity (`Id`) and is **not** a snapshot → implement
  `IEntity<TId>`.
- If it has an identity, is **not** a snapshot, **and** is the
  consistency boundary of an aggregate (i.e. a command handler loads,
  mutates, saves it transactionally) → implement
  `IAggregateRoot<TId>`. This currently requires superseding ADR-001
  first.
- If it is a snapshot (any `*Snapshot` type, or any type that captures
  state at a point in time) → **do not implement** `IEntity<TId>` or
  `IAggregateRoot<TId>`. Ever. The `*Snapshot` naming convention is
  the marker.
- If it is a Value Object → no marker needed. Constraint validation
  in the constructor + structural equality from `sealed record` is
  sufficient.

## References

- ADR-001 — BoardSnapshot as a read-only projection, not an aggregate.
  Remains valid. `IAggregateRoot<TId>` has no implementer today.
- ADR-002 — String constraints extracted into dedicated Value Objects,
  with the 2026-05-27 amendment renaming `Ticket` → `TicketSnapshot`.
- Architect consultation transcript (in-session 2026-05-27): two
  passes, both recommending against marker interfaces. The project
  owner reviewed the argumentation and elected to introduce the
  interfaces with the constrained scope documented here.
- New files:
  - `src/AgentDashboard.TicketTracking.Domain/Abstractions/IEntity.cs`
  - `src/AgentDashboard.TicketTracking.Domain/Abstractions/IAggregateRoot.cs`
- Modified files:
  - `src/AgentDashboard.TicketTracking.Domain/Agents/Agent.cs` —
    implements `IEntity<AgentId>`.
  - `src/AgentDashboard.TicketTracking.Domain/Boards/BoardColumn.cs` —
    implements `IEntity<BoardColumnId>`.
- Knowledge base:
  - `~/.claude/knowledge-base/ddd/entity.md`
  - `~/.claude/knowledge-base/ddd/aggregate.md`
  - `~/.claude/knowledge-base/csharp/ddd-tactical.md`
- Session memory:
  - `feedback_snapshot_is_not_entity_or_aggregate.md` — the rule that
    excludes snapshots from these markers, recorded the same day.
