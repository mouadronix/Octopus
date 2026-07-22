# ADR 001: Use SQLite as Database

## Status

Accepted

## Context

The BlueHarbor terminal management system needs a database to persist ships, docks, assignments, and terminal state. The system is a school project (ITS Learning by Project) that must be easy to set up, run locally, and demonstrate without external infrastructure dependencies.

Key constraints:
- No production deployment target — this is a demo/evaluation project
- Cross-platform development (Windows, macOS, Linux)
- Team members should be able to clone and run without database server setup
- Entity Framework Core is used as the ORM

## Decision

Use **SQLite** as the database provider via `Microsoft.EntityFrameworkCore.Sqlite`.

- Database file: `octopus.db` in the backend project directory
- Connection string: `Data Source=octopus.db`
- Schema managed by EF Core Migrations

## Consequences

**Positive:**
- Zero configuration — no server install, no connection string secrets
- Single file database — easy to share, back up, or delete for a fresh start
- Cross-platform — works identically on all development machines
- Fast for the expected data volume (dozens of ships, 8 docks)
- EF Core abstracts away SQLite-specific SQL

**Negative:**
- No concurrent write support (single-writer locking) — acceptable for a single-user demo
- No network access — cannot be shared across machines in real time
- Limited ALTER TABLE support — migrations that rename columns require workarounds
- Not suitable for production scale — but production is not a project goal

**Neutral:**
- The same EF Core code would work with PostgreSQL or SQL Server by changing one NuGet package and connection string
- Seed data is loaded via `HasData()` in `OnModelCreating`, which is provider-agnostic
