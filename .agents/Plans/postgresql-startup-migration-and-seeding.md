# Add PostgreSQL Startup Migration And Seeding Support

## Summary
- Keep dual-provider support: SQL Server remains usable, PostgreSQL becomes selectable via configuration.
- Keep the existing SQL Server migrations in `CitiesService.Infrastructure`.
- Add a fresh PostgreSQL migration assembly so PostgreSQL never tries to apply SQL Server-shaped migrations.
- Keep one hosted startup service, but extract provider-specific database creation and seed locking behind small infrastructure services.

## Key Changes
- Add config key `Database:Provider` with accepted values `SqlServer` and `PostgreSql`.
- Update CitiesService API and gRPC appsettings/deployment secrets to use:
  - `Database:Provider=PostgreSql`
  - `ConnectionStrings:DefaultConnection=Host=postgres;Port=5432;Database=CitiesServiceDB;Username=postgres;Password=...`
- Add `Npgsql.EntityFrameworkCore.PostgreSQL` to the infrastructure project. Use the current compatible EF Core 10 provider line; NuGet currently shows `10.0.1`.

## Implementation Changes
- Refactor `DbMigrateAndSeedHostedService` into a provider-neutral orchestrator:
  - resolve `IDatabaseBootstrapper`
  - ensure database exists
  - run `GetPendingMigrationsAsync` / `MigrateAsync`
  - call `ICitiesSeeder.SeedIfEmptyAsync`
- Move current SQL Server database creation logic into `SqlServerDatabaseBootstrapper`.
- Add `PostgreSqlDatabaseBootstrapper`:
  - parse `DefaultConnection` with `NpgsqlConnectionStringBuilder`
  - require `Database`
  - connect to maintenance DB `postgres`
  - check `pg_database`
  - run quoted `CREATE DATABASE "<db>"` if missing
  - ignore duplicate-database race errors
- Replace `GenericRepository.TryAcquireSeedLockAsync` with a dedicated seed-lock abstraction used by `CitiesSeeder`.
  - SQL Server implementation keeps `sp_getapplock`
  - PostgreSQL implementation uses session advisory lock, e.g. `pg_try_advisory_lock(hashtext(@resource))`
  - return an async lease so the lock is explicitly released after seeding
- Update EF registration in `AddInfrastructureLayer`:
  - `SqlServer` uses `UseSqlServer(..., migrationsAssembly: CitiesService.Infrastructure)`
  - `PostgreSql` uses `UseNpgsql(..., migrationsAssembly: CitiesService.Migrations.PostgreSql)`
- Add a new `CitiesService.Migrations.PostgreSql` class library referencing `CitiesService.Infrastructure`.
  - API and gRPC startup projects reference this project so migrations are present at runtime.
  - Generate a fresh PostgreSQL initial migration there.
- Update `ApplicationDbContextFactory` to select provider from `Database:Provider` or `--provider`, so EF tooling can scaffold the correct provider’s migrations.

## Test Plan
- Add PostgreSQL testing helpers mirroring the existing SQL Server fixture:
  - `POSTGRES_BASE_CONNECTION`
  - PostgreSQL fact attribute that skips when not configured
  - helper to build per-test database connection strings
- Add PostgreSQL integration coverage for:
  - hosted service creates missing database, applies migrations, and seeds
  - PostgreSQL seed lock allows only one active seeder
  - migrations leave no pending migrations
- Keep existing SQL Server tests passing to verify dual-provider support.
- Run at minimum:
  - `dotnet test src/CitiesService/CitiesService.Tests`
  - `dotnet test src/CitiesService/CitiesService.IntegrationTests` with PostgreSQL configured
  - SQL Server integration tests when SQL Server config is available

## Assumptions
- “Regenerate fresh” means fresh PostgreSQL migrations only; existing SQL Server migrations stay for SQL Server support.
- Auto-create database behavior remains, so the PostgreSQL user must have permission to create databases.
- Kubernetes/docker wiring should point CitiesService and CitiesGrpcService at PostgreSQL; unrelated SQL Server consumers like BackupService can stay unchanged.
- References used: Microsoft EF Core multiple-provider migrations docs and NuGet’s `Npgsql.EntityFrameworkCore.PostgreSQL` package page.
