using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace WarehouseWebApi.Tests;

public class WarehouseControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WarehouseControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsFiveForecastEntries()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/weatherforecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var forecast = await response.Content.ReadFromJsonAsync<WeatherForecastResponse[]>();

        Assert.NotNull(forecast);
        Assert.Equal(5, forecast!.Length);

        var validSummaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        foreach (var item in forecast)
        {
            Assert.NotEqual(default, item.Date);
            Assert.Contains(item.Summary, validSummaries);
            Assert.InRange(item.TemperatureC, -20, 54);
            Assert.Equal(32 + (int)(item.TemperatureC / 0.5556), item.TemperatureF);
        }
    }

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

    [Theory]
    [InlineData(0, 32)]
    [InlineData(10, 50)]
    [InlineData(25, 77)]
    [InlineData(-10, 14)]
    public void TemperatureF_UsesExpectedConversionFormula(int temperatureC, int expectedFahrenheit)
    {
        var forecast = new WeatherForecastResponse
        {
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TemperatureC = temperatureC,
            Summary = "Warm"
        };

        Assert.Equal(expectedFahrenheit, forecast.TemperatureF);
    }

    private sealed class WeatherForecastResponse
    {
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public string? Summary { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    private sealed class WarehouseLocationResponse
    {
        public string? Id { get; set; }
        public string? Zone { get; set; }
        public string? Aisle { get; set; }
        public int Rack { get; set; }
        public int Shelf { get; set; }
    }
}
