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
}
