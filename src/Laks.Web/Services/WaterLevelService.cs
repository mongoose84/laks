using System.Globalization;
using System.Text.Json;
using Dapper;
using Laks.Web.Data;
using Laks.Web.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Laks.Web.Services;

public class WaterLevelService : IWaterLevelService
{
    private const string SnapshotCacheKey = "dashboard-water-snapshot";
    private const string ReadingsCacheKey = "dashboard-water-24h";
    private const string TemperatureCacheKey = "dashboard-water-temperature";
    private const string LevelCacheKey = "dashboard-water-level-live";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IDbConnectionFactory _db;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WaterLevelService> _logger;

    public WaterLevelService(
        IDbConnectionFactory db,
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<WaterLevelService> logger)
    {
        _db = db;
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
            const string latestSql = @"
                SELECT `MeasuredTime` AS Time, `MeasuredLevel` AS LevelMeters
                FROM `WaterLevel`
                ORDER BY `MeasuredTime` DESC
                LIMIT 1";

            const string priorSql = @"
                SELECT `MeasuredLevel`
                FROM `WaterLevel`
                WHERE `MeasuredTime` <= DATE_SUB(UTC_TIMESTAMP(), INTERVAL 3 HOUR)
                ORDER BY `MeasuredTime` DESC
                LIMIT 1";

            WaterLevelReading? latest = null;
            decimal? priorLevel = null;

            using (var conn = _db.CreateConnection())
            {
                latest = await conn.QueryFirstOrDefaultAsync<WaterLevelReading>(latestSql);
                priorLevel = await conn.QueryFirstOrDefaultAsync<decimal?>(priorSql);
            }

            var nveLevel = await GetObservationValueAsync(1000, LevelCacheKey, cancellationToken);
            var waterTemp = await GetObservationValueAsync(1003, TemperatureCacheKey, cancellationToken);

            if (latest is null && !nveLevel.HasValue)
            {
                return null;
            }

            var effectiveLevel = nveLevel ?? latest?.LevelMeters;
            var measuredAt = latest?.Time ?? DateTime.UtcNow;

            var snapshot = new WaterLevelSnapshot
            {
                LevelMeters = effectiveLevel,
                Trend = effectiveLevel.HasValue ? CalculateTrend(effectiveLevel.Value, priorLevel) : WaterLevelTrend.Stable,
                WaterTemperatureC = waterTemp,
                MeasuredAt = measuredAt,
                LastKnownAt = measuredAt
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
            const string sql = @"
                SELECT `MeasuredTime` AS Time, `MeasuredLevel` AS LevelMeters
                FROM `WaterLevel`
                WHERE `MeasuredTime` >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL 24 HOUR)
                ORDER BY `MeasuredTime` ASC";

            using var conn = _db.CreateConnection();
            var rows = (await conn.QueryAsync<WaterLevelReading>(sql)).ToList();
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

    private async Task<decimal?> GetObservationValueAsync(int parameter, string cacheKey, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<decimal?>(cacheKey, out var cachedValue))
        {
            return cachedValue;
        }

        try
        {
            using var response = await _httpClient.GetAsync(
                $"api/v1/Observations?StationId=15.61.0&Parameter={parameter}&ResolutionTime=60",
                cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var value = FindFirstNumericValue(document.RootElement);
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

    private static decimal? FindFirstNumericValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if ((property.Name.Equals("value", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Equals("Value", StringComparison.OrdinalIgnoreCase))
                    && TryReadDecimal(property.Value, out var directValue))
                {
                    return directValue;
                }

                var nested = FindFirstNumericValue(property.Value);
                if (nested.HasValue)
                {
                    return nested;
                }
            }

            return null;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = FindFirstNumericValue(item);
                if (nested.HasValue)
                {
                    return nested;
                }
            }

            return null;
        }

        return TryReadDecimal(element, out var primitiveValue) ? primitiveValue : null;
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
}
