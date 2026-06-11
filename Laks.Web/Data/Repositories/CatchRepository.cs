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
                   c.`Team`       AS Team,
                   c.`TeamName`   AS TeamName,
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

    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null)
    {
        const string sql = @"
            SELECT p.`Id`                        AS AnglerId,
                   p.`Name`                      AS AnglerName,
                   COUNT(c.`Id`)                 AS FishCount,
                   COALESCE(SUM(c.`Weight`), 0)  AS TotalWeightKg,
                   COALESCE(MAX(c.`Weight`), 0)  AS BestWeightKg
            FROM `Catch` c
            JOIN `Person` p ON p.`Id` = c.`PersonId`
            LEFT JOIN `season_config` sc
                   ON sc.`Year` = @Year
                  AND sc.`GroupNumber` = @GroupNumber
            WHERE YEAR(c.`Date`) = @Year
              AND (@GroupNumber IS NULL OR c.`Date` BETWEEN sc.`StartDate` AND sc.`EndDate`)
            GROUP BY p.`Id`, p.`Name`
            ORDER BY TotalWeightKg DESC, FishCount DESC, p.`Name`";

        using var conn = _db.CreateConnection();
        var rows = (await conn.QueryAsync<LeaderboardEntry>(sql, new { Year = year, GroupNumber = groupNumber })).ToList();

        for (var i = 0; i < rows.Count; i++)
        {
            rows[i].Rank = i + 1;
        }

        return rows;
    }

    public async Task<GroupSummary?> GetGroupSummaryAsync(int year, int groupNumber)
    {
        const string sql = @"
            SELECT sc.`Year`                     AS Year,
                   sc.`GroupNumber`              AS GroupNumber,
                   sc.`StartDate`                AS StartDate,
                   sc.`EndDate`                  AS EndDate,
                   COUNT(c.`Id`)                 AS FishCount,
                   COALESCE(SUM(c.`Weight`), 0)  AS TotalWeightKg,
                   COALESCE(MAX(c.`Weight`), 0)  AS BestWeightKg
            FROM `season_config` sc
            LEFT JOIN `Catch` c
                   ON YEAR(c.`Date`) = sc.`Year`
                  AND c.`Date` BETWEEN sc.`StartDate` AND sc.`EndDate`
            WHERE sc.`Year` = @Year
              AND sc.`GroupNumber` = @GroupNumber
            GROUP BY sc.`Year`, sc.`GroupNumber`, sc.`StartDate`, sc.`EndDate`";

        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<GroupSummary>(sql, new { Year = year, GroupNumber = groupNumber });
    }

    public async Task<SeasonSummary?> GetSeasonSummaryAsync(int year)
    {
        const string sql = @"
            SELECT COUNT(c.`Id`)                         AS TotalFish,
                   COALESCE(SUM(c.`Weight`), 0)          AS TotalWeightKg,
                   COALESCE(AVG(c.`Weight`), 0)          AS AvgWeightKg,
                   COALESCE(MAX(c.`Weight`), 0)          AS BiggestFishKg,
                   COALESCE(MAX(CASE WHEN c.`Weight` = mx.MaxWeight THEN p.`Name` END), '') AS BiggestFishAngler,
                   COUNT(DISTINCT c.`PersonId`)          AS ActiveAnglers,
                   (SELECT COUNT(DISTINCT pt.`PersonId`) FROM `Participant` pt WHERE pt.`Year` = @Year) AS TotalAnglers
            FROM `Catch` c
            JOIN `Person` p ON p.`Id` = c.`PersonId`
            JOIN (
                SELECT COALESCE(MAX(`Weight`), 0) AS MaxWeight
                FROM `Catch`
                WHERE YEAR(`Date`) = @Year
            ) mx
            WHERE YEAR(c.`Date`) = @Year";

        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<SeasonSummary>(sql, new { Year = year });
    }

    public async Task<AllTimeRecords?> GetAllTimeRecordsAsync()
    {
        const string biggestSql = @"
            SELECT c.`Weight` AS BiggestFishKg,
                   p.`Name`   AS BiggestFishAngler,
                   YEAR(c.`Date`) AS BiggestFishYear
            FROM `Catch` c
            JOIN `Person` p ON p.`Id` = c.`PersonId`
            ORDER BY c.`Weight` DESC
            LIMIT 1";

        const string prolificSql = @"
            SELECT p.`Name` AS MostProlificAngler,
                   COUNT(c.`Id`) AS MostProlificFishCount
            FROM `Catch` c
            JOIN `Person` p ON p.`Id` = c.`PersonId`
            GROUP BY p.`Id`, p.`Name`
            ORDER BY MostProlificFishCount DESC
            LIMIT 1";

        const string bestSeasonSql = @"
            SELECT YEAR(c.`Date`) AS BestSeasonYear,
                   COUNT(c.`Id`) AS BestSeasonFishCount,
                   COALESCE(SUM(c.`Weight`), 0) AS BestSeasonTotalKg
            FROM `Catch` c
            GROUP BY YEAR(c.`Date`)
            ORDER BY BestSeasonFishCount DESC
            LIMIT 1";

        using var conn = _db.CreateConnection();
        var biggest = await conn.QueryFirstOrDefaultAsync<AllTimeRecords>(biggestSql);
        var prolific = await conn.QueryFirstOrDefaultAsync<AllTimeRecords>(prolificSql);
        var bestSeason = await conn.QueryFirstOrDefaultAsync<AllTimeRecords>(bestSeasonSql);

        if (biggest is null && prolific is null && bestSeason is null)
        {
            return null;
        }

        return new AllTimeRecords
        {
            BiggestFishKg = biggest?.BiggestFishKg ?? 0,
            BiggestFishAngler = biggest?.BiggestFishAngler ?? string.Empty,
            BiggestFishYear = biggest?.BiggestFishYear ?? 0,
            MostProlificAngler = prolific?.MostProlificAngler ?? string.Empty,
            MostProlificFishCount = prolific?.MostProlificFishCount ?? 0,
            BestSeasonYear = bestSeason?.BestSeasonYear ?? 0,
            BestSeasonFishCount = bestSeason?.BestSeasonFishCount ?? 0,
            BestSeasonTotalKg = bestSeason?.BestSeasonTotalKg ?? 0
        };
    }

    public async Task<IEnumerable<CatchLocation>> GetCatchLocationsAsync(int? year = null)
    {
        const string sql = @"
            SELECT c.`Id`         AS CatchId,
                   c.`Latitude`   AS Latitude,
                   c.`Longitude`  AS Longitude,
                   c.`Weight`     AS WeightKg,
                   p.`Name`       AS AnglerName,
                   c.`Type`       AS CatchType,
                   c.`Location`   AS Location,
                   c.`Bait`       AS Bait,
                   c.`Date`       AS CatchDate,
                   YEAR(c.`Date`) AS SeasonYear
            FROM `Catch` c
            JOIN `Person` p ON p.`Id` = c.`PersonId`
            WHERE (@Year IS NULL OR YEAR(c.`Date`) = @Year)
              AND c.`Latitude` <> 0
              AND c.`Longitude` <> 0
            ORDER BY c.`Date` DESC, c.`Time` DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CatchLocation>(sql, new { Year = year });
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

    public async Task<IEnumerable<BiggestSalmonPerTeam>> GetBiggestSalmonPerTeamAsync(int? year = null)
    {
        const string sql = @"
            SELECT c.`TeamName`                      AS TeamName,
                   MAX(c.`Weight`)                   AS BiggestSalmonKg,
                   (SELECT p2.`Name`
                    FROM `Catch` c2
                    JOIN `Person` p2 ON p2.`Id` = c2.`PersonId`
                    WHERE c2.`TeamName` = c.`TeamName`
                      AND c2.`Type` = 'Laks'
                      AND (@Year IS NULL OR YEAR(c2.`Date`) = @Year)
                    ORDER BY c2.`Weight` DESC
                    LIMIT 1)                         AS AnglerName,
                   COUNT(c.`Id`)                     AS TotalSalmonCount,
                   COALESCE(AVG(c.`Weight`), 0)      AS AvgSalmonWeightKg
            FROM   `Catch` c
            WHERE  c.`TeamName` IS NOT NULL
              AND  c.`TeamName` <> ''
              AND  c.`Type` = 'Laks'
              AND  (@Year IS NULL OR YEAR(c.`Date`) = @Year)
            GROUP  BY c.`TeamName`
            ORDER  BY BiggestSalmonKg DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<BiggestSalmonPerTeam>(sql, new { Year = year });
    }

    public async Task<IEnumerable<CatchesPerWeek>> GetCatchesPerWeekAsync()
    {
        // WEEK(date, 3) = ISO-8601 week number, matching Danish/Norwegian convention.
        // Limit to the 5 most recent years to bound the result set.
        const string sql = @"
            SELECT YEAR(c.`Date`)    AS SeasonYear,
                   WEEK(c.`Date`, 3) AS WeekNumber,
                   COUNT(c.`Id`)     AS TotalCatches
            FROM   `Catch` c
            WHERE  YEAR(c.`Date`) >= (SELECT MAX(YEAR(c2.`Date`)) - 4 FROM `Catch` c2)
            GROUP  BY YEAR(c.`Date`), WEEK(c.`Date`, 3)
            ORDER  BY SeasonYear, WeekNumber";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CatchesPerWeek>(sql);
    }

    public async Task<IEnumerable<CatchesByHour>> GetCatchesByHourAsync(int? year = null)
    {
        const string sql = @"
            SELECT HOUR(c.`Time`) AS Hour,
                   COUNT(c.`Id`)  AS TotalCatches
            FROM   `Catch` c
            WHERE  (@Year IS NULL OR YEAR(c.`Date`) = @Year)
            GROUP  BY HOUR(c.`Time`)
            ORDER  BY Hour";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CatchesByHour>(sql, new { Year = year });
    }

    public async Task<IEnumerable<CatchesByWaterLevel>> GetCatchesByWaterLevelAsync(int? year = null)
    {
        // 0.25 m bands; catches without a recorded water level are excluded.
        const string sql = @"
            SELECT FLOOR(c.`WaterLevel` / 0.25) * 0.25 AS BandStartM,
                   COUNT(c.`Id`)                       AS TotalCatches
            FROM   `Catch` c
            WHERE  c.`WaterLevel` IS NOT NULL
              AND  (@Year IS NULL OR YEAR(c.`Date`) = @Year)
            GROUP  BY FLOOR(c.`WaterLevel` / 0.25) * 0.25
            ORDER  BY BandStartM";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CatchesByWaterLevel>(sql, new { Year = year });
    }
}
