namespace Laks.Web.Models;

public enum WaterLevelTrend
{
    Rising,
    Stable,
    Falling
}

public class WaterLevelSnapshot
{
    public decimal? LevelMeters { get; set; }
    public decimal? WaterTemperatureC { get; set; }
    public WaterLevelTrend Trend { get; set; } = WaterLevelTrend.Stable;
    public DateTime? MeasuredAt { get; set; }
    public DateTime? LastKnownAt { get; set; }
}

public class WaterLevelReading
{
    public DateTime Time { get; set; }
    public decimal LevelMeters { get; set; }
}
