using MySql.Data.MySqlClient;
using System.Data;

namespace Laks.Web.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
}
