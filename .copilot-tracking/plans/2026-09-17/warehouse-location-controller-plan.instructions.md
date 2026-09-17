---
applyTo: '.copilot-tracking/changes/2026-09-17/warehouse-location-controller-changes.md'
---
<!-- markdownlint-disable-file -->
# Implementation Plan: Warehouse Location Controller

## Overview

Add a deterministic `GET /api/warehouse-locations` controller endpoint backed by a public response model, while preserving the existing weather endpoint and integration-test entry point.

## Objectives

### User Requirements

* Add a warehouse-location endpoint that returns stubbed location data. Source: `.copilot-tracking/research/2026-09-17/warehouse-location-implementation-plan-research.md`.
* Add focused integration coverage for the new route. Source: `.copilot-tracking/research/2026-09-17/warehouse-location-implementation-plan-research.md`.
* Preserve the existing application and test hosting behavior. Source: conversation context and verified repository research.

### Derived Objectives

* Register and map MVC controllers because the current application exposes only minimal API routes. Derived from `WarehouseWebApi/Program.cs:1-28`.
* Keep stub data deterministic and controller-local because no service or repository pattern exists and persistence is out of scope. Derived from the research alternatives evaluation.
* Use an explicit kebab-case route and a public model to stabilize the inferred response contract. Derived from the research API and schema findings.
* Make runtime availability an explicit validation prerequisite because the projects now target .NET 10. Derived from the verified validation results.

## Context Summary

### Project Files

* `WarehouseWebApi/Program.cs:1-28` - Minimal API startup, weather route, controller registration point, and public partial `Program` test hook.
* `WarehouseWebApi/WarehouseWebApi.csproj:1-10` - ASP.NET Core application targeting `net10.0`.
* `WarehouseWebApi.Tests/WarehouseControllerTests.cs:9-71` - Existing `WebApplicationFactory<Program>` integration-test fixture and test-local DTO pattern.
* `WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj:10-29` - MVC testing 10.0.1, xUnit, test SDK, and API project reference dependencies for `net10.0`.
* `WarehouseWebApi/Properties/launchSettings.json:4-21` - Existing HTTP and HTTPS launch profiles, which do not require changes.

### References

* `.copilot-tracking/research/2026-09-17/warehouse-location-implementation-plan-research.md` - Primary research and selected implementation path.
* `.copilot-tracking/research/2026-09-17/warehouse-location-controller-research.md` - Verified repository findings, implementation examples, risks, and validation results.

### Standards References

* `/Users/snosan/.vscode/extensions/ise-hve-essentials.hve-core-3.3.101/.github/instructions/hve-core/markdown.instructions.md` - Markdown structure and formatting requirements.
* `/Users/snosan/.vscode/extensions/ise-hve-essentials.hve-core-3.3.101/.github/instructions/hve-core/writing-style.instructions.md` - Technical writing conventions.

## Implementation Checklist

### [x] Implementation Phase 1: Confirm the inferred API contract

<!-- parallelizable: false -->

* [x] Step 1.1: Confirm `GET /api/warehouse-locations`, HTTP 200, bare-array response, and fields `id`, `zone`, `aisle`, `rack`, and `shelf` with the API consumer.
  * Details: `.copilot-tracking/details/2026-09-17/warehouse-location-controller-details.md` (Lines 12-34)
* [x] Step 1.2: Record any contract changes before implementation; if no response is available, proceed with the researched contract and note the assumption.
  * Details: `.copilot-tracking/details/2026-09-17/warehouse-location-controller-details.md` (Lines 36-55)

### [x] Implementation Phase 2: Add the API model and controller wiring

<!-- parallelizable: false -->

* [x] Step 2.1: Add `WarehouseWebApi/Models/WarehouseLocation.cs` as a public sealed record with the five response properties.
  * Details: `.copilot-tracking/details/2026-09-17/warehouse-location-controller-details.md` (Lines 61-84)
* [x] Step 2.2: Add `builder.Services.AddControllers()` and `app.MapControllers()` in `WarehouseWebApi/Program.cs` without changing the weather route or removing `public partial class Program`.
  * Details: `.copilot-tracking/details/2026-09-17/warehouse-location-controller-details.md` (Lines 86-107)
* [x] Step 2.3: Add `WarehouseWebApi/Controllers/WarehouseLocationsController.cs` with `[ApiController]`, explicit route `api/warehouse-locations`, deterministic static data, and `Ok(Locations)`.
  * Details: `.copilot-tracking/details/2026-09-17/warehouse-location-controller-details.md` (Lines 109-133)

### [x] Implementation Phase 3: Add focused integration coverage

<!-- parallelizable: false -->

* [x] Step 3.1: Extend `WarehouseWebApi.Tests/WarehouseControllerTests.cs` with a route-level test and private test DTO.
  * Details: `.copilot-tracking/details/2026-09-17/warehouse-location-controller-details.md` (Lines 139-162)
* [x] Step 3.2: Assert HTTP 200, three deterministic items, non-empty string fields, and positive rack and shelf values without locking exact literals unless the consumer confirms them as contractual.
  * Details: `.copilot-tracking/details/2026-09-17/warehouse-location-controller-details.md` (Lines 164-182)

### [x] Implementation Phase 4: Validate the implementation

<!-- parallelizable: false -->

* [x] Step 4.1: Run `dotnet build WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj --no-restore`.
  * Details: `.copilot-tracking/details/2026-09-17/warehouse-location-controller-details.md` (Lines 188-198)
* [x] Step 4.2: Run `dotnet test WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj --no-restore` under .NET 10.
  * Details: `.copilot-tracking/details/2026-09-17/warehouse-location-controller-details.md` (Lines 200-211)
* [x] Step 4.3: Fix only isolated validation issues; document the unrelated `TemperatureF` assertion failures without changing warehouse-location behavior.
  * Details: `.copilot-tracking/details/2026-09-17/warehouse-location-controller-details.md` (Lines 213-227)

## Planning Log

See `.copilot-tracking/plans/logs/2026-09-17/warehouse-location-controller-log.md` for discrepancy tracking, implementation paths considered, and follow-on work.

## Dependencies

* .NET 10 SDK and ASP.NET Core Web SDK used by the application.
* Existing `Microsoft.AspNetCore.Mvc.Testing` 10.0.1, xUnit, test SDK, and API project reference.
* API consumer confirmation, or an explicit decision to proceed with the inferred contract.
* .NET 10 runtime for executing integration tests. The current environment runs SDK `10.0.401` and runtime `10.0.12`.

## Success Criteria

* The exact route `GET /api/warehouse-locations` returns HTTP 200 with a bare JSON array. Traces to the user request and research API contract.
* Each response item serializes as `id`, `zone`, `aisle`, `rack`, and `shelf`. Traces to the research schema findings.
* The weather endpoint and `WebApplicationFactory<Program>` remain functional. Traces to `WarehouseWebApi/Program.cs:1-28` and existing tests.
* The test project builds successfully, and the full test command executes under .NET 10. The warehouse-location test passes; three pre-existing `TemperatureF` assertions remain failing outside this plan's scope.
