# Architecture Decision Records

This directory records architectural decisions for `agent-dashboard`,
using the MADR format. Each ADR captures one decision: what was decided,
what alternatives were considered, and why.

See `~/.claude/knowledge-base/documentation/adr.md` for the format
reference.

| ID | Title | Status | Tags |
|----|-------|--------|------|
| [ADR-001](./ADR-001-board-as-snapshot-not-aggregate.md) | BoardSnapshot as a read-only projection, not an aggregate | Accepted | ddd, aggregate, read-model |
| [ADR-002](./ADR-002-string-constraints-as-value-objects.md) | String constraints extracted into dedicated Value Objects | Accepted | ddd, value-object, primitive-obsession, test-discipline |
| [ADR-003](./ADR-003-cortex-mediator-over-mediatr.md) | Cortex.Mediator over MediatR for in-process CQRS bus | Accepted | architecture, cqrs, licensing |
| [ADR-004](./ADR-004-github-poller-housing-and-trigger.md) | GitHub Issues poller housing, on-demand trigger surface, and time abstraction | Accepted | architecture, ingestion, hosting, testing |
| [ADR-005](./ADR-005-dogfooding-scope-and-hardcoded-repo.md) | v1.0 dogfooding scope and hardcoded GitHub target repository | Accepted | scope, configuration, ingestion, distribution |
| [ADR-006](./ADR-006-blazor-redux-async-dispatch.md) | Blazor.Redux async dispatch wiring for fire-and-forget reducers | Accepted | architecture, state-management, blazor, testing, wiring |
| [ADR-007](./ADR-007-entity-and-aggregate-root-marker-interfaces.md) | `IEntity<TId>` and `IAggregateRoot<TId>` marker interfaces in the Domain layer | Accepted | ddd, entity, aggregate-root, type-system, marker-interface |
| [ADR-008](./ADR-008-ticket-tracking-is-downstream-conformist-read-model.md) | `TicketTracking` is a downstream Conformist read model of GitHub — categorically aggregate-less | Accepted | ddd, bounded-context, context-mapping, cqrs, read-model, conformist |
