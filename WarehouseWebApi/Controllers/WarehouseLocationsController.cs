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