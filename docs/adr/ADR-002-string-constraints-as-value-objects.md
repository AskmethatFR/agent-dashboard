# ADR-002: String constraints extracted into dedicated Value Objects

- Status: Accepted
- Date: 2026-05-21
- Deciders: DevA (slice 1.6 of issue #10)
- Context tags: ddd, value-object, primitive-obsession, test-discipline,
  static-readonly

## Context and Problem Statement

Slice 1.5 (commit `8ec1c4e`) introduced length constants directly on the
domain entities — `Agent.MaxNameLength`, `Agent.MaxGlyphLength`,
`Agent.MaxRoleLength`, `BoardColumn.MaxLabelLength`,
`Ticket.MaxTitleLength`. The corresponding string fields lived as raw
`string` on the entity, with validation inlined in the entity's
constructor or static factory.

This produced several issues, surfaced by the
[`feedback-ddd-test-rules`](../../../.claude/projects/-Users-alexteixeira-VSCodeProjects-agent-dashboard/memory/feedback_ddd_test_rules.md)
memory after slice 1.5 review:

1. **Primitive obsession.** Concepts like "agent name", "agent glyph",
   "ticket title" each have their own identity, invariants, and rules.
   Modeling them as `string` with constants on the host entity scatters
   their concept across files and loses the link between the value and
   its rule.
2. **Test surface confusion.** The entity's test class ends up testing
   string-length policy *and* entity behaviour. Boundary tests for the
   name field bleed into `AgentTests`, where the actual entity behaviour
   should live.
3. **`const` inlining drift.** `public const int MaxNameLength = 128` is
   inlined at the call site at compile time. If the constant ever
   changes in a future version and a consumer assembly is not recompiled,
   the consumer keeps the old value silently. This is a real
   maintainability hazard in OSS-distributed binaries.
4. **No `WithParameterName` discipline.** When a single constructor
   validates 6 parameters, distinguishing which one was at fault relies
   on exception precision; the tests must assert on `paramName`. The
   slice 1.5 constructors made this hard because every string was a
   plain `string` parameter, and the order of the checks mattered.

We must decide whether to keep the slice 1.5 status quo or extract each
constrained string into its own Value Object.

## Decision Drivers

- The domain expects every constrained primitive to be a self-validating
  type with its own test class (rule 6 of `feedback-ddd-test-rules`).
- Tests must assert `WithParameterName(...)` and the exact exception
  subtype (rule 1). This is much easier when the SUT is a single-param
  VO than when validation is buried inside a 6-param entity constructor.
- `MaxLength` must be `public static readonly`, not `public const`, to
  prevent compile-time inlining drift across versions (rule 3).
- The dashboard is read-only; behavior on these VOs is intentionally
  minimal (validation + `Value` + `ToString`). The point is correctness,
  not capability.

## Considered Options

- **Option A — keep slice 1.5 status quo.** Length constants on entities
  (`Agent.MaxNameLength`, etc.), raw `string` fields on entities.
- **Option B — primitive obsession with no constants.** Raw `string`
  fields and no length validation. Trust callers (anti-corruption layer
  if needed at the boundary).
- **Option C — extract each constrained string into a dedicated VO**
  (`AgentName`, `AgentGlyph`, `AgentRole`, `BoardColumnLabel`,
  `TicketTitle`), each `sealed record` with `Value`, `MaxLength`
  (`static readonly`), validation, and its own test class.

## Decision Outcome

Chosen option: **Option C — extract each constrained string into a
dedicated Value Object** — because it eliminates primitive obsession,
makes each constraint individually testable, removes the inlining-drift
risk, and lets the entity constructor reduce to type-presence checks
(`ArgumentNullException.ThrowIfNull`).

### VOs introduced

| New VO | Layer location | Max length | Notes |
|---|---|---|---|
| `AgentName` | `Domain/Agents/` | 128 | non-empty |
| `AgentGlyph` | `Domain/Agents/` | 8 | non-empty |
| `AgentRole` | `Domain/Agents/` | 64 | non-empty |
| `BoardColumnLabel` | `Domain/Boards/` | 128 | non-empty |
| `TicketTitle` | `Domain/Tickets/` | 512 | non-empty, rejects control chars except `\t` |

Each VO:

- is `public sealed record` (structural equality, immutability);
- exposes `public static readonly int MaxLength` — not `const`;
- validates in its single-parameter constructor;
- throws `ArgumentNullException` for null, `ArgumentException` for
  empty/whitespace, `ArgumentOutOfRangeException` for length excess,
  `ArgumentException` for the control-char rejection (`TicketTitle`
  only);
- always sets `paramName = "value"` so test assertions are stable;
- overrides `ToString()` to return `Value`.

### `Ticket` equality stays structural

`Ticket` remains a `sealed record` with structural equality (equality by
all fields). This is **intentional** and aligns with ADR-001: the
dashboard is read-only, so `Ticket` is effectively a snapshot of a
GitHub issue at polling time — not a mutable entity with identity-based
lifecycle. Two `Ticket` instances with the same `Id` but different
states represent two distinct snapshots and are correctly not equal.

If a future slice introduces ticket mutation in the dashboard (it
shouldn't, by ADR-001 and CLAUDE.md §1), `Ticket` should be promoted to
an entity with `Equals`/`GetHashCode` overridden on `Id` only.

### Other side-effects of this ADR

- `Agent`, `BoardColumn`, and `Ticket` no longer carry `MaxXxxLength`
  constants. The constants live on the VOs.
- `AgentId` and `BoardColumnId` already were VOs; their `MaxLength` was
  migrated from `const` to `static readonly` for consistency. Their
  validation was upgraded to use `ArgumentNullException` and
  `ArgumentOutOfRangeException` per rule 1.
- `Ticket.GuardTitle` is removed — the control-char check now lives in
  `TicketTitle` where the rule belongs.
- The entity constructors now only call
  `ArgumentNullException.ThrowIfNull(...)` for each VO/dependency.
  This makes `Agent` and `BoardColumn` "reference VOs" (clusters of
  Value Objects with no behavior). See "Consequences" below.

### Consequences

#### Positive

- Each constraint is testable in isolation. `AgentNameTests`,
  `AgentGlyphTests`, etc. each verify their own bound, normalization,
  equality, and exception precision in 15-17 focused tests.
- The "exactly at min" + "exactly at max" boundary cases are now obvious
  per-VO instead of buried inside a multi-parameter entity test.
- `MaxLength` exposed as `static readonly` removes the inlining hazard
  for any future binary-compatible NuGet distribution.
- `WithParameterName("value")` discipline is now trivial — every VO has
  a single parameter named `value`.
- The entity constructors become small and obvious: a list of null
  checks and field assignments. Easier to read, easier to extend.

#### Neutral

- More files (5 new VOs + 5 new test classes). The total LOC of the
  domain layer goes up by ~150 lines; the total LOC of tests goes up
  significantly (test count moved from 109 → ~232) — most of that
  growth is rule-1/2/3/4 discipline, not VO churn.

#### Negative

- `Agent` and `BoardColumn` are now "reference VOs": clusters of Value
  Objects with no behavior of their own. This is the **anemic-model
  signal** described in rule 5 of `feedback-ddd-test-rules`. We accept
  the signal here, explicitly:
  - The dashboard is read-only by design (ADR-001). There is no
    lifecycle to manage on `Agent` (an agent doesn't get hired or
    promoted in the dashboard — it's projected from the team
    configuration).
  - `BoardColumn` is similarly a label + identity, no behavior.
  - If a future slice introduces behavior (e.g. "an agent can be marked
    as offline"), the type already has the right shape to grow methods.
- A migration cost for any future caller of `new Agent(id, "DevA", ...)`:
  must now construct the VOs first. No callers outside the Domain layer
  exist today (Application / Infrastructure / Web are not yet wired to
  these types), so the cost is zero at adoption time.

## Pros and Cons of the Options

### Option A — keep slice 1.5 status quo

- Good: zero churn, fewer files.
- Bad: violates rule 6 of `feedback-ddd-test-rules` — constraints have
  their own identity and belong in their own VO.
- Bad: keeps `const` inlining drift hazard.
- Bad: `paramName` discipline is harder to apply because constructors
  have many string parameters.

### Option B — primitive obsession with no constants

- Good: simplest possible code.
- Bad: pushes validation to the boundary, which we don't have in slice 1.
- Bad: removes domain-level invariants, defeats the point of having a
  Domain layer.

### Option C — extract into dedicated VOs

- Good: every reason above (see Decision Outcome).
- Bad: more files, anemic-model signal on `Agent` / `BoardColumn` —
  acknowledged and documented.

## References

- `~/.claude/projects/-Users-alexteixeira-VSCodeProjects-agent-dashboard/memory/feedback_ddd_test_rules.md`
  — the 8-rule binding checklist that triggered this work.
- Slice 1.5 commit: `8ec1c4e` (named factories, BoardSnapshot rename,
  VO hardening).
- Knowledge base:
  - `~/.claude/knowledge-base/ddd/value-object.md`
  - `~/.claude/knowledge-base/csharp/ddd-tactical.md`
  - `~/.claude/knowledge-base/ddd/entity.md`
- New VOs:
  - `src/AgentDashboard.TicketTracking.Domain/Agents/AgentName.cs`
  - `src/AgentDashboard.TicketTracking.Domain/Agents/AgentGlyph.cs`
  - `src/AgentDashboard.TicketTracking.Domain/Agents/AgentRole.cs`
  - `src/AgentDashboard.TicketTracking.Domain/Boards/BoardColumnLabel.cs`
  - `src/AgentDashboard.TicketTracking.Domain/Tickets/TicketTitle.cs`
- Test Data Builders:
  - `tests/AgentDashboard.TicketTracking.Domain.UnitTests/Tickets/TicketBuilder.cs`
  - `tests/AgentDashboard.TicketTracking.Domain.UnitTests/Agents/AgentBuilder.cs`
- GitHub Issue: #10
- Related ADRs: ADR-001 (BoardSnapshot as a read-only projection).
