# Infrastructure-Owned Provider Migrations

## Summary
- Enforce that API, gRPC, GraphQL, and tests do not directly reference migration projects.
- Let `CitiesService.Infrastructure` be the only facade project that references provider migration projects.
- Split provider-neutral EF context/model code into an infrastructure-owned persistence project to avoid project-reference cycles.

## Project Graph
- Startup/test projects -> `CitiesService.Infrastructure`
- `CitiesService.Infrastructure` -> `CitiesService.Infrastructure.Persistence`
- `CitiesService.Infrastructure` -> `CitiesService.Migrations.SqlServer`
- `CitiesService.Infrastructure` -> `CitiesService.Migrations.PostgreSql`
- `CitiesService.Migrations.*` -> `CitiesService.Infrastructure.Persistence`
- No project except `CitiesService.Infrastructure` directly references `CitiesService.Migrations.*`.

## Key Changes
- Create `CitiesService.Infrastructure.Persistence` and move `ApplicationDbContext` plus `IApplicationDbContext` there, preserving namespaces where practical.
- Create `CitiesService.Migrations.SqlServer`; move existing SQL Server migrations/snapshot from Infrastructure into it and update migration namespaces.
- Update PostgreSQL migrations to reference `CitiesService.Infrastructure.Persistence`, not `CitiesService.Infrastructure`.
- Move design-time DbContext factories into provider migration projects: one SQL Server factory and one PostgreSQL factory.
- Update `DatabaseMigrationAssemblies.SqlServer` to `CitiesService.Migrations.SqlServer`; keep PostgreSQL pointing to `CitiesService.Migrations.PostgreSql`.
- Add Infrastructure references to both migration projects so startup outputs receive migration assemblies transitively.
- Remove direct migration project references from API, gRPC, and integration tests.
- Add an Infrastructure health-check registration extension so API/gRPC do not need to mention `ApplicationDbContext` for DB health checks.

## Test Plan
- Restore and build API, gRPC, GraphQL, Infrastructure, Persistence, both migration projects, unit tests, and integration tests.
- Run unit tests with the xUnit executable runner.
- Run full CitiesService integration tests against PostgreSQL `localhost:5432` and SQL Server `localhost:1435`.
- Verify direct migration references:
  `rg "ProjectReference Include=.*CitiesService.Migrations" src/CitiesService -g "*.csproj"`
  should report only `CitiesService.Infrastructure.csproj`.
- Verify no EF packages leak outside infrastructure-owned projects:
  `rg "EntityFrameworkCore" src -g "*.csproj"`.

## Assumptions
- "Only Infrastructure" means other projects reference the Infrastructure facade, while Infrastructure may depend on infrastructure-owned subprojects.
- EF packages may exist in infrastructure-owned projects: Infrastructure, Infrastructure.Persistence, and provider migration projects.
- No schema change is intended; SQL Server migration history is moved, not regenerated.
