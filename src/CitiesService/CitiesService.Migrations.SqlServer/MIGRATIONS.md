# SQL Server Migrations

This project owns the SQL Server migration history for `ApplicationDbContext`.
The DbContext/model lives in `CitiesService.Infrastructure.Persistence`; runtime
provider wiring lives in `CitiesService.Infrastructure`.

## Prerequisites

Run commands from the repository root. If `dotnet ef` is not installed, install or
update it first:

```bash
dotnet tool update --global dotnet-ef
```

Use a SQL Server connection string that points at the target database. Local
development commonly uses:

```text
Server=localhost,1435;Database=CitiesServiceDB;User Id=sa;Password=zaq1@WSX;Encrypt=False;TrustServerCertificate=True;
```

## Add A Migration

1. Change the EF model in `CitiesService.Infrastructure.Persistence`.
2. Generate the SQL Server migration into this project:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/CitiesService/CitiesService.Migrations.SqlServer/CitiesService.Migrations.SqlServer.csproj \
  --startup-project src/CitiesService/CitiesService.Migrations.SqlServer/CitiesService.Migrations.SqlServer.csproj \
  --context CitiesService.Infrastructure.Contexts.ApplicationDbContext \
  --output-dir Migrations \
  -- \
  --connection "Server=localhost,1435;Database=CitiesServiceDB;User Id=sa;Password=zaq1@WSX;Encrypt=False;TrustServerCertificate=True;"
```

3. Inspect the generated migration and snapshot before committing.
4. If the model change is provider-neutral, add the matching PostgreSQL migration
   in `CitiesService.Migrations.PostgreSql` too.

## Run Migrations Manually

Apply migrations directly with EF tooling:

```bash
dotnet ef database update \
  --project src/CitiesService/CitiesService.Migrations.SqlServer/CitiesService.Migrations.SqlServer.csproj \
  --startup-project src/CitiesService/CitiesService.Migrations.SqlServer/CitiesService.Migrations.SqlServer.csproj \
  --context CitiesService.Infrastructure.Contexts.ApplicationDbContext \
  -- \
  --connection "Server=localhost,1435;Database=CitiesServiceDB;User Id=sa;Password=zaq1@WSX;Encrypt=False;TrustServerCertificate=True;"
```

The application also runs migrations on startup through
`DbMigrateAndSeedHostedService` after `SqlServerDatabaseBootstrapper` ensures the
database exists.

## Future Agent Notes

- Do not add SQL Server migrations to `CitiesService.Infrastructure` or
  `CitiesService.Infrastructure.Persistence`.
- Keep `CitiesService.Infrastructure` as the only project that directly
  references provider migration projects.
- Keep `DatabaseMigrationAssemblies.SqlServer` set to
  `CitiesService.Migrations.SqlServer`.
- Do not add direct migration project references from API, gRPC, GraphQL, tests,
  or Application.
- After migration changes, run the integration tests against SQL Server and the
  architecture boundary tests.
