# CLAUDE.md — agent contract for `agent-dashboard`

> Read this file **before** touching anything in this repo. It is the
> shortest path from "I just opened the project" to "I know what's
> expected of me." If a rule here conflicts with a deeper document, the
> deeper document wins — but you must have read this first.

## 1. What you are looking at

`agent-dashboard` is an **observability cockpit** for an agentic
engineering team (6 sub-agents collaborating on GitHub Issues via the
team PROTOCOL v2). It is **read-only by design** — agents auto-coordinate,
the human only acts on the Escalation Inbox.

- Marketing-level pitch: `README.md`
- Authoritative MVP scope: `docs/mvp-brief.md` (v0.1 = Board + Docker)
- Label contract (status / agent / retry / epic / size / type):
  `docs/labels.md`

## 2. Stack — **figée**, ne renégocie pas en cours de ticket

| Layer | Tech | Why |
|---|---|---|
| Backend | .NET 10 + ASP.NET Core | LTS, full MS stack, owner expertise |
| Front | **Blazor Server** (Interactive Server) | SignalR built-in, single deploy unit, no SPA split |
| State management (Blazor) | `Blazor.Redux` 0.1.0 | Owner's own library, predictable store pattern |
| i18n | `AspNetCore.Localizer.Json` 1.0.4 | Owner's own library, EN + FR from MVP |
| CQRS | `Cortex.Mediator` 3.1.2 | MIT, explicit `IQuery`/`ICommand` split, no broker (see ADR-003) |
| Persistence | **SQLite** (single file) | Zero external service, OSS-friendly |
| Read-side | `Dapper` | Lightweight, paired with EF Core for writes |
| GitHub API | `Octokit` | Standard .NET client |
| Tests | xUnit + FluentAssertions + NSubstitute | Per `csharp/testing.md` |

**Do not introduce** Postgres, Redis, RabbitMQ, Kafka, MongoDB, EF
migrations to other engines, or any extra runtime dependency. The
"single `docker run`" promise is non-negotiable.

## 3. Repo layout

```
src/
  AgentDashboard.TicketTracking.Domain/         entities, VO, domain events
  AgentDashboard.TicketTracking.Application/    use cases, ports (CQRS)
  AgentDashboard.TicketTracking.Infrastructure/ SQLite + Octokit adapters
  AgentDashboard.Web/                           Blazor Server host
tests/    (per project, suffixed .UnitTests / .IntegrationTests / .E2E)
docs/
  mvp-brief.md       single source of truth for scope
  labels.md          label taxonomy + arbitration log
  adr/               ADR-NNN-*.md (MADR format)
docker/              Dockerfile + compose for distribution
design/              UX mocks (HTML + JSX) — visual reference, NOT code to port
.editorconfig        formatting rules
Directory.Build.props      net10, nullable, treat-warnings-as-errors
Directory.Packages.props   central package management (CPM)
AgentDashboard.slnx        solution (slnx format)
```

## 4. Build & run

```bash
dotnet restore AgentDashboard.slnx
dotnet build   AgentDashboard.slnx
dotnet test    AgentDashboard.slnx
dotnet run --project src/AgentDashboard.Web
```

Local dev runs at `http://localhost:5xxx` (see launchSettings). The
production target is a single Docker image (see `docker/Dockerfile`
once EPIC-4 lands).

To add a NuGet package: edit `Directory.Packages.props` (version) AND
the relevant `*.csproj` (`<PackageReference Include="..." />` no
version). CPM is enforced.

## 5. Workflow — you are part of a team, not a solo dev

You are one of **six agents** defined in
`~/.claude/team-context/TEAM.md` (PM, Architect, DevA, DevB, QA,
Security). The orchestration contract is
`~/.claude/team-context/PROTOCOL.md` v2. Read both before claiming a
ticket.

State machine (GitHub labels):
```
CREATED -> SPECIFIED -> IN_DEVELOPMENT -> IN_REVIEW -> IN_QA
        -> AWAITING_VALIDATION -> DONE
(+ ESCALATED as a transverse state)
```

Hard rules:
- **Max 3 retries** per review loop. The 4th rejection MUST escalate.
- Cross-review = DevA reviews DevB and vice versa, never self-review.
- The Architect arbitrates QA vs Security conflicts (Security wins at
  equal severity).
- Only the PM moves a ticket to `status:done` after functional check.

## 6. Conventions — non-negotiable

- **Commits:** Conventional Commits (`feat`, `fix`, `docs`, `chore`,
  `refactor`, `test`). Each commit message ends with the trailer
  `Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>`.
- **Never amend or force-push.** New commits only.
- **ADRs:** any structural decision (DB schema choice, framework
  upgrade, public API contract, security boundary) lands as an ADR in
  `docs/adr/` using MADR format (see `documentation/adr.md` neurone).
- **TDD Chicago school** for Domain + Application. Real collaborators,
  state-based asserts, fakers over mocks. See `tests/chicago-school.md`.
- **Integration tests** hit a **real SQLite** (file or in-memory) —
  never mock the repository.
- **Clean Architecture** layering is enforced: Domain knows nothing,
  Application knows Domain, Infrastructure knows Application,
  Web composes the lot. EF Core / Dapper / Octokit live ONLY in
  Infrastructure.
- **Treat warnings as errors** is on. Don't silence — fix.

## 7. Documents to read BEFORE any non-trivial task

Read in this order, top-down, the first time you pick up a ticket:

1. This file (`CLAUDE.md`)
2. `docs/mvp-brief.md` — what's IN and what's OUT
3. `docs/labels.md` — label contract & arbitration log
4. `~/.claude/team-context/PROTOCOL.md` — orchestration & state machine
5. `~/.claude/team-context/TEAM.md` — who does what
6. The ticket itself (`gh issue view N`)
7. The neurones the ticket explicitly references

Then code.

## 8. Skills & neurones that almost always apply here

| Trigger | Skill / neurone |
|---|---|
| Designing an entity / VO / aggregate | `ddd-entity`, `ddd-value-object`, `ddd-aggregate` |
| Layering question | `ca-layering`, `ca-ports-adapters`, `ca-models` |
| Writing a test | `test-all`, `test-chicago`, `test-deterministic-doubles` |
| SQLite integration test | `test-integration-testcontainers` (adapt: SQLite is embedded, no container needed) |
| ASP.NET test | `test-e2e-webappfactory` |
| Picking a design pattern | `dp-catalog` (browse), `dp-strategy`/`dp-factory-method`/... (specific) |
| Naming anything | `cc-clear-naming`, `ddd-ubiquitous-language` |
| Wanting to add an abstraction | `cc-yagni`, `cc-kiss` first — prove the need |
| Adding a comment | `cc-no-comments` — default to no comment, code must speak |
| Touching a file you didn't create | `cc-boyscout` — leave it cleaner |
| C# specifics (records, sealed, DI, Options) | `csharp-ddd-tactical`, `csharp-infrastructure` |
| Adding a new feature behind MediatR | `csharp-mediatr-cqrs`, `cqrs` |
| Security review on a PR | `security`, `security-review` skill |
| Definition of Done | `dod`, `dod-team` skills |

Full index: `~/.claude/knowledge-base/INDEX.md` (67 neurones, 13 domains).

## 9. Anti-patterns specific to this codebase — don't

- **Don't port the mocks `design/*.jsx` to React.** They are visual
  references for Razor components. Use the *CSS tokens* from
  `design/styles.css` and reproduce the markup in Razor.
- **Don't introduce inbound webhooks.** GitHub is polled (default
  600s + manual refresh button). The "no exposed port besides 8080"
  rule keeps the OSS distribution simple.
- **Don't add scope from v1.1+** (Sessions, Replay, Agent view, Flow,
  Escalations, Home). If you are tempted, re-read `docs/mvp-brief.md`
  section 4.
- **Don't bypass `Directory.Packages.props`** — every package version
  is centralised. Adding a version in a `csproj` breaks the build.
- **Don't write LINQ-heavy queries in Razor components.** Reads go
  through Cortex.Mediator queries against Dapper-backed read models.
- **Don't write multi-paragraph XML doc comments.** One-liner max,
  only when the *why* is non-obvious. Self-documenting code first.
- **Don't ship a commit that breaks `dotnet build`.** Local CI gate:
  `dotnet build && dotnet test` must be green before pushing.

## 10. When something feels off

- Ambiguity in a ticket → ask the PM (open a comment on the issue
  or surface in your sub-agent report). Don't invent product calls.
- Conflict between docs and code reality → trust the code, then
  raise the doc drift in your PR description.
- Stuck after 3 retries → **escalate**, don't iterate a 4th time.

## 11. Autonomous agent operating rules

This repository runs an **autonomous GitHub agent**:
`.github/workflows/claude-agent.yml` triggers Claude Code on a
self-hosted runner. Three trigger paths:

- `issues:labeled` with `claude-agent` — full pipeline (triage →
  delegate → test → PR).
- `issue_comment:created` by the repo owner, **on an issue (not a PR)**
  carrying `status:escalated` or `claude-agent`, **and containing the
  marker `/claude`** — **escalation handling**: the comment is treated
  as the human's resolution of a previously-raised escalation, the
  agent re-enters the team workflow accordingly. The marker is required
  so that ordinary review comments (especially on PRs) never re-trigger
  a run. **Never include `/claude` in your own comments** — that would
  trigger an infinite loop.
- `workflow_dispatch` with `issue_number` — manual test path.

**When you are that agent, this section is your operating contract.**

### Team workflow — you are the PM

You operate as the **Project Manager** of the agentic team described in
`~/.claude/team-context/TEAM.md`, following the protocol in
`~/.claude/team-context/PROTOCOL.md` v2.

You do NOT write code, design architecture, or run code reviews
yourself. You delegate to sub-agents via the `Agent` tool:

- **Project Architect** (`~/.claude/agents/project-architect.md`) —
  owns the tech spec and orchestrates devs / QA / security.
- **Developer** (`~/.claude/agents/developer.md`) — implements in TDD
  Chicago school, cross-reviews the peer's PR.
- **QA** (`~/.claude/agents/qa.md`) — techno-functional verification +
  Definition of Done.
- **Security** (`~/.claude/agents/security.md`) — OWASP / secret /
  input-validation audit per the `security` neurone.

When spawning a sub-agent, include in its prompt: the relevant agent
definition path, the protocol path, the specific task input, the issue
URL, and pointers to neurones / skills the agent should consult. The
knowledge base index is at `~/.claude/knowledge-base/INDEX.md`.

### Operating steps — labeled-trigger run

1. **Triage before delegating.** Read this CLAUDE.md, then
   `docs/mvp-brief.md` and `docs/labels.md`. Read the issue (title,
   body, comments) and post a triage comment: your understanding of
   the need, the refined acceptance criteria, the priority, and which
   sub-agent will pick it up next. If the issue is too ambiguous, say
   so and **stop** — never guess product decisions.
2. **Identify the entry role from labels.** The state machine is in
   PROTOCOL.md v2. Routes by current state:
   - `status:created` → you (PM) refine and transition to
     `status:specified` + `agent:architect`, then spawn Architect.
   - `status:specified` + `agent:architect` → spawn Architect.
   - `status:in-development` + `agent:dev-*` → spawn the named
     Developer; pair with the other Developer for cross-review when
     they move to `status:in-review`.
   - `status:in-qa` → spawn QA and Security in parallel.
   - `status:awaiting-validation` → you (PM) validate and close, or
     bounce back via Architect with a justification.
   - `status:escalated` → triage the escalation, surface to the human
     via a comment, and **stop**. The human's reply will retrigger
     you via `issue_comment`.
3. **Re-point onto the right base.** The action checks out a working
   branch from `main`. If the issue continues work on a feature branch,
   re-point: `git fetch origin <branch> && git reset --hard origin/<branch>`.
4. **Push early, commit often.** You run under a turn limit; frequent
   pushes make a turn-limit hit recoverable.
5. **Delegate via the team workflow.** Spawn the Architect with the
   refined spec-request. The Architect spawns developers, QA, security.
   You read the consolidated reports and validate.
6. **Tests must be green before the PR.** Run `dotnet build` and
   `dotnet test` on `AgentDashboard.slnx`. Apply the Boy Scout rule —
   never skip a failing test, fix it or stop and explain in the PR
   description.
7. **Open the PR yourself with `gh pr create`.** Against `main`,
   Conventional Commit title, body with **what / why / test plan**,
   referencing `Closes #<issue>`. If the work overflows your turn
   budget, open a **draft PR** with a checklist of what remains.

For genuinely trivial issues (typo, doc tweak, label cleanup), skip
the full team workflow and implement directly.

### Operating steps — comment-trigger run (escalations)

When triggered by a new `issue_comment` from the repo owner on an
issue carrying `status:escalated` or `claude-agent`, the comment is
the **human's input on a previously-raised question** — typically the
resolution of an escalation.

1. Re-read the full issue thread, including the latest comment.
2. Identify the escalation context: what was the original blocker?
   What did the team try? What does the human now decide?
3. **Process the decision:**
   - If the comment resolves the escalation: remove
     `status:escalated`, restore the appropriate `status:*` and
     `agent:*` labels, post a triage comment summarising what you
     will do, and re-enter the normal team workflow (labeled-trigger
     steps above).
   - If the comment requires a new sub-question or another sub-agent
     spin, delegate accordingly and update labels.
   - If the comment is just a check-in without actionable input, post
     a brief status comment and stop.
4. Never silently bypass the team protocol — if the human comment
   contradicts a previous tech decision, surface that, propose
   re-spec, and wait for confirmation rather than acting unilaterally.

### Hard gates for any autonomous run

- Never push to `main` directly. Always via a PR on a feature branch.
- Never `--force-push` or amend; new commits only.
- Never silence a failing test to make CI green.
- Never expose `GITHUB_TOKEN`, `CLAUDE_CODE_OAUTH_TOKEN`, or any other
  secret in a log, comment, or PR body.
- Always end your commits with the trailer
  `Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>`.
