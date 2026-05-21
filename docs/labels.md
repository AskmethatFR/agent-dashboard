# Label Convention — agent-dashboard

> Source of truth for the GitHub label taxonomy used by the agentic team.
> The poller (EPIC-1) and the Team Board (EPIC-2) both rely on this contract.
> Any change here is a structural change and requires an ADR.

Labels are grouped by **category**. Each category uses a distinct color family so
the board stays readable at a glance. A ticket should carry exactly one label per
category (one `status:*`, one `epic:*`, one `size:*`, one `type:*`, optionally one
`agent:*`, and at most one `retry:*`).

## Categories

### `status:*` — workflow state (blue → purple gradient)

Drives the Team Board columns and the state machine defined in
`team-context/PROTOCOL.md` v2. Exactly **one** `status:*` label per active ticket.

| Label | Meaning |
|---|---|
| `status:created` | Ticket created, awaiting PM refinement |
| `status:specified` | PM refined; awaiting Architect tech spec |
| `status:in-development` | Architect assigned; Developer implementing |
| `status:in-review` | Cross-review in progress (DevA ↔ DevB) |
| `status:in-qa` | QA + Security review in progress |
| `status:awaiting-validation` | Awaiting PM functional validation |
| `status:done` | Validated by PM; closed |
| `status:escalated` | Escalated to human (blocker, conflict, or max retries reached) |

### `agent:*` — attribution (green family)

Identifies the agent currently responsible for the ticket. Maps to the six-agent
roster in `team-context/TEAM.md`. At most **one** active `agent:*` label at a
time (ownership transfers as the ticket moves through the state machine).

| Label | Owner |
|---|---|
| `agent:pm` | Project Manager |
| `agent:architect` | Project Architect |
| `agent:dev-a` | Developer A |
| `agent:dev-b` | Developer B |
| `agent:qa` | QA |
| `agent:security` | Security |

### `retry:*` — review retry counter (gradient warn → danger)

Tracks how many times the ticket has bounced from `status:in-review` back to
`status:in-development`. Used by the Team Board to render the `⟳ N/3` badge.
Per PROTOCOL.md v2, **max retries = 3** — the next failure must escalate.

| Label | Meaning | Board treatment |
|---|---|---|
| `retry:0` | No retry yet | Neutral |
| `retry:1` | One retry | Neutral |
| `retry:2` | Two retries | Warn (orange) |
| `retry:3` | Three retries | Danger (red) — next failure escalates |

### `epic:*` — vertical theme (teal family)

Groups every vertical-slice ticket under its parent MVP epic.

| Label | Epic |
|---|---|
| `epic:ingestion` | EPIC-1 — GitHub Issues ingestion into SQLite |
| `epic:team-board` | EPIC-2 — Team Board page (Blazor) |
| `epic:config` | EPIC-3 — Data-driven configuration (YAML + env) |
| `epic:distribution` | EPIC-4 — Docker image + GHCR distribution |

### `size:*` — effort (neutral gray)

Indicative effort, set by the PM during refinement.

| Label | Effort |
|---|---|
| `size:S` | A few hours, one focused PR |
| `size:M` | Half a day to a day, single PR |
| `size:L` | Should usually be split before specification |

### `type:*` — work type (pink/magenta family)

| Label | Meaning |
|---|---|
| `type:feature` | User-facing feature increment |
| `type:chore` | Tooling, infra, scaffolding — no direct user value |
| `type:docs` | Documentation only |
| `type:epic` | Umbrella issue grouping vertical-slice tickets |

## Arbitration of open questions (brief section 9)

These are PM commitments. Architect and Developers MAY revisit them in an ADR.

### Q1 — Convention for status / agent / retry labels
**Decision:** strictly imposed — labels follow the taxonomy above.
The poller is a deterministic mapper, not a heuristic engine. A mapping override
file (`agent-roster.yml`) is still allowed for the **status** and **agent**
mappings (see EPIC-3), but the default and reference contract is this document.

### Q2 — Detection of active cross-review
**Decision:** dedicated label `cross-review:active` is **NOT** introduced for
the MVP. Cross-review is implicit when a ticket is in `status:in-review`.
The Team Board renders the `⇄` badge based on `status:in-review` alone.
Multi-assignee or title patterns are ignored.

### Q3 — "Done today" window
**Decision:** the Team Board's "Done" column shows tickets `status:done`
**closed within the last 24 hours** (rolling window, server clock UTC).
Tickets closed earlier are excluded. The window is hard-coded for the MVP;
making it configurable is deferred.

### Q7 — Retry counter representation in GitHub
**Decision:** the canonical source is the `retry:N` label (0..3). The poller
parses the highest `retry:*` label on the issue. Comments and body checkboxes
are ignored.

### Open questions left for the human (deferred)
- Q4 — Live refresh mechanism (Blazor circuit vs SignalR hub) → **Architect's
  call**, will be settled in the EPIC-2 tech spec.
- Q5 — GitHub rate-limit handling — **RESOLVED by scope change (2026-05-21).**
  Polling default lowered from 60s to 600s (10 min), min 300s, with a manual
  refresh button always available in the topbar. At ~6 req/h the 5000 req/h
  cap is no longer a concern even with several instances on the same token.
  ETag / conditional requests remain a defensive nice-to-have but are not a
  scope driver anymore.
- Q6 — Initial historical window for closed tickets at startup → **defaulted to
  7 days**, configurable via `INITIAL_HISTORY_DAYS` (env). Will be confirmed in
  EPIC-1 tech spec.

## Conventions in tickets

- Each new ticket starts as `status:created` + `type:*` + `epic:*` + `size:*`
  (and no `agent:*` or `retry:*` yet).
- The PM moves it to `status:specified` and adds `agent:architect` when handing
  it off.
- The Architect moves it to `status:in-development` and assigns `agent:dev-a` or
  `agent:dev-b`.
- The reviewer updates `status:in-review` and swaps the `agent:*` to the peer.
- On a rejected review, the agent currently moving the ticket increments
  `retry:N → retry:N+1` and moves it back to `status:in-development`.
- On reaching `retry:3` + another rejection, the agent **must** set
  `status:escalated` and stop.

## See also
- `team-context/PROTOCOL.md` v2 (state machine, retry rules)
- `team-context/TEAM.md` (agent roster)
- `docs/mvp-brief.md` (scope and acceptance criteria source)
