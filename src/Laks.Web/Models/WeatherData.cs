namespace Laks.Web.Models;

public class WeatherData
{
    public decimal? AirTemperatureC { get; set; }
    public decimal? WindSpeedMs { get; set; }
    public string WindDirection { get; set; } = string.Empty;
    public string WeatherSymbol { get; set; } = string.Empty;
    public DateTime? MeasuredAt { get; set; }
}
