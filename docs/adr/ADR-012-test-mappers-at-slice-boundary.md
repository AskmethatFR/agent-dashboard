# ADR-012: Test Single-Consumer Mappers at the Slice Boundary, Not in Isolated Unit Suites

## Status
Accepted

- **Date**: 2026-05-31
- **Deciders**: Project Manager (owner) + Project Architect
- **Tags**: testing, cqrs, vertical-slice

## Context

In this codebase the CQRS write- and read-sides are built as **vertical slices**. A
mapper inside a slice (e.g. the write-side `GitHubIssueToTicketMapper`, the read-side
`GitHubBoardMapper`) has, by construction, **exactly one production consumer** and is
**never reused** across slices:

- `GitHubIssueToTicketMapper` is consumed only by the write-side slice entry point,
  `GitHubIssuesPoller`.
- `GitHubBoardMapper` is consumed only by the read-side slice entry point,
  `GitHubBoardReader` (behind `GetBoardQuery`).

An isolated `XxxMapperTests` suite for such a mapper is **surtest**: it duplicates the
coverage the slice test already needs (a slice cannot pass without its mapper being
correct), and it **freezes the mapper's internal structure** — its method shape,
signature, and the fact that it is a separate type at all become asserted facts, so any
refactor that folds the mapper into its consumer or changes its surface breaks tests
that were never verifying user-observable behavior. That is the opposite of what a test
should pin: tests should track real breakage, not internal arrangement (Chicago-school —
assert observable state, not interactions or structure; see `tests/chicago-school.md`,
`test-chicago`, `test-all`).

This was made concrete in PR #44 (issue #6): AC3/AC4/AC5 and the happy-path
warning+mapping behavior were covered **both** by `GitHubIssueToTicketMapperTests`
(isolated) **and** implicitly by the poller, with the isolated suite freezing the mapper
as a standalone static type.

## Decision

**A single-consumer, never-reused mapper is verified at its slice entry point, asserting
the observable outcome of the slice — not in an isolated mapper unit suite.**

Concretely:

- Cover mapping behavior through the slice's entry point
  (`GitHubIssuesPoller` for the write-side, `GitHubBoardReader` / `GetBoardQuery` for the
  read-side), asserting what the slice *produces*: the warning logged, the SQLite row
  persisted, the snapshot returned.
- Use the **minimum representative set of cases that could break the mapping**, expressed
  as a `[Theory]` over those cases — enough to exercise each distinct mapping rule and
  failure mode, not an exhaustive cross-product.
- **Delete the isolated `XxxMapperTests` suite** once the slice test carries the
  equivalent coverage. No acceptance criterion may be dropped in the move.

This applies only to mappers that are **single-consumer and not reused**. A mapper that
genuinely fans out to multiple consumers, or is exercised by independent slices, is a
shared collaborator and keeps its own focused suite.

## Scope and rollout

| Side | Mapper | Slice entry point | Status |
|---|---|---|---|
| Write | `GitHubIssueToTicketMapper` | `GitHubIssuesPoller` | **Done in PR #44** — AC3/AC4/AC5 + happy-path warning+mapping moved into `GitHubIssuesPollerTests` (`[Theory]`); isolated `GitHubIssueToTicketMapperTests` deleted; no AC lost |
| Read | `GitHubBoardMapper` → `BoardProjection` | **`IBoardProjection.Project`** (Application use case) | **Done in issue #45** — projection absorbed into Application `BoardProjection` (behind `IBoardProjection`); `GitHubBoardMapper` deleted; behavioral `[Theory]` added in `BoardProjectionShould`; isolated `GitHubBoardMapperTests` deleted; no AC lost. Per-BC Application Stryker ≥ 80% (85.80%). |

The PR #44 change was **test-only** (commit `84a911b`); no production code changed.

## Consequences

- **Positive.** Tests track *real* breakage (a mapping rule that actually changes the
  persisted row / logged warning / returned snapshot), and a failure **localizes at the
  meaningful boundary** — the slice the user observes — instead of at an artificial unit
  seam. Removing the duplicate suite cuts maintenance and removes a refactor-blocking
  structural assertion (the mapper can be reshaped or inlined freely).
- **Negative / trade-off (minor, accepted).** Slice tests are integration-style and
  therefore slightly slower than a pure mapper unit test. A single representative
  `[Theory]` case may not pin *every* internal branch of the mapper the way an exhaustive
  isolated suite would; the "minimum representative set that could break the mapping"
  judgement is deliberately what we keep, and that residual risk is accepted.

## Amendment (issue #45) — read-side boundary is the Application projection use case

ADR-012 originally placed the read-side mapper's slice boundary at the Infrastructure
reader (`GitHubBoardReader` / `GetBoardQuery`). Issue #45 found the read-side projection
had **no Application-owned behavioral entry point** (its only caller was
`BoardSnapshotUpdater` in Infrastructure, fusing projection with caching), so it could
only be exercised through integration tests and was the dominant Application mutation
gap (~64% honest score).

Correction: the read-side projection **is an Application use case**
(`IBoardProjection.Project`, implemented by `BoardProjection`); the slice boundary at
which it is verified is **that Application entry point, not the Infrastructure reader**.
`GitHubBoardMapper` is absorbed into `BoardProjection` and deleted; the behavioral
`[Theory]` lives in `BoardProjectionShould`; the Infrastructure reader keeps only its
cache/poll/error tests. This does **not** weaken ADR-012's principle — it identifies the
correct boundary for the read-side. See [[adr-013]].

## Links
- Issue #6 / PR #44 (write-side application, commit `84a911b`).
- Issue #45 / EPIC-2 (read-side application — `GitHubBoardMapper` → `BoardProjection`,
  verified at `BoardProjectionShould`; see [[adr-013]]).
- Related: [[ticket-tracking-write-side]] (the slice whose mapper this governs on the
  write-side), [[adr-011]] (the warning-as-data behavior asserted at the poller),
  [[adr-010]] (the write-side persistence the poller slice produces),
  [[adr-013]] (amends this ADR for the read-side boundary).
- Testing approach: Chicago-school (`tests/chicago-school.md`).
