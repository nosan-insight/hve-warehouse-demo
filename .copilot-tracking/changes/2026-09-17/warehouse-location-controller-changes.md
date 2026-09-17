<!-- markdownlint-disable-file -->
# Release Changes: Warehouse Location Controller

**Related Plan**: warehouse-location-controller-plan.instructions.md
**Implementation Date**: 2026-09-17

## Summary

Implemented a deterministic warehouse-location controller endpoint and its integration coverage. The inferred route and response shape were accepted for this implementation.

## Changes

### Added

* Added `WarehouseWebApi/Models/WarehouseLocation.cs` with the public sealed response record.
* Added `WarehouseWebApi/Controllers/WarehouseLocationsController.cs` with the explicit `GET /api/warehouse-locations` route and three deterministic stub records.

### Modified

* Registered and mapped MVC controllers in `WarehouseWebApi/Program.cs` while preserving the weather route and public partial `Program`.
* Extended `WarehouseWebApi.Tests/WarehouseControllerTests.cs` with route-level coverage for the warehouse-location endpoint and response invariants.
* Upgraded `WarehouseWebApi/WarehouseWebApi.csproj` and `WarehouseWebApi.Tests/WarehouseWebApi.Tests.csproj` to `net10.0` and aligned `Microsoft.AspNetCore.Mvc.Testing` to `10.0.1`.

### Removed

## Additional or Deviating Changes

* Proceeded with the researched route and response schema without external consumer confirmation. The assumption is recorded as ID-01 in the planning log.

## Release Summary

Phases 2, 3, and 4 are complete. The application and test project target .NET 10, and the test project build passed with no warnings or errors. The full test suite executed: 3 tests passed and 3 existing `TemperatureF_UsesExpectedConversionFormula` assertions failed. The warehouse-location integration test passed. The failures are unrelated to the warehouse-location implementation, so source behavior was left unchanged. The stale .NET 8 runtime blocker, WI-01, is resolved.
