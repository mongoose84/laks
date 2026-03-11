using Dapper;
using Laks.Web.Models;

namespace Laks.Web.Data.Repositories;

public class CatchRepository : ICatchRepository
{
    private readonly IDbConnectionFactory _db;

    private const string CatchProjection = @"
            SELECT c.`Id`         AS Id,
                   c.`PersonId`   AS AnglerId,
                   YEAR(c.`Date`) AS SeasonYear,
                   c.`Date`       AS CatchDate,
                   c.`Time`       AS CatchTime,
                   c.`Weight`     AS WeightKg,
                   c.`Location`   AS Location,
                   c.`Weather`    AS Weather,
                   c.`WaterLevel` AS WaterLevel,
                   c.`Bait`       AS Bait,
                   c.`Latitude`   AS Latitude,
                   c.`Longitude`  AS Longitude,
                   c.`Comment`    AS Notes,
                   c.`Type`       AS CatchType,
                   p.`Name`       AS AnglerName
            FROM   `Catch` c
            JOIN   `Person` p ON p.`Id` = c.`PersonId`";

    public CatchRepository(IDbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Catch>> GetRecentAsync(int count = 20)
    {
        var sql = CatchProjection + @"
            ORDER BY c.`Date` DESC, c.`Time` DESC
            LIMIT  @Count";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Catch>(sql, new { Count = count });
    }

    public async Task<IEnumerable<Catch>> GetByYearAsync(int year)
    {
        var sql = CatchProjection + @"
            WHERE  YEAR(c.`Date`) = @Year
            ORDER  BY c.`Date` DESC, c.`Time` DESC, p.`Name`";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Catch>(sql, new { Year = year });
    }

    public async Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId)
    {
        var sql = CatchProjection + @"
            WHERE  c.`PersonId` = @AnglerId
            ORDER  BY c.`Date` DESC, c.`Time` DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Catch>(sql, new { AnglerId = anglerId });
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM `Catch`");
    }

    public async Task<IEnumerable<CatchesPerYear>> GetCatchesPerYearAsync()
    {
        const string sql = @"
            SELECT YEAR(c.`Date`)            AS Year,
                   COUNT(c.`Id`)             AS TotalCatches,
                   COALESCE(SUM(c.`Weight`), 0) AS TotalWeightKg,
                   COALESCE(AVG(c.`Weight`), 0) AS AvgWeightKg
            FROM   `Catch` c
            GROUP  BY YEAR(c.`Date`)
            ORDER  BY YEAR(c.`Date`)";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CatchesPerYear>(sql);
    }

    public async Task<IEnumerable<CatchesPerAngler>> GetCatchesPerAnglerAsync(int? year = null)
    {
        const string sql = @"
            SELECT p.`Name`                    AS AnglerName,
                   COUNT(c.`Id`)               AS TotalCatches,
                   COALESCE(SUM(c.`Weight`), 0) AS TotalWeightKg,
                   COALESCE(MAX(c.`Weight`), 0) AS BestCatchKg
            FROM   `Catch` c
            JOIN   `Person` p ON p.`Id` = c.`PersonId`
            WHERE  (@Year IS NULL OR YEAR(c.`Date`) = @Year)
            GROUP  BY p.`Id`, p.`Name`
            ORDER  BY TotalCatches DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CatchesPerAngler>(sql, new { Year = year });
    }

    public async Task<IEnumerable<CatchesByType>> GetCatchesByTypeAsync(int? year = null)
    {
        const string sql = @"
            SELECT c.`Type`                                                AS TypeName,
                   COUNT(c.`Id`)                                            AS TotalCatches,
                   ROUND(COUNT(c.`Id`) * 100.0 / SUM(COUNT(c.`Id`)) OVER (), 1) AS Percentage
            FROM   `Catch` c
            WHERE  (@Year IS NULL OR YEAR(c.`Date`) = @Year)
            GROUP  BY c.`Type`
            ORDER  BY TotalCatches DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CatchesByType>(sql, new { Year = year });
    }
}
