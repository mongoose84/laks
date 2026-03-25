using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Laks.Web.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Laks.Web.Services;

public class WaterLevelService : IWaterLevelService
{
    private const string SnapshotCacheKey = "dashboard-water-snapshot";
    private const string ReadingsCacheKey = "dashboard-water-24h";
    private const string TemperatureCacheKey = "dashboard-water-temperature";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string StationId = "15.61.0";
    private const int WaterLevelParameter = 1000;
    private const int WaterTemperatureParameter = 1003;

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WaterLevelService> _logger;

    public WaterLevelService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<WaterLevelService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<WaterLevelSnapshot?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<WaterLevelSnapshot>(SnapshotCacheKey, out var cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var readings = await GetLast24HoursAsync(cancellationToken);
            var latest = readings.LastOrDefault();
            if (latest is null)
            {
                return null;
            }

            var priorLevel = FindComparisonLevel(readings, latest.Time.AddHours(-3));
            var waterTemp = await GetLatestObservationValueAsync(WaterTemperatureParameter, TemperatureCacheKey, cancellationToken);

            var snapshot = new WaterLevelSnapshot
            {
                LevelMeters = latest.LevelMeters,
                Trend = CalculateTrend(latest.LevelMeters, priorLevel),
                WaterTemperatureC = waterTemp,
                MeasuredAt = latest.Time,
                LastKnownAt = latest.Time
            };

            _cache.Set(SnapshotCacheKey, snapshot, CacheDuration);
            return snapshot;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load water level snapshot");
            return null;
        }
    }

    public async Task<IReadOnlyList<WaterLevelReading>> GetLast24HoursAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<IReadOnlyList<WaterLevelReading>>(ReadingsCacheKey, out var cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var rows = await GetObservationSeriesAsync(WaterLevelParameter, cancellationToken);
            _cache.Set(ReadingsCacheKey, rows, CacheDuration);
            return rows;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load 24h water level readings");
            return [];
        }
    }

    public static WaterLevelTrend CalculateTrend(decimal latestLevel, decimal? priorLevel)
    {
        if (priorLevel is null)
        {
            return WaterLevelTrend.Stable;
        }

        var delta = latestLevel - priorLevel.Value;
        if (delta > 0.02m)
        {
            return WaterLevelTrend.Rising;
        }

        if (delta < -0.02m)
        {
            return WaterLevelTrend.Falling;
        }

        return WaterLevelTrend.Stable;
    }

    private async Task<decimal?> GetLatestObservationValueAsync(int parameter, string cacheKey, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<decimal?>(cacheKey, out var cachedValue))
        {
            return cachedValue;
        }

        try
        {
            var readings = await GetObservationSeriesAsync(parameter, cancellationToken);
            var value = readings.LastOrDefault()?.LevelMeters;
            _cache.Set(cacheKey, value, CacheDuration);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load NVE observation parameter {Parameter}", parameter);
            _cache.Set<decimal?>(cacheKey, null, CacheDuration);
            return null;
        }
    }

    private async Task<List<WaterLevelReading>> GetObservationSeriesAsync(int parameter, CancellationToken cancellationToken)
    {
        var path = BuildObservationPath(parameter);
        _logger.LogInformation("Requesting NVE observations for parameter {Parameter} at {Path}", parameter, path);

        using var response = await _httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "NVE observations request failed for parameter {Parameter}. Status {StatusCode}. Body: {Body}",
                parameter,
                (int)response.StatusCode,
                errorBody);
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var readings = ParseObservationReadings(document.RootElement);
        _logger.LogInformation(
            "Parsed {Count} NVE observations for parameter {Parameter}",
            readings.Count,
            parameter);

        return readings;
    }

    private static string BuildObservationPath(int parameter)
    {
        var end = DateTime.UtcNow;
        var start = end.AddDays(-1);
        var referenceTime = $"{start:yyyy-MM-dd'T'HH':'mm':'ss'Z'}/{end:yyyy-MM-dd'T'HH':'mm':'ss'Z'}";

        return $"api/v1/Observations?StationId={StationId}&Parameter={parameter}&ResolutionTime=60&ReferenceTime={referenceTime}";
    }

    private static List<WaterLevelReading> ParseObservationReadings(JsonElement root)
    {
        var readings = new List<WaterLevelReading>();

        foreach (var observation in EnumerateObservationNodes(root))
        {
            if (!TryGetTimestamp(observation, out var time)
                || !TryGetObservationValue(observation, out var value))
            {
                continue;
            }

            readings.Add(new WaterLevelReading
            {
                Time = time,
                LevelMeters = value
            });
        }

        return readings
            .OrderBy(reading => reading.Time)
            .ToList();
    }

    private static IEnumerable<JsonElement> EnumerateObservationNodes(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                foreach (var nested in EnumerateObservationNodes(item))
                {
                    yield return nested;
                }
            }

            yield break;
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            yield break;
        }

        if (root.TryGetProperty("observations", out var observations)
            && observations.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in observations.EnumerateArray())
            {
                yield return item;
            }

            yield break;
        }

        if (root.TryGetProperty("data", out var data))
        {
            foreach (var nested in EnumerateObservationNodes(data))
            {
                yield return nested;
            }
        }
    }

    private static bool TryGetTimestamp(JsonElement observation, out DateTime time)
    {
        foreach (var propertyName in new[] { "time", "Time", "timestamp", "Timestamp" })
        {
            if (observation.TryGetProperty(propertyName, out var value)
                && value.ValueKind == JsonValueKind.String
                && DateTime.TryParse(
                    value.GetString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out time))
            {
                return true;
            }
        }

        time = default;
        return false;
    }

    private static bool TryGetObservationValue(JsonElement observation, out decimal value)
    {
        if (TryReadDecimalProperty(observation, "value", out value)
            || TryReadDecimalProperty(observation, "Value", out value))
        {
            return true;
        }

        if (observation.TryGetProperty("parameter", out var parameter)
            && parameter.ValueKind == JsonValueKind.Object
            && (TryReadDecimalProperty(parameter, "value", out value)
                || TryReadDecimalProperty(parameter, "Value", out value)))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryReadDecimalProperty(JsonElement element, string propertyName, out decimal value)
    {
        if (element.TryGetProperty(propertyName, out var propertyValue)
            && TryReadDecimal(propertyValue, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryReadDecimal(JsonElement value, out decimal parsed)
    {
        if (value.ValueKind == JsonValueKind.Number)
        {
            return value.TryGetDecimal(out parsed);
        }

        if (value.ValueKind == JsonValueKind.String
            && decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
        {
            return true;
        }

        parsed = default;
        return false;
    }

    private static decimal? FindComparisonLevel(IReadOnlyList<WaterLevelReading> readings, DateTime targetTime)
    {
        for (var index = readings.Count - 1; index >= 0; index--)
        {
            if (readings[index].Time <= targetTime)
            {
                return readings[index].LevelMeters;
            }
        }

        return readings.FirstOrDefault()?.LevelMeters;
    }
}
