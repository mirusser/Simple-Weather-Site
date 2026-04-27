# CitiesService Migrations Notes For Agents

CitiesService uses provider-specific EF Core migration projects:

- SQL Server: `src/CitiesService/CitiesService.Migrations.SqlServer`
- PostgreSQL: `src/CitiesService/CitiesService.Migrations.PostgreSql`

Each project has its own `MIGRATIONS.md` with the manual `dotnet ef` commands
for adding and running migrations:

- `src/CitiesService/CitiesService.Migrations.SqlServer/MIGRATIONS.md`
- `src/CitiesService/CitiesService.Migrations.PostgreSql/MIGRATIONS.md`

Important boundaries:

- `ApplicationDbContext` and provider-neutral EF model code live in
  `src/CitiesService/CitiesService.Infrastructure.Persistence`.
- Runtime provider selection and startup migration/seeding live in
  `src/CitiesService/CitiesService.Infrastructure`.
- Only `CitiesService.Infrastructure` should directly reference migration
  projects.
- Do not add migration project references to API, gRPC, GraphQL, Application, or
  test projects.

When changing the EF model, check whether both providers need matching
migrations. After migration changes, run the CitiesService integration tests
against the local SQL Server and PostgreSQL containers and keep the architecture
boundary tests passing.
