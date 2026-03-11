namespace Laks.Web.Models;

public class Catch
{
    public int Id { get; set; }
    public int AnglerId { get; set; }
    public int SeasonYear { get; set; }
    public DateTime CatchDate { get; set; }
    public TimeSpan CatchTime { get; set; }
    public decimal WeightKg { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Weather { get; set; } = string.Empty;
    public decimal? WaterLevel { get; set; }
    public string Bait { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string CatchType { get; set; } = string.Empty;

    public string? AnglerName { get; set; }
}
