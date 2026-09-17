namespace WarehouseWebApi.Models;

/// <summary>Represents a warehouse location returned by the API.</summary>
public sealed record WarehouseLocation(
    string Id,
    string Zone,
    string Aisle,
    int Rack,
    int Shelf);