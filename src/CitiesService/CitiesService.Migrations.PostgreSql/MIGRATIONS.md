# PostgreSQL Migrations

This project owns the PostgreSQL migration history for `ApplicationDbContext`.
The DbContext/model lives in `CitiesService.Infrastructure.Persistence`; runtime
provider wiring lives in `CitiesService.Infrastructure`.

## Prerequisites

Run commands from the repository root. If `dotnet ef` is not installed, install or
update it first:

```bash
dotnet tool update --global dotnet-ef
```

Use a PostgreSQL connection string that points at the target database. Local
development commonly uses:

```text
Host=localhost;Port=5432;Database=CitiesServiceDB;Username=postgres;Password=zaq1@WSX
```

For manual `dotnet ef database update`, make sure the target PostgreSQL database
exists first. Normal application startup creates it through
`PostgreSqlDatabaseBootstrapper` before applying migrations.

## Add A Migration

1. Change the EF model in `CitiesService.Infrastructure.Persistence`.
2. Generate the PostgreSQL migration into this project:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/CitiesService/CitiesService.Migrations.PostgreSql/CitiesService.Migrations.PostgreSql.csproj \
  --startup-project src/CitiesService/CitiesService.Migrations.PostgreSql/CitiesService.Migrations.PostgreSql.csproj \
  --context CitiesService.Infrastructure.Contexts.ApplicationDbContext \
  --output-dir Migrations \
  -- \
  --connection "Host=localhost;Port=5432;Database=CitiesServiceDB;Username=postgres;Password=zaq1@WSX"
```

3. Inspect the generated migration and snapshot before committing.
4. If the model change is provider-neutral, add the matching SQL Server migration
   in `CitiesService.Migrations.SqlServer` too.
5. For rowversion/concurrency changes, check whether PostgreSQL needs explicit
   trigger/function SQL in the migration.

## Run Migrations Manually

Apply migrations directly with EF tooling:

```bash
dotnet ef database update \
  --project src/CitiesService/CitiesService.Migrations.PostgreSql/CitiesService.Migrations.PostgreSql.csproj \
  --startup-project src/CitiesService/CitiesService.Migrations.PostgreSql/CitiesService.Migrations.PostgreSql.csproj \
  --context CitiesService.Infrastructure.Contexts.ApplicationDbContext \
  -- \
  --connection "Host=localhost;Port=5432;Database=CitiesServiceDB;Username=postgres;Password=zaq1@WSX"
```

The application also runs migrations on startup through
`DbMigrateAndSeedHostedService` after `PostgreSqlDatabaseBootstrapper` ensures the
database exists.

## Future Agent Notes

- Do not add PostgreSQL migrations to `CitiesService.Infrastructure` or
  `CitiesService.Infrastructure.Persistence`.
- Keep `CitiesService.Infrastructure` as the only project that directly
  references provider migration projects.
- Keep `DatabaseMigrationAssemblies.PostgreSql` set to
  `CitiesService.Migrations.PostgreSql`.
- Do not add direct migration project references from API, gRPC, GraphQL, tests,
  or Application.
- After migration changes, run the integration tests against PostgreSQL and the
  architecture boundary tests.
