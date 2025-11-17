# Data Directory

This folder centralizes all persistence-layer artifacts for RateMyTeacher. It currently holds:

- `ApplicationDbContext` (coming in Phase 2) with EF Core configuration for users, teachers, ratings, bonuses, and AI metadata.
- Entity Framework Core migrations and database seed helpers.
- Data access utilities (design-time factory, migration scripts, and maintenance notes).

## Responsibilities

1. **DbContext Definition**
   - Map every entity outlined in `specs/main/data-model.md`.
   - Configure relationships, indexes, and query filters according to the specification and `PRINCIPLES.md`.
2. **Migrations**
   - Track schema evolution for SQLite and future relational providers.
   - Keep migrations deterministic and committed to source control.
3. **Seed Data**
   - Provide repeatable bootstrap data (admin account, sample teachers/students, bonus config, semesters).
   - Ensure seeds are idempotent and guarded by environment checks.
4. **Design-Time Support**
   - Expose factories and helpers so `dotnet ef` commands work without the running app.

## Operating Notes

- The SQLite data file (`ratemyteacher.db`) lives under `Data/` but is ignored via `.gitignore` patterns so binaries never enter version control.
- All configuration for connection strings and secrets belongs in `.env` or `appsettings.*.json`; no inline secrets here.
- When upgrading frameworks (for example, to ASP.NET Core 10 / EF Core 10), update the DbContext and migrations in lockstep to prevent drift.
