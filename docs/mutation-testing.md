---
id: mutation-testing-strategy
type: technical
owner: architect
status: current
links: [[adr-012]], [[adr-013]]
---

# Mutation testing (Stryker.NET)

We measure test **effectiveness** with mutation testing, not just line/branch
coverage. The two diverge sharply here: the suite reached **100 % line/branch**
while the Application mutation score sat far lower — coverage counts executed
lines, mutation counts *asserted* ones.

## Why two config files (per bounded-context, not per test-project)

The write-/read-sides are CQRS **vertical slices**. A slice's mapper is exercised
through its slice entry point, which for this code lives in
`Infrastructure.IntegrationTests` (the poller / the board reader) — see
[ADR-012](adr/ADR-012-test-mappers-at-slice-boundary.md). A mutation run scoped to
a single `*.UnitTests` project is therefore **blind** to that cross-project
coverage and reports the mapper as "NoCoverage", understating the real score.

Measured per bounded-context instead, the Application score moved from a
misleading **48.86 %** (unit-project only) to a truthful **64.20 %** (unit +
integration). The read-side projection — once the dominant gap, concentrated in
`GitHubBoardMapper` — is **addressed by issue #45**: the projection is now
`BoardProjection`, a first-class Application use case behind `IBoardProjection`
([ADR-013](adr/ADR-013-read-side-projection-is-an-application-use-case.md)),
verified by a behavioral `[Theory]` at the Application boundary
(`BoardProjectionShould`) instead of through Infrastructure. `BoardProjection`
itself mutation-scores **~88–93 %**.

> **⚠️ The aggregate Application-context score is currently non-deterministic.**
> Three Stryker runs of the *same* commit produced **61.9 % / 77.8 % / 85.8 %**.
> Root cause: `GitHubIssuesPollerSqliteIntegrationTests` fail under Stryker's
> repeated execution (shared SQLite file state — `table already exists`); when
> they fail at the baseline, Stryker drops their coverage and mutants they would
> kill appear as survivors. The fix (deterministic per-test SQLite isolation) is
> tracked by **issue #54**. Until it lands, the per-bounded-context **Application
> target ≥ 80 % is report-only** (not CI-gated); only the **Domain** gate is
> enforced (see issue #48). The Domain run is deterministic (`Domain.UnitTests`
> only, no integration tests).

| Context | `--config-file` | Mutated project | Test set |
|---|---|---|---|
| Domain | `stryker.domain.config.json` | `Domain` | `Domain.UnitTests` |
| Application | `stryker.application.config.json` | `Application` | `Application.UnitTests` + `Infrastructure.IntegrationTests` |

## Run it

Stryker is a **tool**, not a package reference (the single-`docker run` promise and
CPM are untouched). Install once, globally:

```bash
dotnet tool install --global dotnet-stryker
```

Then from the repository root:

```bash
dotnet-stryker --config-file stryker.domain.config.json
dotnet-stryker --config-file stryker.application.config.json
```

Reports land in `StrykerOutput/` (git-ignored): `reports/mutation-report.html`
to browse, `mutation-report.json` to script against, plus a `cleartext` summary
on stdout.

## CI gate & ratchet

The **Domain** run is CI-gated: the `mutation-domain` job in `.github/workflows/ci.yml`
runs `stryker.domain.config.json`, whose `thresholds.break` is **90** — Stryker exits
non-zero (failing the PR) if the Domain mutation score drops below 90 %. Current Domain
score is ~98 %, so the gate has ~8 pts of headroom and catches regressions, not noise.

The **Application** run stays **report-only** (`break: 0`): its aggregate score is
non-deterministic while `GitHubIssuesPollerSqliteIntegrationTests` are flaky under
repeated execution (see the ⚠️ note above; fix tracked by issue #54). Gating a flaky
score would fail PRs at random — worse than no gate. Once #54 lands and the Application
score is reproducible, raise its `break` to **70 → ratchet 80**.

**Ratchet policy.** Thresholds live in the two `stryker.*.config.json` files. Raise a
`break` value only after the score has sat comfortably above the next step for a few
cycles — never lower it to make a red build pass (fix the surviving mutant instead).
Domain ratchet: 90 → 95. Application ratchet (post-#54): 70 → 80.
