using Dapper;
using Laks.Web.Models;

namespace Laks.Web.Data.Repositories;

public class AnglerRepository : IAnglerRepository
{
    private readonly IDbConnectionFactory _db;

    public AnglerRepository(IDbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Angler>> GetAllAsync()
    {
        const string sql = "SELECT id, name, country FROM anglers ORDER BY name";
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Angler>(sql);
    }

    public async Task<Angler?> GetByIdAsync(int id)
    {
        const string sql = "SELECT id, name, country FROM anglers WHERE id = @Id";
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Angler>(sql, new { Id = id });
    }
}
