# Cities service options

## Migrations
To run migrations go to this directory:

```bash
cd <Repository_directory>/src/CitiesService
```

Create migration:
```bash
dotnet ef migrations add Migration_name \
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
    where: { name: { contains: "Wars" } },
    order: [{ name: ASC }, { id: ASC }]
  ) {
    nodes { id cityId name countryCode state lat lon rowVersion}
    pageInfo { endCursor hasNextPage }
  }
}
```

Mutations - full edit:
```graphql
mutation {
  updateCity(input: {
    id: 62305
    cityId: 2634736
    name: "Warsop-mutated"
    state: ""
    countryCode: "GB"
    lon: 53.21402
    lat: -1.15091
    rowVersion: "AAAAAAABt6k="  # optional base64 - must be included
  }) {
    city {
      id
      cityId
      name
      state
      countryCode
      lat
      lon
      rowVersion
    }
  }
}
```

Mutations - patch:
```graphql
mutation {
  patchCity(input: {
    id: 62305
    name: "Warsop-patched"
  }) {
    city {
      id
      cityId
      name
      state
      countryCode
      lat
      lon
      rowVersion
    }
  }
}
```