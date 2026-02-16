# Cities service options

## Migrations
To run migrations go to this directory:

```bash
cd <Repository_directory>/src/CitiesService
```

Create migration:
```bash
dotnet ef migrations add Added_indexed \
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

## GraphQL

### Test queries (cursor pagination)

First page
```graphql
query {
  cities(first: 10) {
    totalCount
    pageInfo { hasNextPage endCursor }
    nodes { id cityId name countryCode state lat lon }
  }
}
```

Next page
```graphql
query($after: String!) {
  cities(first: 10, after: $after) {
    pageInfo { hasNextPage endCursor }
    nodes { id name }
  }
}

```

Filter + sort + page
```graphql
query {
  cities(
    first: 10,
    where: { name: { contains: "mad" } },
    order: [{ name: ASC }, { id: ASC }]
  ) {
    nodes { id name countryCode }
    pageInfo { endCursor hasNextPage }
  }
}

```