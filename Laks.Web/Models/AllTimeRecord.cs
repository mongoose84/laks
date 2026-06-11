namespace Laks.Web.Models;

public class AllTimeRecords
{
    public decimal BiggestFishKg { get; set; }
    public string BiggestFishAngler { get; set; } = string.Empty;
    public int BiggestFishYear { get; set; }
    public int BiggestFishAnglerId { get; set; }

    public string MostProlificAngler { get; set; } = string.Empty;
    public int MostProlificFishCount { get; set; }
    public int MostProlificAnglerId { get; set; }

    public int BestSeasonYear { get; set; }
    public int BestSeasonFishCount { get; set; }
    public decimal BestSeasonTotalKg { get; set; }
}
