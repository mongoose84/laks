using Dapper;
using Laks.Web.Models;

namespace Laks.Web.Data.Repositories;

public class TripRepository : ITripRepository
{
    private readonly IDbConnectionFactory _db;

    public TripRepository(IDbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Trip>> GetAllAsync()
    {
        const string sql = @"
            SELECT t.id, t.year, t.start_date AS StartDate, t.end_date AS EndDate,
                   t.river_name AS RiverName, t.location, t.description,
                   COUNT(c.id) AS TotalCatches
            FROM   trips t
            LEFT JOIN catches c ON c.trip_id = t.id
            GROUP  BY t.id
            ORDER  BY t.year DESC";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Trip>(sql);
    }

    public async Task<Trip?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT t.id, t.year, t.start_date AS StartDate, t.end_date AS EndDate,
                   t.river_name AS RiverName, t.location, t.description,
                   COUNT(c.id) AS TotalCatches
            FROM   trips t
            LEFT JOIN catches c ON c.trip_id = t.id
            WHERE  t.id = @Id
            GROUP  BY t.id";

        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Trip>(sql, new { Id = id });
    }

    public async Task<Trip?> GetByYearAsync(int year)
    {
        const string sql = @"
            SELECT t.id, t.year, t.start_date AS StartDate, t.end_date AS EndDate,
                   t.river_name AS RiverName, t.location, t.description,
                   COUNT(c.id) AS TotalCatches
            FROM   trips t
            LEFT JOIN catches c ON c.trip_id = t.id
            WHERE  t.year = @Year
            GROUP  BY t.id";

        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Trip>(sql, new { Year = year });
    }

    public async Task<Trip?> GetLatestAsync()
    {
        const string sql = @"
            SELECT t.id, t.year, t.start_date AS StartDate, t.end_date AS EndDate,
                   t.river_name AS RiverName, t.location, t.description,
                   COUNT(c.id) AS TotalCatches
            FROM   trips t
            LEFT JOIN catches c ON c.trip_id = t.id
            GROUP  BY t.id
            ORDER  BY t.year DESC
            LIMIT  1";

        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Trip>(sql);
    }
}
