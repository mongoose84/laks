using Dapper;
using Laks.Web.Models;

namespace Laks.Web.Data.Repositories;

public class CatchRepository : ICatchRepository
{
    private readonly IDbConnectionFactory _db;

    public CatchRepository(IDbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Catch>> GetRecentAsync(int count = 20)
    {
        const string sql = @"
            SELECT c.id          AS Id,
                   c.trip_id     AS TripId,
                   c.angler_id   AS AnglerId,
                   c.species_id  AS SpeciesId,
                   c.catch_date  AS CatchDate,
                   c.weight_kg   AS WeightKg,
                   c.length_cm   AS LengthCm,
                   c.released    AS Released,
                   c.notes       AS Notes,
                   a.name        AS AnglerName,
                   s.name        AS SpeciesName,
                   t.year        AS TripYear,
                   t.river_name  AS RiverName
            FROM   catches c
            JOIN   anglers  a ON a.id = c.angler_id
            JOIN   species  s ON s.id = c.species_id
            JOIN   trips    t ON t.id = c.trip_id
            ORDER  BY c.catch_date DESC
            LIMIT  @Count";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Catch>(sql, new { Count = count });
    }

    public async Task<IEnumerable<Catch>> GetByTripAsync(int tripId)
    {
        const string sql = @"
            SELECT c.id, c.trip_id AS TripId, c.angler_id AS AnglerId,
                   c.species_id AS SpeciesId, c.catch_date AS CatchDate,
                   c.weight_kg AS WeightKg, c.length_cm AS LengthCm,
                   c.released AS Released, c.notes AS Notes,
                   a.name AS AnglerName, s.name AS SpeciesName,
                   t.year AS TripYear, t.river_name AS RiverName
            FROM   catches c
            JOIN   anglers a ON a.id = c.angler_id
            JOIN   species s ON s.id = c.species_id
            JOIN   trips   t ON t.id = c.trip_id
            WHERE  c.trip_id = @TripId
            ORDER  BY c.catch_date, a.name";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Catch>(sql, new { TripId = tripId });
    }

    public async Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId)
    {
        const string sql = @"
            SELECT c.id, c.trip_id AS TripId, c.angler_id AS AnglerId,
                   c.species_id AS SpeciesId, c.catch_date AS CatchDate,
                   c.weight_kg AS WeightKg, c.length_cm AS LengthCm,
                   c.released AS Released, c.notes AS Notes,
                   a.name AS AnglerName, s.name AS SpeciesName,
                   t.year AS TripYear, t.river_name AS RiverName
            FROM   catches c
            JOIN   anglers a ON a.id = c.angler_id
            JOIN   species s ON s.id = c.species_id
            JOIN   trips   t ON t.id = c.trip_id
            WHERE  c.angler_id = @AnglerId
            ORDER  BY c.catch_date DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Catch>(sql, new { AnglerId = anglerId });
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM catches");
    }

    public async Task<IEnumerable<CatchesPerYear>> GetCatchesPerYearAsync()
    {
        const string sql = @"
            SELECT t.year                    AS Year,
                   COUNT(c.id)               AS TotalCatches,
                   COALESCE(SUM(c.weight_kg), 0)  AS TotalWeightKg,
                   COALESCE(AVG(c.weight_kg), 0)  AS AvgWeightKg
            FROM   catches c
            JOIN   trips   t ON t.id = c.trip_id
            GROUP  BY t.year
            ORDER  BY t.year";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CatchesPerYear>(sql);
    }

    public async Task<IEnumerable<CatchesPerAngler>> GetCatchesPerAnglerAsync(int? year = null)
    {
        var sql = @"
            SELECT a.name                     AS AnglerName,
                   COUNT(c.id)                AS TotalCatches,
                   COALESCE(SUM(c.weight_kg), 0)   AS TotalWeightKg,
                   COALESCE(MAX(c.weight_kg), 0)   AS BestCatchKg
            FROM   catches c
            JOIN   anglers  a ON a.id = c.angler_id
            JOIN   trips    t ON t.id = c.trip_id
            WHERE  (@Year IS NULL OR t.year = @Year)
            GROUP  BY a.id, a.name
            ORDER  BY TotalCatches DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CatchesPerAngler>(sql, new { Year = year });
    }

    public async Task<IEnumerable<CatchesBySpecies>> GetCatchesBySpeciesAsync(int? year = null)
    {
        var sql = @"
            SELECT s.name                                                  AS SpeciesName,
                   COUNT(c.id)                                             AS TotalCatches,
                   ROUND(COUNT(c.id) * 100.0 / SUM(COUNT(c.id)) OVER (), 1) AS Percentage
            FROM   catches  c
            JOIN   species  s ON s.id = c.species_id
            JOIN   trips    t ON t.id = c.trip_id
            WHERE  (@Year IS NULL OR t.year = @Year)
            GROUP  BY s.id, s.name
            ORDER  BY TotalCatches DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CatchesBySpecies>(sql, new { Year = year });
    }
}
