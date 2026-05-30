---
id: "glossary"
type: "functional"
owner: "pm"
status: "current"
updated: "2026-05-30"
links:
  - "feature-catalog"
  - "ticket-ingestion-acceptance"
  - "ticket-tracking-write-side"
  - "adr-008"
answers:
  - "What does each domain term in agent-dashboard mean (Ticket, TicketStatus, AgentId, RetryCount, MappingWarning)?"
  - "What are the 8 TicketStatus values and their state-machine order?"
  - "What do the status:* / agent:* / retry:N labels map to?"
  - "What is the 'downstream Conformist read model' relationship?"
decided_in:
  - "#6"
---

# Glossary — Ubiquitous Language

> **One-liner**: The shared vocabulary of agent-dashboard's TicketTracking context — every term used in code, docs, and acceptance criteria has exactly one meaning here.
> **Links**: [[feature-catalog]] [[ticket-ingestion-acceptance]] [[ticket-tracking-write-side]] [[adr-008]] — these consume the terms below.

## Context

The team and the model speak one language. This node is the canonical definitions; nodes and code reference these terms rather than re-defining them. Terms are scoped to the **TicketTracking** bounded context. The label-derived terms mirror the taxonomy in `docs/labels.md`.

## Decision / Specification

| Term | Definition |
|---|---|
| **Ticket** | The write-model entity representing one tracked GitHub Issue. Identified by `(repo, github_issue_number)`. Carries `id`, `title`, `status`, `agent` (nullable), `retry_count`, `github_url`, `created_at_utc`, `updated_at_utc`, `closed_at_utc` (nullable). One open issue ⇒ exactly one `Ticket` row. |
| **TicketStatus** | The workflow state of a `Ticket`, a value object with 8 values. Ordered (earliest → latest): `created → specified → in-development → in-review → in-qa → awaiting-validation → done`. `escalated` is the 8th value — a **transverse** state outside the linear order (blocker / conflict / max-retries-reached). Derived from the issue's `status:*` label. |
| **State-machine order** | The linear ordering of the first 7 `TicketStatus` values above. Used to resolve a conflicting multi-`status:*` label set: the value **furthest along** this order wins (see [[ticket-ingestion-acceptance]] AC3). |
| **AgentId** | The value object identifying the agent currently responsible for a `Ticket`. One of `pm`, `architect`, `dev-a`, `dev-b`, `qa`, `security`. Derived from the issue's `agent:*` label; `null` when absent (assignees are not consulted in the MVP). |
| **RetryCount** | The review-retry counter of a `Ticket`, a value object constrained to `0..3`. Tracks how many times the ticket bounced from `in-review` back to `in-development`. Derived from the highest `retry:N` label; `0` when absent. At `3` + another rejection the ticket must escalate. |
| **MappingWarning** | A piece of operator-facing data emitted when the label set of an observed issue is ambiguous (multiple `status:*`) or incomplete (no `status:*`). It names the issue and the conflicting/missing labels. The `Ticket` row is still written. Treated as data, not an exception — see [[adr-011]]. |
| **`status:*` label** | The GitHub label encoding workflow state; maps 1:1 to a `TicketStatus` value (`status:created` ⇒ `created`, etc.). |
| **`agent:*` label** | The GitHub label encoding attribution; maps 1:1 to an `AgentId` (`agent:dev-a` ⇒ `dev-a`, etc.). |
| **`retry:N` label** | The GitHub label encoding the retry counter; `retry:0..retry:3` map to the `RetryCount` value `N`. |
| **Downstream Conformist read model** | The relationship of the TicketTracking context to GitHub Issues (the upstream): the model **conforms** to GitHub's vocabulary (labels, issue numbers) rather than translating it, and is **downstream** (read-only, polled — it never writes back to GitHub). The write model ingested here feeds a separate read-side projection. See [[adr-008]]. |

## Consequences / Constraints

- **MUST**: code, ADRs, acceptance criteria, and commit messages use these exact terms — no synonyms (e.g. never "issue state" for `TicketStatus`, never "owner" for `AgentId`).
- **MUST NOT**: introduce a `cross-review:active` label or assignee-based attribution — ruled out by `docs/labels.md` Q2.
- **Out of scope**: read-side / board-display terms (column, badge, "done today" window) — to be added to the glossary when EPIC-2 ships.

## Open questions / Gaps

- [ ] Read-side / board vocabulary (EPIC-2) — add terms once that feature is specified.
