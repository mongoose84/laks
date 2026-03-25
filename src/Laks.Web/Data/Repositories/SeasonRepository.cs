using Dapper;
using Laks.Web.Models;

namespace Laks.Web.Data.Repositories;

public class SeasonRepository : ISeasonRepository
{
    private readonly IDbConnectionFactory _db;

    private const string SeasonSummarySql = @"
        SELECT y.`Year`                           AS Year,
               MIN(c.`Date`)                      AS FirstCatchDate,
               MAX(c.`Date`)                      AS LastCatchDate,
               COUNT(c.`Id`)                      AS TotalCatches,
               COALESCE(p.ParticipantCount, 0)    AS ParticipantCount
        FROM (
            SELECT `Year` FROM `Participant`
            UNION
            SELECT YEAR(`Date`) AS `Year` FROM `Catch`
        ) y
        LEFT JOIN `Catch` c
               ON YEAR(c.`Date`) = y.`Year`
        LEFT JOIN (
            SELECT `Year`, COUNT(*) AS ParticipantCount
            FROM `Participant`
            GROUP BY `Year`
        ) p ON p.`Year` = y.`Year`
        GROUP BY y.`Year`, p.ParticipantCount";

    public SeasonRepository(IDbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<FishingSeason>> GetAllAsync()
    {
        var sql = SeasonSummarySql + " ORDER BY y.`Year` DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<FishingSeason>(sql);
    }

    public async Task<FishingSeason?> GetByYearAsync(int year)
    {
        var sql = SeasonSummarySql + @"
        HAVING y.`Year` = @Year
        ORDER BY y.`Year` DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<FishingSeason>(sql, new { Year = year });
    }

    public async Task<FishingSeason?> GetLatestAsync()
    {
        var sql = SeasonSummarySql + " ORDER BY y.`Year` DESC LIMIT 1";

        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<FishingSeason>(sql);
    }

    public async Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year)
    {
        const string sql = @"
            SELECT `Year` AS Year,
                   `GroupNumber` AS GroupNumber,
                   `StartDate` AS StartDate,
                   `EndDate` AS EndDate
            FROM `season_config`
            WHERE `Year` = @Year
            ORDER BY `GroupNumber`";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<SeasonConfig>(sql, new { Year = year });
    }

    public async Task<int?> GetAnglerGroupAsync(int year, int anglerId)
    {
        const string sql = @"
            SELECT `GroupNumber`
            FROM `Participant`
            WHERE `Year` = @Year AND `PersonId` = @AnglerId";

        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<int?>(sql, new { Year = year, AnglerId = anglerId });
    }
}