namespace Laks.Web.Models;

public class SeasonSummary
{
    public int TotalFish { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal AvgWeightKg { get; set; }
    public decimal BiggestFishKg { get; set; }
    public string BiggestFishAngler { get; set; } = string.Empty;
    public int ActiveAnglers { get; set; }
    public int TotalAnglers { get; set; }
}
