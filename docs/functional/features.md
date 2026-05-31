---
id: "feature-catalog"
type: "functional"
owner: "pm"
status: "current"
updated: "2026-05-31"
links:
  - "ticket-ingestion-acceptance"
  - "glossary"
answers:
  - "What user-observable capabilities does agent-dashboard provide?"
  - "Which feature owns GitHub Issues ingestion into the Ticket write model?"
  - "What is in scope vs out of scope for the ingestion feature delivered by #6?"
decided_in:
  - "#6"
---

# Feature Catalog

> **One-liner**: The authoritative list of agent-dashboard's user-observable capabilities, what each does, and where its scope ends.
> **Links**: [[ticket-ingestion-acceptance]] [[glossary]] — follow these for the detailed acceptance and the domain vocabulary.

## Context

`agent-dashboard` is a read-only observability cockpit for a 6-agent engineering team that coordinates through GitHub Issues. Capabilities are delivered as vertical slices grouped under MVP epics (see `docs/mvp-brief.md`). This catalog is the entry point of the functional graph: one row per capability, each pointing at its detailed acceptance node.

## Decision / Specification

| Capability | Epic | What it does (observable behavior) | Detailed acceptance |
|---|---|---|---|
| **GitHub Issues ingestion → Ticket write model** | EPIC-1 (ingestion) | Each open GitHub Issue observed by the poller is deterministically mapped to a `Ticket` row in SQLite, keyed by `github_issue_number`. Re-observing an issue updates the same row in place. Status / agent / retry are derived from the issue's labels per `docs/labels.md`; ambiguous or missing `status:*` labels still produce a row plus an operator warning. | [[ticket-ingestion-acceptance]] |

## Consequences / Constraints

- **MUST**: every observed open issue results in exactly one persisted `Ticket` row (insert-or-update on `github_issue_number`).
- **MUST**: label-to-domain mapping is **deterministic** (a mapper, not a heuristic engine) — it follows the [[glossary]] terms and the taxonomy in `docs/labels.md`.
- **MUST NOT**: surface ingestion as a user action — the dashboard is read-only; ingestion is driven by the background poller (default 600s + manual refresh).
- **Out of scope (delivered by later slices / epics)**:
  - The **read-side board projection** optimised for display lives in **EPIC-2** (team-board) — this feature owns only the write model.
  - **Closed-ticket handling** and the "done within 24h" window — later slice in EPIC-1 / EPIC-2.
  - **ETag conditional requests** — own slice in EPIC-1.
  - **Inbound webhooks** — explicitly ruled out; GitHub is polled.

## Open questions / Gaps

- [ ] Read-side board feature node (EPIC-2) not yet authored — add when that slice ships.
- [ ] Closed-ticket lifecycle (close detection, `closed_at_utc` population beyond the open-issue path) — to be specified in the next EPIC-1 slice.
