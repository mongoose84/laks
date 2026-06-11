using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Laks.Web.Pages.Anglers;

public class IndexModel : PageModel
{
    private readonly IAnglerRepository _anglers;
    private readonly ICatchRepository _catches;
    private readonly ISeasonRepository _seasons;
    private readonly ILogger<IndexModel> _logger;
    private readonly TimeProvider _timeProvider;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public AnglerProfileViewModel Profile { get; private set; } = default!;
    public int CurrentYear { get; private set; }

    public IndexModel(
        IAnglerRepository anglers,
        ICatchRepository catches,
        ISeasonRepository seasons,
        ILogger<IndexModel> logger,
        TimeProvider? timeProvider = null)
    {
        _anglers = anglers;
        _catches = catches;
        _seasons = seasons;
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        CurrentYear = _timeProvider.GetUtcNow().Year;

        var angler = await _anglers.GetByIdAsync(Id);
        if (angler is null)
        {
            return NotFound();
        }

        var catchesTask = SafeCallAsync(
            () => _catches.GetByAnglerAsync(Id),
            "load angler catches");

        var groupTask = SafeCallAsync(
            () => _seasons.GetAnglerGroupAsync(CurrentYear, Id),
            "load angler group");

        var currentYearLeaderboardTask = SafeCallAsync(
            () => _catches.GetLeaderboardAsync(CurrentYear),
            "load current year leaderboard");

        await Task.WhenAll(catchesTask, groupTask, currentYearLeaderboardTask);

        var allCatches = ((await catchesTask) ?? []).ToList();
        var groupNumber = await groupTask;
        var currentYearLeaderboard = ((await currentYearLeaderboardTask) ?? []).ToList();

        // Per-year rank: one leaderboard call per distinct season year the angler caught in.
        var distinctYears = allCatches
            .Select(c => c.SeasonYear)
            .Distinct()
            .ToList();

        var leaderboardTasks = distinctYears
            .Select(y => (Year: y, Task: SafeCallAsync(
                () => _catches.GetLeaderboardAsync(y),
                $"load leaderboard year {y}")))
            .ToList();

        await Task.WhenAll(leaderboardTasks.Select(t => t.Task));

        var leaderboardsByYear = new Dictionary<int, IReadOnlyList<LeaderboardEntry>>();
        foreach (var (year, task) in leaderboardTasks)
        {
            var result = await task;
            if (result is not null)
            {
                leaderboardsByYear[year] = result.ToList();
            }
        }

        var career = BuildCareerStats(allCatches);
        var seasonHistory = BuildSeasonHistory(allCatches, leaderboardsByYear, Id);
        var currentSeason = BuildCurrentSeasonStats(allCatches, CurrentYear, currentYearLeaderboard, groupNumber, Id);
        var topBaits = BuildTopBaits(allCatches);
        var timeOfDay = BuildTimeOfDay(allCatches);
        var recentCatches = allCatches.Take(10).ToList();

        Profile = new AnglerProfileViewModel
        {
            Angler = angler,
            Career = career,
            CurrentSeason = currentSeason,
            SeasonHistory = seasonHistory,
            RecentCatches = recentCatches,
            TopBaits = topBaits,
            TimeOfDay = timeOfDay,
            HasAnyCatches = allCatches.Count > 0
        };

        return Page();
    }

    internal static AnglerCareerStats BuildCareerStats(IReadOnlyList<Catch> catches)
    {
        if (catches.Count == 0)
        {
            return new AnglerCareerStats();
        }

        var bestCatch = catches.MaxBy(c => c.WeightKg);
        var distinctYears = catches.Select(c => c.SeasonYear).Distinct().OrderBy(y => y).ToList();

        return new AnglerCareerStats
        {
            FishCount = catches.Count,
            TotalWeightKg = catches.Sum(c => c.WeightKg),
            BestWeightKg = bestCatch?.WeightKg ?? 0m,
            BestWeightYear = bestCatch?.SeasonYear ?? 0,
            SeasonsActive = distinctYears.Count,
            FirstSeasonYear = distinctYears.Count > 0 ? distinctYears.First() : null,
            LastSeasonYear = distinctYears.Count > 0 ? distinctYears.Last() : null
        };
    }

    internal static IReadOnlyList<AnglerSeasonRow> BuildSeasonHistory(
        IReadOnlyList<Catch> catches,
        IReadOnlyDictionary<int, IReadOnlyList<LeaderboardEntry>> leaderboardsByYear,
        int anglerId)
    {
        var byYear = catches
            .GroupBy(c => c.SeasonYear)
            .OrderByDescending(g => g.Key)
            .Select(g =>
            {
                var year = g.Key;
                var yearCatches = g.ToList();
                int? rank = null;
                int leaderboardSize = 0;

                if (leaderboardsByYear.TryGetValue(year, out var lb))
                {
                    leaderboardSize = lb.Count;
                    var entry = lb.FirstOrDefault(e => e.AnglerId == anglerId);
                    rank = entry?.Rank;
                }

                return new AnglerSeasonRow
                {
                    Year = year,
                    FishCount = yearCatches.Count,
                    TotalWeightKg = yearCatches.Sum(c => c.WeightKg),
                    BestWeightKg = yearCatches.Max(c => c.WeightKg),
                    Rank = rank,
                    LeaderboardSize = leaderboardSize
                };
            })
            .ToList();

        return byYear;
    }

    internal static AnglerCurrentSeasonStats? BuildCurrentSeasonStats(
        IReadOnlyList<Catch> catches,
        int currentYear,
        IReadOnlyList<LeaderboardEntry> currentYearLeaderboard,
        int? groupNumber,
        int anglerId)
    {
        var currentYearCatches = catches.Where(c => c.SeasonYear == currentYear).ToList();

        // Only render strip if there are catches in current year OR angler is group-registered
        if (currentYearCatches.Count == 0 && !groupNumber.HasValue)
        {
            return null;
        }

        int? rank = null;
        int leaderboardSize = currentYearLeaderboard.Count;

        // Find this angler's rank using the explicit anglerId parameter
        if (currentYearCatches.Count > 0)
        {
            var entry = currentYearLeaderboard.FirstOrDefault(e => e.AnglerId == anglerId);
            rank = entry?.Rank;
        }

        return new AnglerCurrentSeasonStats
        {
            Year = currentYear,
            FishCount = currentYearCatches.Count,
            TotalWeightKg = currentYearCatches.Sum(c => c.WeightKg),
            BestWeightKg = currentYearCatches.Count > 0 ? currentYearCatches.Max(c => c.WeightKg) : 0m,
            Rank = rank,
            LeaderboardSize = leaderboardSize,
            GroupNumber = groupNumber
        };
    }

    internal static IReadOnlyList<AnglerBaitShare> BuildTopBaits(IReadOnlyList<Catch> catches, int top = 3)
    {
        var withBait = catches.Where(c => !string.IsNullOrWhiteSpace(c.Bait)).ToList();
        if (withBait.Count == 0)
        {
            return [];
        }

        var denominator = withBait.Count;

        // Group case-insensitively, display form = first non-empty trimmed value in source order (date-DESC)
        var grouped = withBait
            .GroupBy(c => c.Bait.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new
            {
                DisplayBait = g.First().Bait.Trim(),
                Count = g.Count(),
                Key = g.Key
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Take(top)
            .Select(x => new AnglerBaitShare
            {
                Bait = x.DisplayBait,
                CatchCount = x.Count,
                SharePct = Math.Round((decimal)x.Count / denominator * 100m, 1)
            })
            .ToList();

        return grouped;
    }

    internal static AnglerTimeOfDay BuildTimeOfDay(IReadOnlyList<Catch> catches)
    {
        if (catches.Count == 0)
        {
            return new AnglerTimeOfDay();
        }

        var morning = 0;
        var day = 0;
        var evening = 0;
        var night = 0;

        foreach (var c in catches)
        {
            var bucket = ClassifyHour(c.CatchTime.Hours);
            switch (bucket)
            {
                case "Morgen": morning++; break;
                case "Dag": day++; break;
                case "Aften": evening++; break;
                default: night++; break;
            }
        }

        var total = catches.Count;

        // Determine winner; tie order: Morgen > Dag > Aften > Nat
        string? winner = null;
        int winnerCount = 0;

        if (morning >= day && morning >= evening && morning >= night && morning > 0)
        {
            winner = "Morgen";
            winnerCount = morning;
        }
        else if (day >= evening && day >= night && day > 0)
        {
            winner = "Dag";
            winnerCount = day;
        }
        else if (evening >= night && evening > 0)
        {
            winner = "Aften";
            winnerCount = evening;
        }
        else if (night > 0)
        {
            winner = "Nat";
            winnerCount = night;
        }

        var winnerPct = winner is not null
            ? Math.Round((decimal)winnerCount / total * 100m, 1)
            : 0m;

        return new AnglerTimeOfDay
        {
            MorningCount = morning,
            DayCount = day,
            EveningCount = evening,
            NightCount = night,
            WinnerBucket = winner,
            WinnerSharePct = winnerPct
        };
    }

    internal static string ClassifyHour(int hour)
    {
        return hour switch
        {
            >= 4 and <= 9 => "Morgen",
            >= 10 and <= 15 => "Dag",
            >= 16 and <= 21 => "Aften",
            _ => "Nat"   // 22, 23, 0, 1, 2, 3
        };
    }

    private async Task<T?> SafeCallAsync<T>(Func<Task<T>> action, string operation)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Angler profile section failed: {Operation}", operation);
            return default;
        }
    }
}
