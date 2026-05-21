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
