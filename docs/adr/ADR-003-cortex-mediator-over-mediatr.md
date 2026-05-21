# ADR-003: Cortex.Mediator over MediatR for in-process CQRS dispatch

- Status: Accepted
- Date: 2026-05-21
- Deciders: PM (Alex)
- Context tags: architecture, cqrs, licensing

## Context and Problem Statement

Slice 2 of issue #10 introduced MediatR 13.0.0 as the in-process bus for
the single `GetBoardQuery`. At first dispatch, the runtime emitted a
"Lucky Penny" commercial-license notice from MediatR's startup hook —
MediatR went commercial with v12+ and any production use without a paid
plan is out of contract. `agent-dashboard` is distributed OSS under the
"single `docker run`" promise (`CLAUDE.md` §2 / `docs/mvp-brief.md`);
shipping a binary that prints a commercial-license nag, or that
technically requires a per-deploy fee, is incompatible with that
promise. Cross-review on PR #22 also flagged a duplicated
`AddMediatR(...)` registration (one in `Program.cs`, one in
`Application/DependencyInjection.cs`).

We must either remove MediatR or upstream the cost. The migration
window is at its lifetime minimum: exactly **one query**
(`GetBoardQuery`), **one handler**, and **one dispatch site**
(`Home.razor`). Delaying the decision multiplies the eventual rewrite
cost linearly with each new use-case added to the MVP and beyond.

## Decision Drivers

- OSS license compatibility (MIT or Apache 2.0 only; no commercial
  clause, no "free for X seats" gate).
- .NET 10 / `net10.0` target framework support, today.
- Explicit CQRS abstraction split (`IQuery` vs `ICommand` rather than a
  single `IRequest<T>`), aligned with the `cqrs` neurone and the
  project's CQRS-first stance.
- Minimum migration cost — 1 query, 1 handler, 1 dispatch.
- Active maintenance on NuGet (recent releases, open issues triaged).
- No transitive runtime bloat (no extra hosted services, no source
  generator dependency, no IL emit at startup).

## Considered Options

- **A — Pin MediatR to v12.x (last fully OSS release).** Stay on
  current API, no migration, no commercial nag at runtime.
- **B — Hand-roll a minimal `IQueryDispatcher` / `ICommandDispatcher`
  pair.** Two tiny interfaces and a reflection-free DI extension.
- **C — Cortex.Mediator 3.1.2 (Buildersoft, MIT).** Drop-in mediator
  with `IQuery<TResult>` / `ICommand` / `IQueryHandler` /
  `ICommandHandler` and an assembly-scan DI extension.
- **D — Wolverine 3.x (JasperFx, MIT).** Full message bus with
  in-process and out-of-process transports.

## Decision Outcome

Chosen option: **C — Cortex.Mediator 3.1.2**. MIT-licensed, ships
explicit `IQuery` / `ICommand` interfaces matching the CQRS neurone,
and replaces both `AddMediatR(...)` calls with a single
`AddCortexMediator(new[] { typeof(DependencyInjection) }, _ => { })`
inside `Application/DependencyInjection.cs`. The migration touched
five files, kept the same handler signature (`Handle(query, ct)`), and
left all 255 tests green with zero new warnings. Runtime stdout no
longer mentions MediatR, Lucky Penny, or any commercial notice.

## Consequences

- **Good.** MIT licence — OSS distribution stays clean. Explicit
  `IQuery<T>` interface aligns code with the CQRS neurone vocabulary.
  Single DI registration point removes the orphan duplication.
  Zero runtime noise on boot. No transitive license obligation.
- **Neutral.** Smaller community than MediatR — fewer Stack Overflow
  answers, but the API surface is narrow enough that the package's
  own README plus the
  [quick-start guide](https://cortex.buildersoft.io/mediator-design-pattern/quick-start-guide/)
  cover every use case we need for the MVP.
- **Bad.** Bus-factor risk on a single-maintainer package — mitigated
  by the trivial replacement cost (one DI line and a global
  search-and-replace on the namespace, since the handler signature is
  identical). The `csharp-mediatr-cqrs` neurone in the shared skills
  repository now references a library this project no longer uses;
  flagged for a follow-up out-of-scope of issue #10.

## Pros and Cons of the Options

### A — MediatR v12 pin

- Good. Zero migration cost.
- Good. Largest .NET mediator ecosystem.
- Bad. License terms past v12 are unfriendly to OSS redistribution;
  pinning is a delaying tactic, not a resolution.
- Bad. v12 receives no further security or .NET-version fixes — a
  future .NET upgrade may force the migration anyway.

### B — Hand-rolled `IQueryDispatcher`

- Good. Zero external dependency, total control over the abstraction.
- Good. Trivial to implement at MVP scale (one switch on
  `IServiceProvider.GetRequiredService<>`).
- Bad. Re-implements pipeline behaviours, exception handling, and
  registration scanning from scratch as soon as the first cross-cutting
  concern (logging, validation, caching) appears.
- Bad. Reinvents a wheel that two MIT mediators already provide.

### C — Cortex.Mediator 3.1.2

- Good. MIT licence, `net10.0` target, explicit CQRS interfaces.
- Good. Drop-in handler signature — refactor is mechanical.
- Good. Optional pipeline behaviours (`AddDefaultBehaviors`) available
  if and when we need logging or exception handling.
- Bad. Smaller user base; one full-time maintainer.
- Bad. Documentation is thinner than MediatR's — IntelliSense and
  source reading are required for the less-common overloads.

### D — Wolverine 3.x

- Good. MIT licence, mature, JasperFx-backed.
- Good. Same in-process API plus a transport story for later.
- Bad. Pulls in a hosted service, message-tracking, and transport
  scaffolding the MVP does not need — violates `cc-yagni`.
- Bad. Much larger API surface to learn for a 1-handler MVP; raises
  cognitive cost for every reader of the codebase.

## References

- `src/AgentDashboard.TicketTracking.Application/DependencyInjection.cs`
- `src/AgentDashboard.TicketTracking.Application/Queries/GetBoardQuery.cs`
- `src/AgentDashboard.TicketTracking.Application/Queries/GetBoardQueryHandler.cs`
- `src/AgentDashboard.Web/Program.cs`
- `src/AgentDashboard.Web/Components/Pages/Home.razor`
- `Directory.Packages.props`
- [ADR-001 — BoardSnapshot as a read-only projection, not an aggregate](./ADR-001-board-as-snapshot-not-aggregate.md)
- [ADR-002 — String constraints extracted into dedicated Value Objects](./ADR-002-string-constraints-as-value-objects.md)
- [Cortex.Mediator quick-start guide](https://cortex.buildersoft.io/mediator-design-pattern/quick-start-guide/)
- [Cortex.Mediator 3.1.2 on NuGet](https://www.nuget.org/packages/Cortex.Mediator/3.1.2)
- PR #22 — slice 2 + slice 2.1 (issue #10)
