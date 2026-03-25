using MySql.Data.MySqlClient;

namespace Laks.Importer;

public class FishDatabase(string connectionString)
{
    public async Task AddPersonAsync(string name)
    {
        const string sql = "INSERT IGNORE INTO `Person` (`Name`) VALUES (@Name)";

        await using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", name);
        await cmd.ExecuteNonQueryAsync();

        Console.WriteLine($"Added person: {name}");
    }

    public async Task<int> AddCatchesAsync(IList<CatchModel> catches)
    {
        var added = 0;
        foreach (var catchModel in catches)
        {
            try
            {
                await AddCatchAsync(catchModel);
                added++;
            }
            catch (MySqlException)
            {
                // Duplicate or constraint violation — skip
            }
        }
        return added;
    }

    private async Task AddCatchAsync(CatchModel catchModel)
    {
        const string sql = """
            INSERT INTO `Catch`
                (`PersonId`, `Date`, `Time`, `Weight`, `Location`, `Weather`,
                 `WaterLevel`, `Bait`, `Latitude`, `Longitude`, `Comment`, `Type`)
            VALUES
                (@PersonId, @Date, @Time, @Weight, @Location, @Weather,
                 @WaterLevel, @Bait, @Latitude, @Longitude, @Comment, @Type)
            """;

        await using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PersonId", catchModel.PersonId);
        cmd.Parameters.AddWithValue("@Date", catchModel.DateAndTime.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@Time", catchModel.DateAndTime.TimeOfDay.ToString());
        cmd.Parameters.AddWithValue("@Weight", catchModel.Weight);
        cmd.Parameters.AddWithValue("@Location", catchModel.Location);
        cmd.Parameters.AddWithValue("@Weather", catchModel.Weather);
        cmd.Parameters.AddWithValue("@WaterLevel", catchModel.WaterLevel);
        cmd.Parameters.AddWithValue("@Bait", catchModel.Bait);
        cmd.Parameters.AddWithValue("@Latitude", catchModel.Latitude);
        cmd.Parameters.AddWithValue("@Longitude", catchModel.Longitude);
        cmd.Parameters.AddWithValue("@Comment", catchModel.Comment);
        cmd.Parameters.AddWithValue("@Type", catchModel.Type);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Dictionary<string, int>> GetAllNamesAsync()
    {
        const string sql = "SELECT `Id`, `Name` FROM `Person`";
        var names = new Dictionary<string, int>();

        await using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names[reader.GetString(1)] = reader.GetInt32(0);
        }

        return names;
    }
}
