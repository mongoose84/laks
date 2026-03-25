using System.Globalization;

namespace Laks.Importer;

public class DataConverter(IDictionary<string, int> names)
{
    private static readonly CultureInfo DanishCulture = new("da-DK", false);

    public List<CatchModel> GetCatches(IList<IList<object>> rows)
    {
        var catchList = new List<CatchModel>();
        foreach (var row in rows)
        {
            var catchModel = GetCatch(row);
            if (catchModel is not null)
                catchList.Add(catchModel);
        }
        return catchList;
    }

    private CatchModel? GetCatch(IList<object> row)
    {
        if (row.Count == 0)
            return null;

        var dateValue = row[0].ToString();
        if (string.IsNullOrWhiteSpace(dateValue))
            return null;

        var name = row[1].ToString()!;
        var personId = names[name];
        var dateTimeValue = $"{row[0]} {row[2]}";
        var dateTime = DateTime.Parse(dateTimeValue, DanishCulture);
        var weight = float.Parse(row[3].ToString()!, DanishCulture);
        var type = row[4].ToString()!;
        var locationValue = row[5].ToString()!;
        var weather = row[6].ToString()!;
        var waterLevel = float.Parse(row[7].ToString()!, DanishCulture);
        var bait = row[8].ToString()!;
        var comment = row.Count > 9 ? row[9].ToString()! : string.Empty;

        var (lat, lon) = GetLocation(locationValue);

        return new CatchModel
        {
            PersonId = personId,
            PersonName = name,
            DateAndTime = dateTime,
            Weight = weight,
            Type = type,
            Weather = weather,
            WaterLevel = waterLevel,
            Bait = bait,
            Comment = comment,
            Location = locationValue,
            Latitude = lat,
            Longitude = lon
        };
    }

    private static (double lat, double lon) GetLocation(string locationValue) =>
        locationValue.ToLowerInvariant() switch
        {
            "1"              => (59.186981, 9.995239),
            "1a"             => (59.186824, 9.995149),
            "1c"             => (59.186676, 9.995257),
            "2"              => (59.186552, 9.994756),
            "4"              => (59.186799, 9.993195),
            "5"              => (59.186958, 9.992113),
            "4.5" or "4+5"   => (59.186891, 9.992537),
            "8"              => (59.187464, 9.993583),
            "foss"           => (59.187490, 9.994093),
            "hytterne"       => (59.186355, 9.995293),
            "pynten"         => (59.187351, 9.994373),
            "klipperne"      => (59.186450, 9.995789),
            "talerstolen"    => (59.186954, 9.996033),
            "walle" or "walthers hul" => (59.186570, 9.996678),
            _ => throw new ArgumentException($"Unknown location: '{locationValue}'")
        };
}
