<!-- markdownlint-disable-file -->
# Task Research: Warehouse Location Controller Implementation Plan

Translate the verified repository research into an implementation-ready plan
for a stubbed warehouse-location endpoint.

## Task Implementation Requests

* Add a deterministic `GET /api/warehouse-locations` endpoint.
* Preserve the existing weather endpoint and integration-test entry point.
* Add focused integration coverage for the new route.
* Define validation steps that account for the missing .NET 8 runtime.

## Scope and Success Criteria

* Scope: API model, controller registration and routing, controller-local stub
  data, and one route-level integration test. Persistence, filtering, and a
  service or repository abstraction are out of scope.
* Assumptions: The proposed route and five-field response shape are acceptable
  to the API consumer; the endpoint is intentionally a stub; the application
  remains a .NET 8 ASP.NET Core application.
* Success Criteria:
  * `GET /api/warehouse-locations` returns HTTP 200 and a JSON array.
  * Each item contains `id`, `zone`, `aisle`, `rack`, and `shelf`.
  * The existing weather endpoint and `WebApplicationFactory<Program>` setup
    continue to work.
  * The test project builds, and tests execute when the .NET 8 runtime is
    available.

## Outline

1. Confirm the route and response contract.
2. Add the public response model.
3. Enable and map MVC controllers.
4. Add the deterministic controller-local stub.
5. Add the integration test.
6. Build and run tests, recording the runtime prerequisite if still blocked.

## Potential Next Research

* Confirm the public route and response schema with the API consumer.
  * Reasoning: The repository contains no warehouse-location contract, so the
    route and field names are inferred from the existing research.
  * Reference: `README.md:1-2`; research note
    `.copilot-tracking/research/2026-09-17/warehouse-location-controller-research.md`.

## Research Executed

### File Analysis

* `WarehouseWebApi/Program.cs:1-28`
  * The app currently uses minimal APIs, registers no MVC services, maps only
    `/weatherforecast`, and exposes `public partial class Program` for tests.
* `WarehouseWebApi/WarehouseWebApi.csproj:1-10`
  * The application already targets `net8.0` with the ASP.NET Core Web SDK.
* `WarehouseWebApi.Tests/WarehouseControllerTests.cs:9-71`
  * Tests use `WebApplicationFactory<Program>`, an in-process `HttpClient`,
    HTTP assertions, JSON deserialization, and test-local DTOs.
* `WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj:10-29`
  * Existing MVC testing, xUnit, test SDK, Coverlet, and project-reference
    dependencies are sufficient; no package changes are required.
* `WarehouseWebApi/Properties/launchSettings.json:4-21`
  * Existing HTTP and HTTPS launch profiles do not need route changes.

### Code Search Results

* `AddControllers`, `MapControllers`
  * No existing controller registration or controller route mapping was found.
* `WarehouseLocation`, `Controllers`, `Models`
  * No existing warehouse model, controller, service, or repository abstraction
    was found.

### External Research

* None. The plan is based on repository inspection and the verified subagent
  research note.

### Project Conventions

* Keep the existing minimal weather endpoint unchanged.
* Use nullable reference types and implicit usings already enabled by the test
  project.
* Do not introduce a new dependency or architectural layer for stub data.
* Preserve `public partial class Program` because the test fixture depends on it.

## Key Discoveries

### Project Structure

The API is intentionally small: `Program.cs` owns the existing endpoint and
there are no application `Controllers` or `Models` directories. The tests
already host the application in-process, making a route-level integration test
the narrowest discriminating check for both controller registration and route
mapping.

### Implementation Patterns

ASP.NET Core controller JSON uses the default web serializer, so PascalCase C#
properties will be emitted as camelCase JSON properties. The existing API
returns bare arrays with HTTP 200 rather than response envelopes. The new
controller should therefore return `Ok(Locations)` and avoid custom JSON
configuration.

### Complete Examples

```csharp
// WarehouseWebApi/Models/WarehouseLocation.cs
namespace WarehouseWebApi.Models;

public sealed record WarehouseLocation(
    string Id,
    string Zone,
    string Aisle,
    int Rack,
    int Shelf);
```

```csharp
// WarehouseWebApi/Controllers/WarehouseLocationsController.cs
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

### API and Schema Documentation

Proposed contract:

* Method and route: `GET /api/warehouse-locations`
* Status: `200 OK`
* Body: a bare JSON array of objects with `id`, `zone`, `aisle`, `rack`, and
  `shelf` properties.
* Data: three deterministic stub locations. Exact literals should only be
  asserted if they are confirmed as public contract.

## Technical Scenarios

### Controller-Local Stub Endpoint

**Requirements:**

* Minimal production footprint.
* Deterministic output for integration testing.
* No new package or persistence dependency.

**Preferred Approach:**

Add the response record and controller, then register MVC services and route
mapping in `Program.cs`. Keep the static array beside the action because the
repository currently has no data-access abstraction and the requested behavior
is stub-only.

```text
WarehouseWebApi/
  Controllers/WarehouseLocationsController.cs  (add)
  Models/WarehouseLocation.cs                  (add)
  Program.cs                                   (change)
WarehouseWebApi.Tests/
  WarehouseControllerTests.cs                  (change)
```

**Implementation Details:**

1. Confirm `/api/warehouse-locations` and the five response fields.
2. Add `WarehouseLocation` as a public sealed record.
3. Add `builder.Services.AddControllers()` immediately after creating the
   builder.
4. Add `app.MapControllers()` before `app.Run()`, preserving the weather route
   and partial `Program` declaration.
5. Add the explicit plural kebab-case controller route. Do not use
   `[controller]`, which would produce `/api/warehouselocations`.
6. Add a test that requests the exact route, asserts `200 OK`, deserializes a
   test-local DTO array, verifies three items, and checks non-empty strings and
   positive rack and shelf values.

#### Considered Alternatives

An injected repository or service would separate retrieval from HTTP and make a
future data source easier to replace. It is rejected for this iteration because
the project has no service or repository pattern, the data is static, and the
extra abstraction would increase the change footprint without adding behavior.
Introduce it when persistence, filtering, reuse, or a second consumer is
required.

## Risks and Discriminating Checks

* Missing `AddControllers()` or `MapControllers()` produces an unreachable
  route; the focused integration test directly detects this.
* Removing `public partial class Program` breaks the existing
  `WebApplicationFactory<Program>` fixture.
* Random or mutable stub data makes the contract unstable; use a static array.
* Exact stub literals should not be locked into tests until the API consumer
  confirms they are contractual.

## Validation Plan

1. Run `dotnet build WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj
   --no-restore` as the available static validation.
2. Run `dotnet test WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj
   --no-restore` after the .NET 8 runtime is installed or otherwise available.
3. Expected current environment limitation: SDK `10.0.401` is present, but
   `Microsoft.NETCore.App 8.0.0` is missing, so tests build and then stop before
   execution with zero tests run.

## Recommended Handoff

Implement the controller-local stub in the sequence above after confirming the
API contract. No package installation or launch-profile change is needed. The
first coding validation should be the focused integration test; if runtime
availability prevents execution, use the build result and record the blocked
test run explicitly.