# ADR-008: `TicketTracking` is a downstream Conformist read model of GitHub — categorically aggregate-less

- Status: Accepted
- Date: 2026-05-27
- Deciders: Project owner (PM/orchestrator role), after a third-pass
  Project Architect analysis
- Context tags: ddd, bounded-context, context-mapping, cqrs, read-model,
  conformist

## Context and Problem Statement

After ADR-001 ("BoardSnapshot is not an aggregate") and ADR-007
("`IEntity<TId>` and `IAggregateRoot<TId>` marker interfaces, with
zero implementer for the aggregate root marker"), the question kept
recurring: **a bounded context without an aggregate root is suspicious
— did we miss a domain concept?**

The Architect was consulted a third time with that explicit framing.
The investigation produced a structural answer: the question recurs
not because a concept is missing from the code, but because **the
category of bounded context this is** was never named in the ADRs.
ADR-001 explained why `BoardSnapshot` specifically isn't an aggregate;
it did not explain why *no type* in this bounded context is an
aggregate root, and *would not become one* under the current product
scope. That gap left the door open for the question to come back.

This ADR closes that gap by naming the category.

## Decision Drivers

- **Stop the recurring question.** The "why no aggregate root?"
  conversation has happened three times in one session. The cost of
  an ADR is much lower than the cost of having the conversation
  again.
- **Be honest about what the codebase does.** The dashboard does not
  own state with a lifecycle, does not execute commands, does not
  mutate any aggregate. Calling its `Domain` layer "the bounded
  context where the team tracks tickets" is misleading — the
  *tracking* happens in GitHub; the dashboard *reads and reports*.
- **Constrain future drift.** Without this ADR, a future contributor
  might be tempted to add a command handler ("ack escalation",
  "mark agent offline", "snooze ticket"), which would change the
  *kind* of bounded context this is and require its own re-scoping.
  Naming the category makes such proposals visible as
  category-shifts, not as routine slices.
- **Reposition ADR-007's reservation.** ADR-007 introduced
  `IAggregateRoot<TId>` with zero implementer "for a future
  supersession of ADR-001". This ADR (008) clarifies that the future
  implementer is unlikely to live in `TicketTracking.Domain` at all —
  it will live in a future, separate bounded context (e.g. `Session`).
  The reservation may need to be relocated rather than activated.

## Considered Options

- **Option A — Do nothing. Leave the question to recur.** Trust ADR-001
  and ADR-007 to cover the answer implicitly. Rejected: the question
  has recurred three times in one session, including from the
  project owner. Implicit answers don't survive contact with new
  readers.
- **Option B — Rename the bounded context now.** Move
  `AgentDashboard.TicketTracking.*` to e.g.
  `AgentDashboard.TicketReporting.*` or
  `AgentDashboard.BoardObservability.*`. Settles the question by
  making the name carry the meaning. Rejected for now (not
  permanently): the rename touches 4 projects, every namespace,
  every test file, every `csproj` name, every PR description ever
  referencing the old name. Cost is real; benefit is purely
  cognitive. ADR-008 buys ~90% of the benefit at ~5% of the cost. A
  rename can be revisited separately if ADR-008 fails to stop the
  recurrence.
- **Option C — Write this ADR.** Name the category explicitly: the
  bounded context is a **downstream Conformist read model** of
  GitHub's issue-tracking context. Aggregate-less *by category*, not
  by accident or by current-slice scope.

## Decision Outcome

Chosen option: **Option C — write this ADR, name the category**.

### The bounded context's category

In DDD context-mapping terminology
(`~/.claude/knowledge-base/ddd/bounded-context.md`):

- The dashboard's `TicketTracking.Domain` sits **downstream** of
  GitHub's issue-tracking context.
- The integration relationship is **Conformist**: the dashboard adopts
  GitHub's vocabulary (Issue → `TicketSnapshot`, label → status /
  agent / retry / co-agent / escalation), without translating it.
  The mapper preserves GitHub's terms 1:1 except where derivation is
  needed (e.g. `TicketSeverity` is computed; `Freshness` is computed
  from `CreatedAt` + `now`).
- GitHub plays the **Open Host Service** role via its REST API,
  publishing its model. The dashboard is one of many possible
  downstream consumers; it does not influence GitHub's model.
- The dashboard's role in CQRS terms is **read-side-only**: it owns
  query handlers and a materialized read model cache; it owns no
  command handlers, no domain events, no event sourcing, no
  transactional aggregate.

### Evidence in the codebase

The category is not a label retrofitted onto unrelated code; the code
already implements the pattern. Reading the layers:

- **Query port**: `IBoardReader.GetCurrentAsync` — a query, by
  signature. Returns `BoardSnapshot`. No write counterpart exists.
- **Materialized read model cache**:
  `Infrastructure/Boards/BoardSnapshotCache.cs`. Its
  `Update(snapshot, asOf)` method is **replace-only**, not patch.
  Aggregates are mutated through commands; read models are replaced
  by projections. This cache is the latter.
- **Projection function**:
  `Application/GitHub/GitHubBoardMapper.MapToBoardSnapshot(records,
  now)`. Pure function: input = upstream events; output = read-side
  snapshot. This is the canonical shape of a CQRS projection.
- **Subscription mechanism**:
  `Infrastructure/GitHub/GitHubIssuesPoller`, a `BackgroundService`.
  Polling-as-subscription per ADR-004 (no webhooks, by design).
- **Query handler**:
  `Application/Queries/GetBoard/GetBoardQueryHandler` — the only
  Mediator handler in the codebase. **There is no `*CommandHandler`
  anywhere.** A `grep -rn "CommandHandler" src/` returns nothing.
- **No `IRepository<>`** in any layer. Repositories exist to load and
  save aggregates; the dashboard has no aggregates to load and save.
- **Projection invariants vs. aggregate invariants**:
  `Domain/Boards/BoardSnapshot.cs`'s constructor enforces
  referential integrity ("every `TicketSnapshot.AgentId` resolves to
  an `Agent` in the same snapshot", "no duplicate ticket IDs",
  "every `EscalationTarget` resolves to an `Agent`"). These look
  syntactically like aggregate invariants (constructor throws if
  violated), but they are categorically **projection invariants** —
  they assert the snapshot is internally consistent, not that
  domain commands respect rules. The two have different failure
  semantics: a violated projection invariant means the *projection
  pipeline is buggy* (or GitHub upstream changed in a way the mapper
  doesn't handle); a violated aggregate invariant means a *command
  would corrupt the state*. The dashboard executes zero commands,
  so the latter is impossible.

### Why this is categorical, not "not yet"

A common misreading of "no aggregates today" is "we haven't gotten
around to it yet". This ADR rejects that reading. The bounded context
is aggregate-less **by construction**, not by current-slice scope.
Three concurrent reasons:

1. **CLAUDE.md §1 contract**: "read-only by design — agents
   auto-coordinate, the human only acts on the Escalation Inbox".
   The product promise is observation, not transactional ownership.
   The day this changes, the *kind* of system changes, and a new
   ADR must supersede this one.

2. **The write-side lives in GitHub.** Every concept this bounded
   context handles has its write-side owner in GitHub:

   | Concept | Write-side owner |
   |---|---|
   | Issue id, title, createdAt | GitHub |
   | Status / column | GitHub (`status:*` label) |
   | Agent ownership | GitHub (`agent:*` label) |
   | Retry counter | GitHub (`retry:N` label) |
   | Cross-review pairing | GitHub (`co-agent:*` + `status:in-review`) |
   | Escalation | GitHub (`status:escalated` + `escalation-target:*`) |
   | Freshness / severity | Computed (deterministic from above + `now`) |

   The dashboard projects; it does not own. Adding a command handler
   would create a second write-side for the same concept (the
   dashboard's command vs. the GitHub label), which would put the
   two write-sides in race conditions — a textbook anti-pattern for
   downstream Conformist contexts.

3. **No identifier in the bounded context names an owned thing.**
   `TicketSnapshot` is a projection of an Issue (owned by GitHub).
   `Agent` is a projection of team configuration (owned by GitHub /
   the team-config source). `BoardColumn` is a projection of the
   `status:*` label taxonomy (owned by GitHub). `BoardSnapshot` is a
   projection of "the team's board at moment T" (a derived concept,
   no owner). Aggregate roots are identified by their *ownership of
   a write-side*; nothing here is owned.

### Implications for ADR-007 (`IAggregateRoot<TId>` reservation)

ADR-007 reserved `IAggregateRoot<TId>` in
`TicketTracking.Domain.Abstractions` with zero implementers, "for a
future supersession of ADR-001". ADR-008 narrows that prediction:
the future implementer is unlikely to live in this bounded context
at all. The categorical aggregate-lessness means that introducing an
aggregate root in `TicketTracking` would require *first* changing
the category of the bounded context (read-only → read+write), which
is a much larger product call than a slice ticket.

The likely future home for `IAggregateRoot<TId>` is a **separate**
bounded context (see next section). When that context is introduced,
the interface should be **relocated**, not duplicated:

- Either: move to a new `AgentDashboard.SharedKernel.Abstractions`
  project (shared kernel between bounded contexts), where both
  `TicketTracking` (which doesn't use it) and the new context (which
  does) can reference it.
- Or: leave it in `TicketTracking.Domain.Abstractions` if that ends
  up being the only project that needs to publish the contract, and
  let the new context reference it as a dependency.

Either way, **ADR-007's reservation is preserved here but flagged as
unlikely-to-be-used-locally**. Removing the interface from
`TicketTracking.Domain` is an option a future ADR may take.

### Implications for v1.1+ scope (Sessions, Escalations, Replay, Flow)

`docs/mvp-brief.md` §4 lists post-v0.1 features. Analyzing them
against the category boundary:

- **Sessions** (one agent × one ticket × one pass, with events,
  outcome, cost): this **is** an aggregate-shaped concept. It has an
  identity (`SessionId`), a lifecycle (`running` → terminal
  outcome), invariants (a `running` session has no `ended`
  timestamp; an `escalated` outcome implies an escalation chain;
  `events` count never decreases). **But Sessions ingest from a
  different upstream** (the agent runtime's `.jsonl` transcripts,
  not GitHub) **and represent a different concept** (an *act of
  agent work*, not the *ticket lifecycle*). Per `ddd-bounded-context`
  rules, this is a **separate bounded context** when it lands —
  candidate name: `AgentDashboard.SessionTracking.Domain` or
  `AgentDashboard.AgenticWork.Domain`. *Inside that future
  bounded context*, `Session` would be an aggregate root.

- **Escalations**: thinner. An Escalation looks like a projection
  over (TicketSnapshot + retry count + last-3-rejection-reasons +
  waiting-for-human), where the actual state change happens on the
  GitHub issue (`status:escalated` is removed when the human
  comments with `/claude`, per CLAUDE.md §11). The write-side is
  *still* GitHub. Escalation is likely a *highlight projection*, not
  an owned aggregate. Could move into Sessions (when an escalation
  is the outcome of a session) without becoming its own context.

- **Replay, Flow, Agent view**: visualizations / read-only navigation
  over Sessions or Tickets. No new aggregates.

**Conclusion**: v1.1+ introduces *at most* one new aggregate
(`Session`), and it lives in a future, distinct bounded context.
**The current `TicketTracking` bounded context does not gain an
aggregate root in any planned future scope.**

### Implications for naming (`TicketTracking`)

`TicketTracking` is mildly misleading: the dashboard does not *track*
(i.e. own and update) tickets — it observes and reports what GitHub
tracks. The Architect proposed three more precise names:

1. `TicketReporting` — most precise to the literal truth.
2. `BoardObservability` — best matches the project's stated mission
   ("observability cockpit"). Mild risk of APM-flavored
   connotation.
3. `BoardProjection` — DDD-correct (signals "downstream read model")
   but exposes the implementation pattern rather than the business
   concept.

**This ADR does NOT rename**. A rename touches 4 projects, all
namespaces, all tests, all `csproj` names, all PR descriptions ever
referencing the old name. The cost is real; the benefit of the
rename *after* ADR-008 is small. The rename is a separate ticket,
optional, and only justified if ADR-008 fails to stop the recurring
question.

## Pros and Cons of the Options

### Option A — Do nothing

- Good: zero cost.
- Bad: the question recurs. Already recurred three times in one
  session. Implicit answers don't survive new readers.

### Option B — Rename the bounded context

- Good: the name itself answers the question forever.
- Bad: high cost (4 projects, namespaces, tests, csprojs, PR
  references).
- Bad: irreversible without a second rename. Locking in a name
  before v1.1+ scope is known is premature.

### Option C — This ADR

- Good: cheap (~1 file, ~30 min), high impact (settles the question
  permanently, with citable evidence).
- Good: leaves the rename option open for later if needed.
- Good: makes ADR-007's reservation visible as the weak commitment
  it was, and explicitly flags relocation as the likely future.
- Bad: the underlying naming mismatch (`TicketTracking` for a
  reporting context) remains. Acceptable trade-off; revisitable.

## Consequences

### Positive

- The answer to "why no aggregate root?" is now a one-line response
  with a citable source: *"Because this bounded context is a
  downstream Conformist read model of GitHub; aggregate-less by
  category. See ADR-008."*
- Future contributors proposing command handlers in this bounded
  context will hit ADR-008 as a category-shift gate, not as a
  routine review nit.
- ADR-007's `IAggregateRoot<TId>` reservation is contextualized: its
  intended home is a future bounded context, not this one.
- v1.1+ design conversations have a clearer separation: Sessions go
  into a new context, not into `TicketTracking`.

### Negative

- `TicketTracking` remains a mildly misleading name. Acceptable for
  now; revisitable by a future ADR.
- ADR-007 (yesterday's decision) is partially walked back here in
  the sense that the predicted future implementer is now
  characterized as "likely not in this bounded context". ADR-007 is
  not superseded; this ADR refines its outlook.

### Future-conditional clauses

- **If** a v1.1+ slice introduces command handlers in
  `TicketTracking.Domain` (e.g. "ack escalation", "mark agent
  offline"), **then** ADR-008 must be superseded first, with the
  successor ADR justifying which write-side moves from GitHub to
  the dashboard, and which aggregate the new commands operate on.
- **If** a v1.1+ slice introduces a `Session` aggregate, **then**:
  - It lives in a new bounded context (`SessionTracking` or
    `AgenticWork`), not in `TicketTracking`.
  - `IAggregateRoot<TId>` is either relocated to a new
    `SharedKernel.Abstractions` project or duplicated, per the new
    context's needs.
  - ADR-007's text is amended to reflect the relocation.
- **If** the recurring "why no aggregate?" question continues to
  surface after ADR-008 is published, **then** the rename ticket
  (Option B) is reactivated and run as a separate refactor PR.

## Amendment (2026-05-31, Issue #6 follow-up): Ticket identity is `GitHubIssueNumber` alone, not a composite with the repository

The original `Ticket` projection entity used the composite identity
`(GitHubRepository, GitHubIssueNumber)`, carrying a `GitHubRepository`
value object in `TicketTracking.Domain`. This contradicted ADR-005,
which fixed the hardcoded repository as a **v1.0 Infrastructure
deployment detail** ("lives in Infrastructure, not in Domain or
Application"). The repo identity was therefore both mis-layered (a
deployment constant living in the Domain) and redundant (already held
by `GitHubPollingOptions` owner/name constants in Infrastructure).

**Decision.** Align to ADR-005: `Ticket`'s identity is
`GitHubIssueNumber` alone. The `GitHubRepository` value object is
**deleted** from the Domain (not relocated — Infrastructure already
expresses the repo via `GitHubPollingOptions`; a second carrier would
be redundant per YAGNI). Multi-repo identity remains deferred to #29
("no implementation, no ADR, no tech spec until v1.0 is DONE"), to be
designed on a clean slate.

This does not change the bounded context's category: `TicketTracking`
remains a downstream Conformist read model of GitHub. `GitHubIssueNumber`
still names a GitHub-owned thing; the dashboard owns no write-side.

## References

- ADR-001 — BoardSnapshot as a read-only projection. Not superseded;
  ADR-008 generalizes the same argument to the whole bounded
  context.
- ADR-004 — GitHub poller housing. The subscription mechanism that
  this ADR cites as evidence of the read-model pattern.
- ADR-007 — `IEntity<TId>` and `IAggregateRoot<TId>` marker
  interfaces. ADR-008 contextualizes ADR-007's reservation as
  unlikely-to-be-used-locally.
- `docs/mvp-brief.md` §4 — v1.1+ scope used to evaluate whether any
  future feature introduces an aggregate to this context.
- `CLAUDE.md` §1 — the "read-only by design" product contract.
- Knowledge base:
  - `~/.claude/knowledge-base/ddd/bounded-context.md` — Conformist,
    downstream, Open Host Service vocabulary.
  - `~/.claude/knowledge-base/cqrs/cqrs.md` — read-side / write-side
    separation framing.
- Evidence files (read-side-only pattern in the current codebase):
  - `src/AgentDashboard.TicketTracking.Application/Queries/GetBoard/GetBoardQueryHandler.cs`
    (the only Mediator handler in the codebase)
  - `src/AgentDashboard.TicketTracking.Application/GitHub/GitHubBoardMapper.cs`
    (projection function)
  - `src/AgentDashboard.TicketTracking.Infrastructure/Boards/BoardSnapshotCache.cs`
    (replace-only materialized read model)
  - `src/AgentDashboard.TicketTracking.Infrastructure/GitHub/GitHubIssuesPoller.cs`
    (subscription)
- Session memory:
  - `feedback_snapshot_is_not_entity_or_aggregate.md` — companion
    rule that snapshots never receive Entity / AggregateRoot
    markers.
