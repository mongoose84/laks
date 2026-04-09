namespace Laks.Web.Models;

/// <summary>Catches per year – used for the trend-line chart.</summary>
public class CatchesPerYear
{
    public int Year { get; set; }
    public int TotalCatches { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal AvgWeightKg { get; set; }
}

/// <summary>Catches per angler – used for the bar-comparison chart.</summary>
public class CatchesPerAngler
{
    public string AnglerName { get; set; } = string.Empty;
    public int TotalCatches { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal BestCatchKg { get; set; }
}

/// <summary>Distribution by catch type – used for the pie/donut chart.</summary>
public class CatchesByType
{
    public string TypeName { get; set; } = string.Empty;
    public int TotalCatches { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>Biggest salmon per team – used for the team statistics bar chart.</summary>
public class BiggestSalmonPerTeam
{
    public string TeamName { get; set; } = string.Empty;
    public decimal BiggestSalmonKg { get; set; }
    public string AnglerName { get; set; } = string.Empty;
    public int TotalSalmonCount { get; set; }
    public decimal AvgSalmonWeightKg { get; set; }
}
