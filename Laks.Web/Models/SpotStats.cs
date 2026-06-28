namespace Laks.Web.Models;

public class SpotStats
{
    public string Location { get; set; } = string.Empty;
    public int TotalCatches { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal AvgWeightKg { get; set; }
    public decimal BiggestWeightKg { get; set; }
    public string BiggestAnglerName { get; set; } = string.Empty;
    public DateTime BiggestCatchDate { get; set; }
    public string TopBait { get; set; } = string.Empty;
    public decimal? BestWaterBandStartM { get; set; }
    public int? BestHour { get; set; }
}
