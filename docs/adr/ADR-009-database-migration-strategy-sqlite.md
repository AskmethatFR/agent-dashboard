# ADR-009: Database Migration Strategy — Hand-Rolled SQL for SQLite

## Status
Superseded by ADR-010

## Context

EPIC-1 (GitHub Issues ingestion into SQLite) requires persisting Ticket data to a local SQLite database. The team must decide on a migration strategy for managing the database schema over time.

The TicketTracking bounded context (see ADR-008) is a **downstream Conformist read model** of GitHub. SQLite serves as a local cache, not as the system of record. Schema evolution needs are therefore simple: the `tickets` table with a composite primary key and a handful of indexes.

Options considered:

1. **Entity Framework Core Migrations**: Full-featured, code-first or migration-first, supports SQLite
2. **DbUp**: Lightweight, script-based migration tool
3. **Flyway**: Database migration tool, supports SQLite
4. **Hand-rolled SQL with `CREATE TABLE IF NOT EXISTS`**: No migration tooling, schema is idempotent

## Decision

**Use hand-rolled SQL with `CREATE TABLE IF NOT EXISTS` and no migration tooling.**

The schema is embedded as a string constant in the `SqliteTicketWriteRepository` class and executed **once at startup in the constructor**. SQLite's `IF NOT EXISTS` clause makes this idempotent.

## Rationale

### Pros

- **Zero new dependencies**: No need to add EF Core, DbUp, or Flyway packages. The project already depends on `Microsoft.Data.Sqlite` for raw ADO.NET access
- **Trivial schema**: The `tickets` table has 10 columns, 3 indexes, and a composite primary key. This is not complex enough to warrant migration tooling
- **Single-instance deployment**: The agent-dashboard runs as a single instance in dogfooding mode. Concurrent schema changes are not a concern
- **Downstream model**: As a read model cache, schema drift from GitHub is acceptable within polling intervals. There is no need for complex migration scripts

### Cons

- **No version tracking**: Hand-rolled approach doesn't track schema versions. Future schema changes require manual coordination
- **No rollback support**: Cannot easily roll back to a previous schema version
- **Manual testing**: Schema changes must be manually tested for idempotency

### Why not EF Core?

- EF Core would add ~50 transitive dependencies
- EF Core's SQLite provider has known limitations (no raw SQL migrations in some versions)
- The domain model uses strongly-typed value objects that map cleanly to raw ADO.NET parameter binding
- EF Core's change tracker is unnecessary overhead for a write-only port

### Why not DbUp or Flyway?

- Both are excellent tools for production systems with evolving schemas
- For a single-instance, single-table cache, the overhead exceeds the benefit
- The team already has hand-rolled SQL experience from previous projects

## Consequences

### Positive

- Minimal footprint: only `Microsoft.Data.Sqlite` dependency
- Fast: raw ADO.NET is the most performant option for simple CRUD
- Simple: developers can read and understand the schema directly in the code

### Negative

- Future schema changes require manual SQL edits
- No automated migration testing
- Potential for runtime errors if schema SQL has syntax errors (caught at first write, not at startup)

## Alternatives Considered and Rejected

### 1. EF Core with Code-First
Rejected due to dependency bloat and lack of need for change tracking or LINQ translation.

### 2. DbUp
Rejected due to single-instance nature and simplicity of the schema.

### 3. Flyway
Rejected for the same reasons as DbUp.

### 4. Embedded SQL file
Considered but rejected in favor of string constant for simplicity. A separate `.sql` file adds build complexity (embedding, path resolution) without significant benefit for a 15-line schema.

## Related ADRs

- ADR-005: Hardcoded repository source for dogfooding
- ADR-008: TicketTracking is a downstream Conformist read model

## Notes

The repository source (`AskmethatFR/agent-dashboard`) is hardcoded per ADR-005. If the project expands to poll multiple repositories in the future, the schema will need to accommodate this (already does via the `repo` column in the composite PK).
