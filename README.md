# agent-dashboard

> Observability cockpit for an agentic engineering team.
> Watch your Claude Code agents work together on tickets like a real engineering team.

**Status:** 🚧 Early WIP — Step 1 (scaffold + Team Board on stub data)

## What is this?

`agent-dashboard` is an open-source, self-hostable dashboard to observe a team of
Claude Code sub-agents (Project Manager, Architect, Developers, QA, Security)
collaborating on tickets driven by a defined protocol.

It is **read-only** by design — agents auto-coordinate. The only place a human
acts is the Escalation Inbox.

### The 7 screens (target)

| Screen | Purpose |
|---|---|
| **Home** | At-a-glance signal: who is running, agent load, live stream, KPIs |
| **Team Board** | Kanban of tickets across the state machine, one card per ticket |
| **Sessions** | Index of every agent × ticket × pass with outcome, cost, duration |
| **Session Replay** | Step-by-step replay of one session's events (prompt → tool → finding) |
| **Agent** | Per-agent profile: workload, first-pass yield, top neurones, recent feedback |
| **Flow** | Lean/Kanban analytics: lead time, CFD, first-pass yield, throughput |
| **Escalations** | The only actionable screen — escalations awaiting a human decision |

## Stack

- **Backend:** .NET 10 + ASP.NET Core + MediatR (CQRS)
- **Frontend:** Blazor Server (Interactive Server) — SignalR for live updates
- **Persistence:** SQLite single-file (writes, reads, event store) — no external service
- **Ingestion:** GitHub polling (no inbound webhook) + Claude Code transcript tailer
- **Distribution:** Single Docker image, `docker run` and go

## Run (target — not yet wired)

```bash
docker run -p 8080:8080 \
  -v ~/.claude/projects:/claude-data:ro \
  -v ./data:/data \
  -e GITHUB_TOKEN=$GITHUB_TOKEN \
  -e GITHUB_REPO=your-org/your-repo \
  ghcr.io/askmethatfr/agent-dashboard:latest
```

Then open http://localhost:8080.

## Repository layout

```
src/
  AgentDashboard.TicketTracking.{Domain,Application,Infrastructure}/
  AgentDashboard.Web/                  # Blazor Server host
tests/
  AgentDashboard.*.{Unit,Integration}Tests/
  AgentDashboard.E2E/
docs/
  adr/                                  # Architecture Decision Records
  architecture.md                       # High-level overview
design/                                 # UX mocks (reference, not the build target)
docker/
  Dockerfile
```

## Roadmap

- [x] Step 0 — Architecture & design alignment
- [ ] **Step 1 — Scaffold + Team Board on stub data ← here**
- [ ] Step 1.5 — GitHub poller + SQLite for tickets
- [ ] Step 2 — Sessions index + transcript tailer
- [ ] Step 3 — Session Replay (event-sourced)
- [ ] Step 4 — Escalations Inbox
- [ ] Step 5 — Agent view
- [ ] Step 6 — Flow Analytics
- [ ] Step 7 — Home dashboard

## License

[MIT](LICENSE)
