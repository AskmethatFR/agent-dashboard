# ADR-005: v1.0 dogfooding scope and hardcoded GitHub target repository

- Status: Accepted
- Date: 2026-05-22
- Deciders: Project Manager + Project Architect (issue #30)
- Context tags: scope, configuration, ingestion, distribution

## Context and Problem Statement

EPIC-1 (#1) shipped the first GitHub ingestion slice (#5, #8) and the
manual refresh button (#14). The current configuration contract,
fixed by ADR-004, exposes three environment variables read in
`GitHubPollingOptionsFactory`:

- `GITHUB_TOKEN` — personal access token (required, fail-fast).
- `GITHUB_REPO` — `owner/name` of the single target repo (required,
  fail-fast, validated by regex `^[A-Za-z0-9._-]+/[A-Za-z0-9._-]+$`).
- `POLL_INTERVAL_SECONDS` — optional, default 600s, min 300s.

The MVP brief (`docs/mvp-brief.md` §3, §5.4) historically positioned
the dashboard as a generic OSS tool: "any team clones the README,
points `GITHUB_REPO` at their own repo, runs `docker run`, and sees
their board in two minutes." Issue #29 is the placeholder for the
**post-v1 multi-repo / multi-tenant contract** — it is deliberately
unspecified (env-var list? YAML? appsettings? per-repo PAT?) and
expected to be designed from scratch when v1.1 starts.

The PM decision recorded in the amendment to `docs/mvp-brief.md` is:

> **v1.0 = dogfooding strict.** The dashboard observes the agentic
> team developing it. The target repo is hardcoded to
> `AskmethatFR/agent-dashboard`. Generic OSS usage is deferred to
> post-v1 via #29.

This ADR records that decision and resolves the question: **what
becomes of the `GITHUB_REPO` env var?**

## Decision Drivers

- **YAGNI strict.** No production tenant exists outside the dogfooding
  loop. Carrying a configurable repo through v1.0 would be a feature
  with exactly one user, who is also the maintainer.
- **No portable dette technique.** Issue #29 will design the
  multi-repo contract from scratch (likely a list, a YAML file, or a
  per-repo PAT model — none of which is a scalar `owner/name`).
  Preserving the current scalar `GITHUB_REPO` for v1.0 produces a
  contract that is guaranteed to be discarded.
- **Clarity for OSS readers.** README and `docker run` example must
  not promise "point this at any repo" until v1.1 actually delivers
  that capability. A clear "dogfooding for v1.0; see #29 for the
  generic story" message is honest.
- **Keep dynamic configuration that is genuinely per-user.**
  `GITHUB_TOKEN` (PAT, per-deployer secret) and
  `POLL_INTERVAL_SECONDS` (rate-limit knob) stay env vars.
- **Layering.** The hardcoded repo identity is a v1.0 deployment
  detail, not a domain or application concept. It lives in
  Infrastructure (alongside Octokit), not in Domain or Application.

## Considered Options

### A — Keep `GITHUB_REPO` env var, document the dogfooding default

- A1. Leave the factory as-is. Document that the only supported value
  for v1.0 is `AskmethatFR/agent-dashboard`. Any other value is
  unsupported.

### B — Hardcode the target repo in Infrastructure, drop the env var

- B1. Replace the configurable `RepositoryOwner` / `RepositoryName` on
  `GitHubPollingOptions` with two compile-time constants inside the
  Infrastructure project. `GitHubPollingOptionsFactory` no longer
  reads `GITHUB_REPO`; the constants are projected into the
  `GitHubPollingOptions` instance at construction. Delete the regex
  and the malformed-value tests.

### C — Introduce a `repos.yml` stub now to "prepare v1.1"

- C1. Build a thin YAML loader returning a single-entry list of repos,
  pre-shaping the v1.1 contract. The factory becomes a list-reader.

## Decision Outcome

**B1** — hardcode the target repo as Infrastructure constants and
drop the `GITHUB_REPO` env var entirely.

The two constants live in `GitHubPollingOptions` (Infrastructure):

```csharp
private const string DogfoodingRepositoryOwner = "AskmethatFR";
private const string DogfoodingRepositoryName  = "agent-dashboard";
```

The factory continues to expose a `GitHubPollingOptions` instance,
but `RepositoryOwner` / `RepositoryName` are now sourced from those
constants instead of `IConfiguration`. The regex
`RepoFormat()` and the `repo`-related branches in
`FromConfiguration` are removed. `GITHUB_TOKEN` and
`POLL_INTERVAL_SECONDS` are unchanged.

### Rejected — A1 (keep the env var)

Reading a value, validating it against a regex, parsing it, and
projecting it into options — only to require by documentation that
the value be exactly `AskmethatFR/agent-dashboard` — is theatre. It
produces failure modes the user cannot recover from
(`GITHUB_REPO=my-org/my-repo` would boot but observe a repo for which
the PAT has no rights or for which no data flows) and tests
(`malformed`, `missing`) whose maintenance cost serves no live
contract.

### Rejected — C1 (build the v1.1 contract early)

Designing a list/YAML/per-PAT contract before issue #29 is opened
risks pre-emptively constraining the v1.1 design space. Issue #29
explicitly says: "no implementation, no ADR, no tech spec until v1.0
is DONE." Speculative scaffolding now would be the exact opposite.

## Consequences

- **Good.** The configuration surface shrinks to two env vars
  (`GITHUB_TOKEN`, `POLL_INTERVAL_SECONDS`). The README and Docker
  example are honest about what the user controls. The "no dead
  validation code" rule is upheld.
- **Good.** Removing the regex and the malformed-repo tests reduces
  the surface area to maintain for v1.0. The unit and integration
  test suites become exactly as wide as the behaviours they protect.
- **Good.** Issue #29 designs the multi-repo contract on a clean
  slate. No "but we used to read `GITHUB_REPO`, let's keep
  backward-compat" pressure.
- **Neutral.** Anyone wanting to fork the project for their own
  dogfooding has to change two constants and rebuild the Docker image.
  This is acceptable for v1.0; v1.1 will make it a runtime concern.
- **Bad / minor.** ADR-004 references `GITHUB_REPO` as a fail-fast env
  var; this ADR supersedes that on the env-var question (the rest of
  ADR-004 — poller housing, refresh trigger, time abstraction,
  inline-validation strategy — stays valid). ADR-004 is not edited;
  this ADR is the newer source of truth.
- **Bad / minor.** The pair `(RepositoryOwner, RepositoryName)` on
  `GitHubPollingOptions` becomes effectively constant for v1.0 but
  remains an `init`-property pair, because the poller and the logger
  use them as labels. Pinning the values at object construction (via
  the factory) is the smallest possible surface change.

### Security implications

No secret is introduced — the target repo identity (`AskmethatFR /
agent-dashboard`) is public information. PAT handling is unchanged:
the `GITHUB_TOKEN` env var is read at startup, fail-fast on
missing/empty, never logged. As marginal A05/A10 hardening, the
misconfiguration "operator sets `GITHUB_REPO=<wrong-value>` and the
poller boots against the wrong repo" is now impossible — the key is
ignored by `GitHubPollingOptionsFactory`. As reviewed in the Security
audit attached to #30.

## Migration

- One PR (issue #30), on the same branch as #14 (`feat/issue-14-top-bar`)
  per PM directive. The branch already carries the TopBar slice; the
  chore is small enough to ride along in the same PR (#28).
- No data migration. No database schema change.
- Anyone running the previous Docker image with `-e GITHUB_REPO=...`
  will simply see the variable ignored after the upgrade. There is
  no breaking error path: the old env var becomes inert.

## References

- Issue #30 — chore(config): hardcode dogfooding repo for v1.0
- Issue #29 — chore(config): post-v1 multi-repo configuration contract
- Issue #14 — feat(ui): TopBar + manual refresh button (branch host)
- `docs/mvp-brief.md` §3, §4 (scope OUT), §5.3 (EPIC Config), §5.4
  (EPIC Docker), §10 (acceptance globale du MVP)
- ADR-004 — GitHub poller housing and trigger (supersedes the
  `GITHUB_REPO` env-var line item only)
- `src/AgentDashboard.TicketTracking.Infrastructure/GitHub/GitHubPollingOptions.cs`
- `src/AgentDashboard.TicketTracking.Infrastructure/GitHub/GitHubPollingOptionsFactory.cs`
