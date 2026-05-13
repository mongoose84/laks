using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Laks.Web.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Laks.Web.Services;

public class WeatherService : IWeatherService
{
    private const string CacheKey = "dashboard-weather-current";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(HttpClient httpClient, IMemoryCache cache, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<WeatherData?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<WeatherData>(CacheKey, out var cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            using var response = await _httpClient.GetAsync(
                "weatherapi/locationforecast/2.0/compact?lat=59.186959&lon=9.993806",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var series = document.RootElement
                .GetProperty("properties")
                .GetProperty("timeseries")[0];

            var instantDetails = series
                .GetProperty("data")
                .GetProperty("instant")
                .GetProperty("details");

            var weather = new WeatherData
            {
                AirTemperatureC = TryGetDecimal(instantDetails, "air_temperature"),
                WindSpeedMs = TryGetDecimal(instantDetails, "wind_speed"),
                WindDirection = ToCardinal(TryGetDecimal(instantDetails, "wind_from_direction")),
                WeatherSymbol = ExtractWeatherSymbol(series),
                PrecipitationMm = ExtractPrecipitation(series),
                MeasuredAt = series.TryGetProperty("time", out var time)
                    ? time.GetDateTime()
                    : DateTime.UtcNow
            };

            _cache.Set(CacheKey, weather, CacheDuration);
            return weather;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Weather API unavailable");
            return null;
        }
    }

    private static decimal? TryGetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var d) => d,
            JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null
        };
    }

    private static string ExtractWeatherSymbol(JsonElement timeseriesEntry)
    {
        if (!timeseriesEntry.TryGetProperty("data", out var data))
        {
            return string.Empty;
        }

        if (data.TryGetProperty("next_1_hours", out var next1)
            && next1.TryGetProperty("summary", out var next1Summary)
            && next1Summary.TryGetProperty("symbol_code", out var next1Code)
            && next1Code.ValueKind == JsonValueKind.String)
        {
            return next1Code.GetString() ?? string.Empty;
        }

        if (data.TryGetProperty("next_6_hours", out var next6)
            && next6.TryGetProperty("summary", out var next6Summary)
            && next6Summary.TryGetProperty("symbol_code", out var next6Code)
            && next6Code.ValueKind == JsonValueKind.String)
        {
            return next6Code.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static decimal? ExtractPrecipitation(JsonElement timeseriesEntry)
    {
        if (!timeseriesEntry.TryGetProperty("data", out var data))
        {
            return null;
        }

        if (data.TryGetProperty("next_1_hours", out var next1)
            && next1.TryGetProperty("details", out var details)
            && details.TryGetProperty("precipitation_amount", out var amount)
            && amount.ValueKind == JsonValueKind.Number
            && amount.TryGetDecimal(out var value))
        {
            return value;
        }

        return null;
    }

    private static string ToCardinal(decimal? degrees)
    {
        if (degrees is null)
        {
            return string.Empty;
        }

        var dirs = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        var normalized = ((double)degrees.Value % 360 + 360) % 360;
        var index = (int)Math.Round(normalized / 45, MidpointRounding.AwayFromZero) % 8;
        return dirs[index];
    }
}
