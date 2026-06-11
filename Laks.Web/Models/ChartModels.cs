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

/// <summary>Catches per ISO week per season – used for the season-progress curve.</summary>
public class CatchesPerWeek
{
    public int SeasonYear { get; set; }
    public int WeekNumber { get; set; }
    public int TotalCatches { get; set; }
}

/// <summary>Catches per hour of day – used for the time-of-day chart.</summary>
public class CatchesByHour
{
    public int Hour { get; set; }
    public int TotalCatches { get; set; }
}

/// <summary>Catches per water-level band – used for the water-level chart.</summary>
public class CatchesByWaterLevel
{
    /// <summary>Lower bound of the band in metres (band width 0.25 m).</summary>
    public decimal BandStartM { get; set; }
    public int TotalCatches { get; set; }
}
