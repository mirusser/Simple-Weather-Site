# Cities service options

## Migrations
To run migrations go to this directory:

```bash
cd <Repository_directory>/src/CitiesService
```

Create migration:
```bash
dotnet ef migrations add Migration_Name \
  --project CitiesService.Infrastructure/CitiesService.Infrastructure.csproj \
  --context ApplicationDbContext
```

There should be no need for that, cause migrations are applied during app startup,
but if needed manually apply migration:
```bash
dotnet ef database update \
  --project CitiesService.Infrastructure/CitiesService.Infrastructure.csproj \
  --context ApplicationDbContext
```