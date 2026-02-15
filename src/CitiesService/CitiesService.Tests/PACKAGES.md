# CitiesService.Tests packages

This test project intentionally keeps dependencies small and focused. 

Below is why each NuGet package is included in `src/CitiesService/CitiesService.Tests/CitiesService.Tests.csproj`.

## Test framework + runner

- `xunit`
  - The unit test framework (`[Fact]`, `[Theory]`, assertions via `Assert.*`).

- `Microsoft.NET.Test.Sdk`
  - Integrates test projects with `dotnet test`/VSTest (discovery, running, reporting).

- `xunit.runner.visualstudio`
  - VSTest runner adapter for xUnit so IDEs/`dotnet test` can discover and run xUnit tests.
  - Marked as `PrivateAssets=all` so it doesn't flow to consumers.

## Mocking + data generation

- `Moq`
  - Mocking framework used for mocks-only unit tests (verify calls, configure return values, etc.).

- `AutoFixture`
  - Generates test data/objects to reduce boilerplate.

- `AutoFixture.AutoMoq`
  - AutoFixture customization to automatically create mocks with Moq when needed.

- `AutoFixture.Xunit2`
  - Adds xUnit attributes/integration for AutoFixture.

## Realistic handler tests (EF Core)

- `Microsoft.EntityFrameworkCore.InMemory`
  - EF Core provider for in-memory DbContext.
  - Used to run async LINQ (`AnyAsync`, `ToListAsync`, `CountAsync`) realistically in unit tests.

## Logging

- `Microsoft.Extensions.Logging.Abstractions`
  - Provides `NullLogger<T>` and other lightweight logging abstractions for tests.

## Code coverage (optional)

- `coverlet.collector`
  - Enables code coverage collection via `dotnet test --collect:"XPlat Code Coverage"`.
  - Marked as `PrivateAssets=all` so it doesn't flow to consumers.
