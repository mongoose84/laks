namespace Laks.Web.Models;

public class AnglerProfileViewModel
{
    public Angler Angler { get; init; } = default!;
    public AnglerCareerStats Career { get; init; } = new();
    public AnglerCurrentSeasonStats? CurrentSeason { get; init; }
    public IReadOnlyList<AnglerSeasonRow> SeasonHistory { get; init; } = [];
    public IReadOnlyList<Catch> RecentCatches { get; init; } = [];
    public IReadOnlyList<AnglerBaitShare> TopBaits { get; init; } = [];
    public AnglerTimeOfDay TimeOfDay { get; init; } = new();
    public bool HasAnyCatches { get; init; }
}

public class AnglerCareerStats
{
    public int FishCount { get; init; }
    public decimal TotalWeightKg { get; init; }
    public decimal BestWeightKg { get; init; }
    public int BestWeightYear { get; init; }
    public int SeasonsActive { get; init; }
    public int? FirstSeasonYear { get; init; }
    public int? LastSeasonYear { get; init; }
}

public class AnglerCurrentSeasonStats
{
    public int Year { get; init; }
    public int FishCount { get; init; }
    public decimal TotalWeightKg { get; init; }
    public decimal BestWeightKg { get; init; }
    public int? Rank { get; init; }
    public int LeaderboardSize { get; init; }
    public int? GroupNumber { get; init; }
}

public class AnglerSeasonRow
{
    public int Year { get; init; }
    public int FishCount { get; init; }
    public decimal TotalWeightKg { get; init; }
    public decimal BestWeightKg { get; init; }
    public int? Rank { get; init; }
    public int LeaderboardSize { get; init; }
}

public class AnglerBaitShare
{
    public string Bait { get; init; } = string.Empty;
    public int CatchCount { get; init; }
    public decimal SharePct { get; init; }
}

public class AnglerTimeOfDay
{
    public int MorningCount { get; init; }
    public int DayCount { get; init; }
    public int EveningCount { get; init; }
    public int NightCount { get; init; }
    public string? WinnerBucket { get; init; }
    public decimal WinnerSharePct { get; init; }
}
