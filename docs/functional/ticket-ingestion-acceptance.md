---
id: "ticket-ingestion-acceptance"
type: "functional"
owner: "pm"
status: "current"
updated: "2026-05-31"
links:
  - "feature-catalog"
  - "glossary"
  - "ticket-tracking-write-side"
  - "adr-011"
  - "adr-010"
  - "adr-005"
  - "adr-008"
answers:
  - "What are the acceptance criteria for GitHub Issues ingestion into the Ticket write model?"
  - "How is a Ticket row keyed, and when is it inserted vs updated?"
  - "How are status / agent / retry derived from labels, and what happens on conflicting or missing labels?"
  - "What warning does the operator see for ambiguous label sets?"
decided_in:
  - "#6"
---

# Ticket Ingestion — Acceptance Criteria

> **One-liner**: The authoritative acceptance criteria for mapping each open GitHub Issue to a deterministic `Ticket` write-model row in SQLite (EPIC-1, delivered in #6).
> **Links**: [[feature-catalog]] [[glossary]] [[ticket-tracking-write-side]] [[adr-011]] [[adr-010]] — follow these for the parent capability, vocabulary, the write-side technical decision, and the migration/warning ADRs.

## Context

This node settles *what correct ingestion behavior looks like* — independent of how it is built (the technical lane is [[ticket-tracking-write-side]]). Domain terms used below (`Ticket`, `TicketStatus`, `AgentId`, `RetryCount`, `MappingWarning`) are defined in [[glossary]]. The label taxonomy and the state-machine order are fixed in `docs/labels.md`.

## Decision / Specification

### Acceptance criteria

| # | Given | Then |
|---|---|---|
| AC1 | An open issue with at most one `status:*` label, when the poller runs | A `Ticket` row exists in SQLite keyed by `github_issue_number` carrying `title`, `status`, `agent` (nullable), `retry_count` (0..3), `github_url`, `created_at_utc`, `updated_at_utc`, `closed_at_utc` (nullable). |
| AC2 | An issue is observed again with a changed label set | The existing row is updated **in place** — no duplicate rows. |
| AC3 | An issue carries multiple `status:*` labels | The status matching the **latest position in the state-machine order** is selected; a [[glossary]] `MappingWarning` is surfaced naming the issue and the conflicting labels; the row is still written. |
| AC4 | An issue carries no `status:*` label | It is treated as `status:created`; a `MappingWarning` is surfaced; the row is still written. |
| AC5 | An issue carries multiple `retry:N` labels | The **highest** `N` wins; absent any, `retry_count = 0`. |
| AC6 | An issue carries an `agent:*` label | `agent` is set to that `AgentId`; otherwise `agent` is `null`. **Assignees are NOT consulted** in the MVP (Q2 arbitration). |
| AC7 | Application startup | The SQLite database file lives under `DATA_PATH` (default `/data`, env-overridable); the schema is created **idempotently** at startup; the migration approach is documented in an ADR — see [[adr-010]]. |
| AC8 | Architecture layering | A write-repository port lives in `AgentDashboard.TicketTracking.Application`; the SQLite implementation lives in `AgentDashboard.TicketTracking.Infrastructure` — see [[ticket-tracking-write-side]]. |
| AC9 | Test strategy — integration | A real on-disk SQLite database covers: insert new ticket, update existing ticket, ignore unchanged ticket. |
| AC10 | Test strategy — unit | Chicago-school table-driven cases cover the label-to-domain mapping, **including the warning paths** of AC3, AC4, AC5. |

### Label-mapping & warning rules

| Input situation | Resolution rule | Operator outcome |
|---|---|---|
| One `status:*` label | Use it directly | Row written, no warning |
| Multiple `status:*` labels | Pick the one **furthest along** the state-machine order below | Row written + `MappingWarning` (issue + conflicting labels) |
| No `status:*` label | Default to `status:created` | Row written + `MappingWarning` (issue, missing status) |
| Multiple `retry:N` | Highest `N` | Row written, no warning |
| No `retry:N` | `retry_count = 0` | Row written, no warning |
| `agent:*` present | Map to `AgentId` | — |
| No `agent:*` | `agent = null` (assignees ignored) | — |

**State-machine order** (earliest → latest), per `docs/labels.md`:

`created → specified → in-development → in-review → in-qa → awaiting-validation → done`  (+ `escalated` as a transverse state)

> The "latest position" rule of AC3 resolves a conflicting `status:*` set to the right-most value in this sequence. Warnings are **data surfaced to the operator's logs** — see [[adr-011]] (warnings-as-data) for the technical treatment.

## Consequences / Constraints

- **MUST**: keying is `github_issue_number` alone — this is the natural identity of an ingested `Ticket` (v1.0 observes a single hardcoded repo per [[adr-005]]; the former `(repo, github_issue_number)` composite was dropped, see [[adr-008]]). Insert-or-update is decided on this key (AC1/AC2).
- **MUST**: a row is **always** written even when labels are ambiguous or absent (AC3/AC4) — ingestion never drops an observed issue.
- **MUST NOT**: derive `agent` from GitHub assignees in the MVP (AC6) — only the `agent:*` label is authoritative.
- **Out of scope**: read-side projection, closed-ticket lifecycle beyond `closed_at_utc` nullability, ETag requests, webhooks — see [[feature-catalog]].

## Open questions / Gaps

- [ ] Population of `closed_at_utc` for the close-detection path — current slice only guarantees the column exists and is nullable; the closed-ticket slice will define when it is set.
- [ ] (technical, route to Architect) Exact log channel / severity for `MappingWarning` surfacing — covered by [[adr-011]] if fully settled there; confirm no functional gap remains.
