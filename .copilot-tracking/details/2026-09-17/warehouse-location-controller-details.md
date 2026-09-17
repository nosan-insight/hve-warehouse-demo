<!-- markdownlint-disable-file -->
# Implementation Details: Warehouse Location Controller

## Context Reference

Sources: `.copilot-tracking/research/2026-09-17/warehouse-location-implementation-plan-research.md`; `.copilot-tracking/research/2026-09-17/warehouse-location-controller-research.md`; `WarehouseWebApi/Program.cs:1-28`; `WarehouseWebApi.Tests/WarehouseControllerTests.cs:9-71`.

## Implementation Phase 1: Confirm the Inferred API Contract

<!-- parallelizable: false -->

### Step 1.1: Confirm the route and response shape

Confirm the proposed public contract before coding because the repository contains no existing warehouse-location schema. The selected default is `GET /api/warehouse-locations`, HTTP 200, a bare JSON array, and five fields: `id`, `zone`, `aisle`, `rack`, and `shelf`.

Files:

* No source file changes.

Discrepancy references:

* Addresses DR-01 in the planning log by making the inferred consumer contract explicit.

Success criteria:

* The API consumer confirms the route and field names, or the implementation owner records approval to proceed with the researched defaults.

Context references:

* `.copilot-tracking/research/2026-09-17/warehouse-location-implementation-plan-research.md` (API and Schema Documentation) - Proposed route and response contract.

Dependencies:

* Consumer confirmation or an explicit implementation decision.

### Step 1.2: Record contract deviations

If the consumer changes the route, status, envelope, or field names, update the implementation plan, details, and planning log before source edits. If no changes are requested, retain the selected controller-local stub path.

Files:

* `.copilot-tracking/plans/2026-09-17/warehouse-location-controller-plan.instructions.md` - Update requirements and success criteria if needed.
* `.copilot-tracking/plans/logs/2026-09-17/warehouse-location-controller-log.md` - Record deviations from research.

Success criteria:

* The plan and implementation details describe one consistent response contract.

Context references:

* `.copilot-tracking/research/2026-09-17/warehouse-location-implementation-plan-research.md` (Technical Scenarios) - Selected implementation path.

Dependencies:

* Step 1.1 completion.

## Implementation Phase 2: Add the API Model and Controller Wiring

<!-- parallelizable: false -->

### Step 2.1: Add the warehouse location model

Create a public sealed record so the controller has a reusable, strongly typed response shape. Use `Id`, `Zone`, `Aisle`, `Rack`, and `Shelf`; default ASP.NET Core web JSON serialization emits these as camelCase.

Files:

* `WarehouseWebApi/Models/WarehouseLocation.cs` - New public response record.

Discrepancy references:

* Addresses the model addition in the selected path; no deviation from research.

Success criteria:

* The model compiles under `net10.0`.
* The model exposes exactly the five planned response properties with the researched types.

Context references:

* `.copilot-tracking/research/2026-09-17/warehouse-location-implementation-plan-research.md` (Complete Examples) - Model shape.

Dependencies:

* Phase 1 contract decision.

### Step 2.2: Register and map controllers

Add MVC services immediately after `WebApplication.CreateBuilder(args)` and map controller endpoints before `app.Run()`. Preserve the existing weather route and `public partial class Program`, which the integration fixture requires.

Files:

* `WarehouseWebApi/Program.cs` - Add controller service registration and endpoint mapping.

Success criteria:

* The existing `/weatherforecast` route remains present.
* `public partial class Program` remains available to the test assembly.
* Controller route discovery is enabled.

Context references:

* `WarehouseWebApi/Program.cs:1-28` - Current startup and test hook.
* `.copilot-tracking/research/2026-09-17/warehouse-location-implementation-plan-research.md` (Risks and Discriminating Checks) - Missing registration produces an unreachable route.

Dependencies:

* Step 2.1 can be completed independently at the file level, but controller mapping must be present before route validation.

### Step 2.3: Add the deterministic controller

Create a sealed `ControllerBase` endpoint with `[ApiController]`, `[Route("api/warehouse-locations")]`, and `[HttpGet]`. Keep a static array of three locations in the controller and return `Ok(Locations)` to match the existing bare-array HTTP 200 convention.

Files:

* `WarehouseWebApi/Controllers/WarehouseLocationsController.cs` - New controller and stub data.

Discrepancy references:

* Addresses IP-01 by selecting a controller-local stub rather than an injected repository.

Success criteria:

* The exact route is `/api/warehouse-locations`, not the implicit `/api/warehouselocations`.
* Responses are deterministic and contain three valid location records.
* No new package or repository/service abstraction is required beyond the MVC controller registration in `Program.cs`.

Context references:

* `.copilot-tracking/research/2026-09-17/warehouse-location-implementation-plan-research.md` (Complete Examples) - Controller structure and data.

Dependencies:

* Steps 2.1 and 2.2.

## Implementation Phase 3: Add Focused Integration Coverage

<!-- parallelizable: false -->

### Step 3.1: Add the route-level integration test

Extend the existing fixture-based test class beside the weather test. Create an `HttpClient`, request the exact route, assert `HttpStatusCode.OK`, deserialize into a private test-local DTO array, and retain the established JSON assertion style.

Files:

* `WarehouseWebApi.Tests/WarehouseControllerTests.cs` - New endpoint test and private DTO.

Discrepancy references:

* Addresses DR-02 by preserving the repository's established integration-test pattern rather than adding a new unit-test harness.

Success criteria:

* The test exercises the application through `WebApplicationFactory<Program>`.
* A missing controller registration or route mapping causes the focused test to fail.

Context references:

* `WarehouseWebApi.Tests/WarehouseControllerTests.cs:9-71` - Fixture and assertion conventions.

Dependencies:

* Phase 2 endpoint implementation.

### Step 3.2: Assert stable response invariants

Assert three returned records, non-empty `Id`, `Zone`, and `Aisle`, and positive `Rack` and `Shelf` values. Assert exact stub literals only if the API consumer confirms that data as part of the contract.

Files:

* `WarehouseWebApi.Tests/WarehouseControllerTests.cs` - Response invariants.

Success criteria:

* The test validates route reachability, status, shape, count, and value invariants without coupling to incidental stub text.

Context references:

* `.copilot-tracking/research/2026-09-17/warehouse-location-implementation-plan-research.md` (Risks and Discriminating Checks) - Determinism and contract guidance.

Dependencies:

* Step 3.1 completion.

## Implementation Phase 4: Validate the Implementation

<!-- parallelizable: false -->

### Step 4.1: Build the test project

Run the available static validation command after source changes.

Validation commands:

* `dotnet build WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj --no-restore` - Build the API and test project.

Success criteria:

* The project builds without errors.

### Step 4.2: Execute the test project

Run the full test command under the current `net10.0` target. The environment provides SDK `10.0.401` and runtime `10.0.12`.

Validation commands:

* `dotnet test WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj --no-restore` - Execute existing and new integration tests.

Success criteria:

* The new route test executes and passes.
* Any unrelated existing test failures are recorded without changing warehouse-location behavior.

### Step 4.3: Resolve validation findings

Apply only isolated fixes within the planned files. If validation exposes a contract or architecture change, stop implementation and create follow-on planning rather than expanding scope silently.

Files:

* `WarehouseWebApi/Program.cs`
* `WarehouseWebApi/Models/WarehouseLocation.cs`
* `WarehouseWebApi/Controllers/WarehouseLocationsController.cs`
* `WarehouseWebApi.Tests/WarehouseControllerTests.cs`

Success criteria:

* No new compile errors remain.
* The final validation result and any environment blocker are reflected in the planning log.

## Dependencies

* Phase 1 contract decision precedes source changes.
* Phase 2 precedes Phase 3 integration coverage.
* .NET 10 runtime availability is required for test execution but not for static build validation.

## Success Criteria

* The endpoint contract is implemented consistently across model, controller, and test.
* The application builds and the integration test executes when runtime prerequisites are met.
* No unrelated files or architectural layers are introduced.
