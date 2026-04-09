namespace Laks.Importer;

public class CatchModel
{
    public int PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public DateTime DateAndTime { get; set; }
    public float Weight { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Weather { get; set; } = string.Empty;
    public float WaterLevel { get; set; }
    public string Bait { get; set; } = string.Empty;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;

    public override string ToString() => Bait;
}