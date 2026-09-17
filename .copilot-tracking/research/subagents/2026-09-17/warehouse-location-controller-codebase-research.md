---
title: Warehouse Location Controller Codebase Research
description: Research findings for adding a stubbed warehouse location controller without modifying application code.
author: GitHub Copilot
ms.date: 2026-09-17
ms.topic: reference
keywords:
  - warehouse
  - ASP.NET Core
  - controller
  - testing
---

## Research Scope

* Investigate the smallest implementation for a controller that returns stubbed warehouse location information
* Verify routes, controller setup, serialization, dependency injection, model naming, response conventions, test hosting, and assertion patterns
* Compare a controller-local stub list with an injected repository or service

## Status

Complete, with one environment limitation: the existing tests build but cannot
execute because the .NET 8 runtime is not installed on the machine.

## Findings

### Repository Shape

* `README.md:1-2` identifies the repository only as a demo .NET API simulating
  a warehouse. It provides no endpoint, schema, or architectural convention.
* No `.github` directory exists, so `.github/copilot-instructions.md` is not
  present.
* `WarehouseWebApi` contains only `Program.cs`, project metadata,
  `Properties/launchSettings.json`, and configuration. There are no
  `Controllers`, `Models`, repository, or service source directories.
* `WarehouseWebApi.Tests` contains only `WarehouseControllerTests.cs` and its
  project file. There is no solution file, `global.json`,
  `Directory.Build.props`, or `Directory.Packages.props`.

### Endpoint and Routing Setup

* `WarehouseWebApi/Program.cs:1-3` creates a `WebApplication` but does not add
  MVC controllers with `builder.Services.AddControllers()`.
* `WarehouseWebApi/Program.cs:12-22` registers the sole endpoint directly with
  `app.MapGet("/weatherforecast", ...)`, then gives it the endpoint name
  `GetWeatherForecast`.
* `WarehouseWebApi/Program.cs:5` enables HTTPS redirection, and
  `WarehouseWebApi/Program.cs:24` starts the app. No controller routes are
  mapped with `app.MapControllers()`.
* `WarehouseWebApi/Properties/launchSettings.json:4-12` launches the HTTP
  profile at `http://localhost:5123`; `:13-21` configures HTTPS at
  `https://localhost:7123` and HTTP at port 5123. Both profiles open the
  existing `weatherforecast` route.

### Models, Serialization, and Responses

* `WarehouseWebApi/Program.cs:29-32` defines the only application model as a
  file-local `WeatherForecast` record with PascalCase C# members and a computed
  `TemperatureF` property.
* `WarehouseWebApi/Program.cs:14-20` returns a record array directly. It does
  not wrap results in an envelope, return `IResult`, or apply explicit status
  metadata. The normal successful response convention is therefore a bare JSON
  array with HTTP 200.
* No JSON or MVC serializer options are configured in `Program.cs` or either
  `appsettings` file. ASP.NET Core controller JSON uses the default web
  `System.Text.Json` options, including camel-cased JSON property names. The
  existing test response type uses PascalCase C# properties, which
  `ReadFromJsonAsync` maps with web defaults.
* `WarehouseWebApi/appsettings.json:1-8` configures logging and allows all
  hosts. `WarehouseWebApi/appsettings.Development.json:1-7` only narrows
  logging configuration. Neither supplies warehouse data or a DI binding.

### Dependency Injection

* `WarehouseWebApi/Program.cs:1-24` has no explicit service registrations.
  The current endpoint owns its in-memory data in a local `summaries` array at
  `:7-10`.
* No configuration source, persistence package, or source abstraction exists
  for warehouse data. Introducing an injected repository or service would be a
  new architectural pattern, not an extension of an existing one.

### Test Hosting and Assertions

* `WarehouseWebApi.Tests/WarehouseControllerTests.cs:9-16` uses xUnit's
  `IClassFixture<WebApplicationFactory<Program>>` and creates an in-process
  `HttpClient` from that factory. The `public partial class Program` declaration
  in `WarehouseWebApi/Program.cs:26-28` exposes the minimal API entry point to
  the test assembly.
* `WarehouseWebApi.Tests/WarehouseControllerTests.cs:18-44` issues a real GET,
  asserts `HttpStatusCode.OK`, deserializes with
  `response.Content.ReadFromJsonAsync<WeatherForecastResponse[]>()`, then
  verifies array size and every returned property.
* `WarehouseWebApi.Tests/WarehouseControllerTests.cs:46-64` uses an xUnit
  `[Theory]` with `[InlineData]` for deterministic pure-value behavior. The
  private `WeatherForecastResponse` DTO at `:66-73` is test-local rather than a
  reference to the application model.
* `WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj:1-8` targets `net8.0`
  with nullable and implicit usings enabled. `:10-25` references
  `Microsoft.AspNetCore.Mvc.Testing` 8.0.8, `Microsoft.NET.Test.Sdk` 17.11.1,
  xUnit 2.9.2, its Visual Studio adapter, and Coverlet. `:27-29` project
  references the API.
* `dotnet test WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj --no-restore`
  built successfully with SDK 10.0.401 but aborted before test execution because
  the .NET 8.0 runtime is missing. Zero tests ran, so the current test-hosting
  behavior could not be observed at runtime.

## Recommended Smallest Implementation

Add MVC controller support and keep a static, controller-local list for this
first stub endpoint. The smallest production change is three files:

1. Add `builder.Services.AddControllers();` after the builder is created and
   `app.MapControllers();` before `app.Run()` in `WarehouseWebApi/Program.cs`.
2. Add a public `WarehouseLocation` response model in
   `WarehouseWebApi/Models/WarehouseLocation.cs`.
3. Add `WarehouseWebApi/Controllers/WarehouseLocationsController.cs` with an
   explicit plural kebab-case route: `GET /api/warehouse-locations`.

An explicit route is preferable to `[Route("api/[controller]")]`: the latter
would yield `/api/warehouselocations`, which is less readable and there is no
established controller route convention to preserve. Return `Ok(Locations)` to
match the current bare-array success response while making the HTTP 200 result
explicit in controller code.

### Implementation Example

Add the controller services and route mapping:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

// Existing weather endpoint remains here.

app.MapControllers();

app.Run();
```

Use a small immutable response model:

```csharp
namespace WarehouseWebApi.Models;

public sealed record WarehouseLocation(
    string Id,
    string Zone,
    string Aisle,
    int Rack,
    int Shelf);
```

Keep the deterministic stub at its owning endpoint:

```csharp
using Microsoft.AspNetCore.Mvc;
using WarehouseWebApi.Models;

namespace WarehouseWebApi.Controllers;

[ApiController]
[Route("api/warehouse-locations")]
public sealed class WarehouseLocationsController : ControllerBase
{
    private static readonly WarehouseLocation[] Locations =
    {
        new("RCV-A-01-01", "Receiving", "A", 1, 1),
        new("STO-B-03-02", "Storage", "B", 3, 2),
        new("SHP-C-01-04", "Shipping", "C", 1, 4)
    };

    [HttpGet]
    public ActionResult<IReadOnlyList<WarehouseLocation>> Get()
    {
        return Ok(Locations);
    }
}
```

The API serializes the model as `id`, `zone`, `aisle`, `rack`, and `shelf` by
default. Do not add JSON options unless the external response contract requires
different names or enum behavior.

### Focused Test Example

Follow the established integration-test fixture and test-local response DTO
pattern in `WarehouseWebApi.Tests/WarehouseControllerTests.cs`:

```csharp
[Fact]
public async Task GetWarehouseLocations_ReturnsStubbedLocations()
{
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/api/warehouse-locations");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var locations = await response.Content
        .ReadFromJsonAsync<WarehouseLocationResponse[]>();

    Assert.NotNull(locations);
    Assert.Equal(3, locations!.Length);
    Assert.All(locations, location =>
    {
        Assert.False(string.IsNullOrWhiteSpace(location.Id));
        Assert.False(string.IsNullOrWhiteSpace(location.Zone));
        Assert.False(string.IsNullOrWhiteSpace(location.Aisle));
        Assert.InRange(location.Rack, 1, int.MaxValue);
        Assert.InRange(location.Shelf, 1, int.MaxValue);
    });
}

private sealed class WarehouseLocationResponse
{
    public string Id { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Aisle { get; set; } = string.Empty;
    public int Rack { get; set; }
    public int Shelf { get; set; }
}
```

If the literals are intentionally part of the public stub contract, add
`Assert.Collection` assertions for the ordered entries. Otherwise, retain
shape-level assertions so the test permits realistic stub-data revisions.

## Alternatives Evaluation

| Option | Change footprint | Benefits | Costs | Fit for this repository |
|--------|------------------|----------|-------|-------------------------|
| Controller-local stub list | Add controller registration, model, controller, and one integration test | Smallest change; deterministic; mirrors the existing endpoint's in-memory data ownership | Data and HTTP action stay coupled; later reuse requires extraction | Recommended for the stated stub-only requirement |
| Injected repository or service | Add controller registration, model, abstraction, implementation, DI registration, controller, and tests | Separates retrieval from HTTP; easy to replace with database, configuration, or remote data; enables isolated unit tests | Adds a pattern absent from the project; static in-memory data gains no practical behavior from indirection | Defer until a second consumer, data source, filtering/query behavior, or persistence requirement exists |

For the injected alternative, prefer a repository only when the abstraction is
about retrieving locations, not because a service layer is customary. A minimal
shape would be:

```csharp
public interface IWarehouseLocationRepository
{
    IReadOnlyList<WarehouseLocation> GetAll();
}

public sealed class StubWarehouseLocationRepository
    : IWarehouseLocationRepository
{
    public IReadOnlyList<WarehouseLocation> GetAll() =>
        new[]
        {
            new WarehouseLocation("RCV-A-01-01", "Receiving", "A", 1, 1),
            new WarehouseLocation("STO-B-03-02", "Storage", "B", 3, 2),
            new WarehouseLocation("SHP-C-01-04", "Shipping", "C", 1, 4)
        };
}

## Verified Implementation Inventory

### Files to Add or Change

* Change `WarehouseWebApi/Program.cs:1-3` to register MVC services and
  `WarehouseWebApi/Program.cs:24` to map controller routes before `app.Run()`.
* Add `WarehouseWebApi/Models/WarehouseLocation.cs` with the public response
  record containing `Id`, `Zone`, `Aisle`, `Rack`, and `Shelf`.
* Add `WarehouseWebApi/Controllers/WarehouseLocationsController.cs` with
  `GET /api/warehouse-locations`, a deterministic static array, and
  `Ok(Locations)`.
* Change `WarehouseWebApi.Tests/WarehouseControllerTests.cs:18-44` by adding a
  route-level integration test beside the existing weather test and a private
  test DTO beside `:66-73`.

### Dependencies and Existing Conventions

* No new package is required. `WarehouseWebApi/WarehouseWebApi.csproj:1-10`
  already uses the ASP.NET Core Web SDK and targets `net8.0`.
* `WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj:10-25` already supplies
  `Microsoft.AspNetCore.Mvc.Testing`, xUnit, the test SDK, and Coverlet.
  `:27-29` references the API project.
* `WarehouseWebApi/Program.cs:26-28` must retain `public partial class Program`
  because the fixture in `WarehouseWebApi.Tests/WarehouseControllerTests.cs:9`
  uses `WebApplicationFactory<Program>`.
* `WarehouseWebApi/Properties/launchSettings.json:4-21` exposes HTTP on port
  `5123` and HTTPS on port `7123`; its launch URL remains `weatherforecast` and
  does not need changing for the controller implementation.

### Risks and Discriminating Checks

* Omitting either `AddControllers()` or `MapControllers()` leaves the new
  controller unreachable. The focused integration test should fail with a
  non-200 response, which directly detects either omission.
* An implicit `[controller]` route would produce `/api/warehouselocations`, so
  use the explicit kebab-case route to preserve the selected contract.
* Returning a mutable or randomly generated collection would make the stub
  contract unstable. Use a static deterministic array and assert the expected
  count and field invariants.
* A controller-local list couples data to the endpoint. That is acceptable for
  this stub-only scope, but extraction is warranted once persistence, filtering,
  reuse, or a second consumer appears.
* The existing weather test itself accepts randomized values at
  `WarehouseWebApi.Tests/WarehouseControllerTests.cs:18-44`; this does not affect
  the proposed deterministic location test.

### Verified Validation

* Repository state check: `git status --short` succeeded and reported the
  existing `.copilot-tracking/` directory as untracked. No application source
  files were changed during this research.
* `dotnet --info` showed SDK `10.0.401` and only runtime `10.0.12`. The required
  .NET 8 runtime is absent.
* `dotnet test WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj --no-restore`
  exited with code 1. The project built successfully, then test execution
  stopped because `Microsoft.NETCore.App 8.0.0` was not installed; zero tests
  executed.
* After the .NET 8 runtime is available, run the same test command to validate
  the complete test host. Before runtime installation, the available static
  check is `dotnet build WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj
  --no-restore`.

## Recommended Implementation Sequence

1. Confirm the public route and five-field response contract with the API
   consumer, because the repository has no existing location contract.
2. Add `WarehouseWebApi/Models/WarehouseLocation.cs`.
3. Register controllers and map controller routes in
   `WarehouseWebApi/Program.cs`, preserving the weather endpoint and partial
   `Program` declaration.
4. Add `WarehouseWebApi/Controllers/WarehouseLocationsController.cs` with the
   explicit route and deterministic stub array.
5. Add the integration test and test-local DTO in
   `WarehouseWebApi.Tests/WarehouseControllerTests.cs`.
6. Run `dotnet build WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj
   --no-restore`, then run
   `dotnet test WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj --no-restore`
   after installing or otherwise providing the .NET 8 runtime.
```

Register it with `builder.Services.AddSingleton<IWarehouseLocationRepository,
StubWarehouseLocationRepository>();`, inject it through the controller
constructor, and return `Ok(repository.GetAll())`. This version should add a
controller unit test that supplies a fake repository only when the application
needs behavior beyond the integration contract. The existing integration test
pattern still applies unchanged.

## Pitfalls

* Adding a controller class alone will produce 404 responses because the current
  application has neither `AddControllers()` nor `MapControllers()`.
* Preserve `public partial class Program` in `WarehouseWebApi/Program.cs`.
  Removing it breaks the existing `WebApplicationFactory<Program>` test setup.
* `GET /api/warehouse-locations` must be asserted with its exact explicit route.
  Switching to `[controller]` token routing changes the URL.
* Keep stub output deterministic. The current weather endpoint uses random data,
  but a location stub should not, because route-level tests need stable results.
* Do not return an anonymous type or a private controller-only record when the
  model may be reused. A public response model gives the controller and tests a
  stable, named contract.
* Test response DTOs should continue using `HttpClientJsonExtensions` and
  PascalCase C# properties. The default web serializer handles camel-case JSON;
  manually asserting raw JSON casing would create unnecessary coupling unless
  casing is an explicit API contract.
* `dotnet test` requires a .NET 8 runtime for this `net8.0` test host. SDK
  10.0.401 alone does not supply that runtime in this environment.

## Remaining Research Gaps

* Product requirements do not define the route name, required warehouse fields,
  identifier format, ordering guarantee, or whether locations need availability
  or capacity data. The recommended route and five-field schema are a minimal,
  implementation-ready proposal.
* Runtime verification of the HTTP endpoint and existing factory host is blocked
  until a .NET 8 runtime is installed. The project compiles successfully, but no
  tests executed in the current environment.

## Follow-On Questions

* Should the public contract use the proposed `Id`, `Zone`, `Aisle`, `Rack`, and
  `Shelf` fields, or does an upstream warehouse schema already exist?
* Is `/api/warehouse-locations` the intended external route, or must it conform
  to an existing client contract?
