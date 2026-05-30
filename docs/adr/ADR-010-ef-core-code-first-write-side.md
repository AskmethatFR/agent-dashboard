# ADR-010: EF Core Code-First for the SQLite Write-Side

## Status
Accepted (supersedes ADR-009)

## Context

ADR-009 chose hand-rolled raw-ADO.NET SQL (`CREATE TABLE IF NOT EXISTS` + `INSERT ... ON CONFLICT`) for the SQLite write-side of the TicketTracking bounded context, explicitly rejecting EF Core to avoid dependency bloat and change-tracker overhead.

The owner has since mandated (non-negotiable) that the write-side use **EF Core code-first**, aligning the persistence write path with the owner's preferred .NET stack contract. The read-side stays on Dapper (ADR-002/EPIC-2, unchanged). The `ITicketWriteRepository` port in the Application layer stays **frozen** — the Application layer remains EF-unaware; EF Core lives only in Infrastructure (Clean Architecture layering).

## Decision

**Re-implement `SqliteTicketWriteRepository` over EF Core code-first.**

- **Boundary crossing — Snapshot pattern, two types.** The domain `Ticket` externalizes its own state via `ToSnapshot()` returning an immutable `TicketPersistenceSnapshot` (flat primitives, Domain layer); `Ticket.FromSnapshot(...)` rehydrates. Infrastructure maps `TicketPersistenceSnapshot ⇄ TicketRow` (a mutable EF POCO) with a trivial flat-to-flat copy that never touches a value object. `TicketPersistenceSnapshot` is a frozen projection: no identity, no lifecycle, no transactional boundary — it is not an entity, aggregate, or `Entity<TId>`. `TicketRow` is a persistence POCO, not a domain type. Two types are required because EF's change-tracker / find-then-update path wants a mutable POCO with settable properties (an immutable record fights the tracker), and the layering rule forbids EF configuration referencing a Domain type.
- **Schema creation — guarded `IRelationalDatabaseCreator` (NOT migrations).** Run synchronously once at bootstrap (in the repository constructor, matching the prior synchronous-init contract). No `dotnet ef` CLI, no migrations folder, no `__EFMigrationsHistory` table, preserving the "single `docker run`" / zero-external-tooling promise. **`Microsoft.EntityFrameworkCore.Design` is deliberately NOT referenced.** The bare `EnsureCreated()` proved **not robustly idempotent** under concurrent construction (see "Robustly idempotent bootstrap" below); the repository instead creates the database only when `!Exists()` and the tables only when `!HasTables()`, guarded by a per-data-source lock — semantically equivalent to the prior `CREATE TABLE IF NOT EXISTS`.
- **WAL mode + checkpoint.** The repository runs `ExecuteSqlRaw("PRAGMA journal_mode=WAL;")` once at bootstrap, then `PRAGMA wal_checkpoint(TRUNCATE)` after creating the schema so the new tables are visible in the main `.db` file rather than only the `-wal` sidecar. Both pragmas are constant strings with no interpolation of any input.
- **Table shape preserved verbatim.** Table `tickets`; columns `repo`, `github_issue_number`, `title`, `status`, `agent` (nullable), `retry_count`, `github_url`, `created_at_utc`, `updated_at_utc`, `closed_at_utc` (nullable); composite PK `(repo, github_issue_number)`; indexes on `repo`, `status`, `agent`. Configured via the Fluent API in `OnModelCreating` (`ToTable`/`HasKey`/`HasIndex`, explicit `HasColumnName`, nullability). No data annotations on `TicketRow`.
- **Timestamps stored as strings** in `DateTimeOffset` round-trip ("o") format, matching the exact on-disk format the prior raw-ADO repository wrote. A unit test pins this byte-compatibility.
- **Upsert semantics preserved.** `SaveAsync` maps the snapshot to a `TicketRow`, `FindAsync([Repo, GitHubIssueNumber])`, then `Add` if absent or copies the mutable columns (`title`, `status`, `agent`, `retry_count`, `github_url`, `updated_at_utc`, `closed_at_utc`) onto the tracked row if present — never overwriting `created_at_utc`. This reproduces the exact observable semantics of the prior `INSERT ... ON CONFLICT(repo, github_issue_number) DO UPDATE`. No raw `MERGE`/`ON CONFLICT` SQL.
- **DbContext lifetime.** The repository keeps its externally-observed singleton DI registration but uses a `PooledDbContextFactory<TicketTrackingDbContext>` and creates/disposes one context per `SaveAsync` call, so the singleton never shares a mutable change-tracker across calls. SQLite ADO connection pooling is disabled on the effective connection string so each context fully closes its connection on dispose (the WAL is checkpointed on last-connection-close; no orphaned `-wal`/`-shm` handles linger across instances). The connection string flows exactly as before (`DATA_PATH` → `Data Source={dataPath}/tickets.db`).

## Robustly idempotent bootstrap (concurrent construction)

A bare `Database.EnsureCreated()` in the constructor is **not** robustly idempotent when several repository instances / DbContexts target the same SQLite file concurrently — e.g. parallel xUnit integration classes, each booting its own `WebApplicationFactory<Program>` resolving the same `DATA_PATH`. With connection pooling disabled and `journal_mode=WAL`, the schema created by the first instance lives in the uncheckpointed `-wal` sidecar; a second instance's detection connection reads the empty main `.db`, concludes the database has no tables, and re-issues `CREATE TABLE tickets` → `SQLite Error 1: 'table "tickets" already exists'`. This failure is logically impossible under the prior `CREATE TABLE IF NOT EXISTS`.

The bootstrap is therefore made robustly idempotent:

1. A process-wide per-data-source `lock` (keyed by the SQLite `DataSource` path) serializes schema creation across repository instances, so only one thread runs the create against a given file at a time.
2. The database file is created only when `IRelationalDatabaseCreator.Exists()` is false, and the tables only when `HasTables()` is false.
3. `PRAGMA wal_checkpoint(TRUNCATE)` after creation flushes the schema from the `-wal` sidecar into the main `.db`, so any later detection connection sees it.

This stays on EF Core code-first + the Snapshot pattern; no migrations, no raw DDL as the primary path.

## EnsureCreated / Migrate mutual exclusion (important)

`Database.EnsureCreated()` and `Database.Migrate()` are **mutually exclusive** and must never be mixed on the same database. `EnsureCreated()` creates the schema directly from the model without recording any migration history; `Migrate()` expects a `__EFMigrationsHistory` table and will fail or behave incorrectly against a database created by `EnsureCreated()`. If migrations are ever genuinely needed later, the move requires a deliberate ADR superseding this one: stop calling `EnsureCreated()`, add `Microsoft.EntityFrameworkCore.Design`, generate an initial migration, and provision the history table — not a silent bolt-on.

## Rationale

- Aligns the write-side with the owner's mandated stack contract.
- The Snapshot pattern keeps the Domain encapsulated and the Application port EF-unaware (Clean Architecture preserved).
- `EnsureCreated()` keeps the zero-external-tooling / single-`docker run` distribution promise intact — the main practical benefit ADR-009 sought.
- Preserving the verbatim column shape and timestamp format means the read-side (Dapper) and all existing tests continue to work unchanged.

## Consequences

### Positive
- Owner stack contract satisfied; write path is now ORM-managed and testable with EF tooling.
- Encapsulation and layering preserved via the Snapshot pattern.
- Existing read-side and test suites unaffected (column shape + timestamp format preserved verbatim).

### Negative
- Adds `Microsoft.EntityFrameworkCore.Sqlite` and its transitive dependencies to Infrastructure (the dependency-bloat con ADR-009 cited).
- The code-first bootstrap retains ADR-009's no-version-tracking / no-rollback limitation — accepted for a downstream read-model cache.
- Schema bootstrap holds a brief process-wide per-data-source lock on construction. Repositories are DI singletons constructed once per data source, so contention is limited to startup / test-host parallelism; the steady-state `SaveAsync` path is unaffected.

## Related ADRs

- ADR-009: Hand-rolled SQL migration strategy (superseded by this ADR).
- ADR-008: TicketTracking is a downstream Conformist read model.
- ADR-005: Hardcoded repository source for dogfooding.

## Notes

`Microsoft.EntityFrameworkCore.Sqlite` version is pinned via central package management (`Directory.Packages.props`) on the `10.0.0` wave, aligned to the existing `Microsoft.Data.Sqlite 10.0.0`. `Microsoft.Data.Sqlite` and `Dapper` are kept (read-side and tests depend on them).
