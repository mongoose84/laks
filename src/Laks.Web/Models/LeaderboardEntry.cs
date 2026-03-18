namespace Laks.Web.Models;

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public int AnglerId { get; set; }
    public string AnglerName { get; set; } = string.Empty;
    public int FishCount { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal BestWeightKg { get; set; }
}
