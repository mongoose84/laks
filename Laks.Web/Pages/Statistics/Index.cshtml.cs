using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Laks.Web.Pages.Statistics;

public class IndexModel : PageModel
{
    private readonly ICatchRepository _catches;
    private readonly ISeasonRepository _seasons;
    private readonly ILogger<IndexModel> _logger;

    // Chart data as JSON strings (safe for embedding in script tags)
    public string TrendLabelsJson { get; private set; } = "[]";
    public string TrendTotalsJson { get; private set; } = "[]";
    public string TrendWeightJson { get; private set; } = "[]";

    public string BarLabelsJson  { get; private set; } = "[]";
    public string BarCatchesJson { get; private set; } = "[]";
    public string BarWeightJson  { get; private set; } = "[]";

    public string PieLabelsJson { get; private set; } = "[]";
    public string PieDataJson   { get; private set; } = "[]";

    public string TeamLabelsJson      { get; private set; } = "[]";
    public string TeamBiggestJson     { get; private set; } = "[]";
    public string TeamAvgWeightJson { get; private set; } = "[]";
    public IEnumerable<BiggestSalmonPerTeam> TeamStats { get; private set; } = [];

    // Fishing-insight modules (descriptive statistics)
    public string SeasonProgressLabelsJson { get; private set; } = "[]";
    public string SeasonProgressSeriesJson { get; private set; } = "[]";
    public string HourDataJson             { get; private set; } = "[]";
    public bool HasHourData                { get; private set; }
    public string WaterBandLabelsJson      { get; private set; } = "[]";
    public string WaterBandDataJson        { get; private set; } = "[]";
    public bool HasWaterBandData           { get; private set; }

    // Per-spot statistics (all-time, ignores year filter)
    public IEnumerable<SpotStats> SpotStatsRows { get; private set; } = [];
    public string SpotChartLabelsJson { get; private set; } = "[]";
    public string SpotChartCountsJson { get; private set; } = "[]";
    public bool HasSpotData { get; private set; }

    public IEnumerable<FishingSeason> Seasons { get; private set; } = [];
    public int? SelectedYear { get; private set; }

    public IndexModel(ICatchRepository catches, ISeasonRepository seasons, ILogger<IndexModel> logger)
    {
        _catches = catches;
        _seasons = seasons;
        _logger  = logger;
    }

    public async Task OnGetAsync(int? year)
    {
        SelectedYear = year;

        try
        {
            Seasons = await _seasons.GetAllAsync();

            var trendTask   = _catches.GetCatchesPerYearAsync();
            var anglerTask  = _catches.GetCatchesPerAnglerAsync(year);
            var typeTask    = _catches.GetCatchesByTypeAsync(year);
            var teamTask    = _catches.GetBiggestSalmonPerTeamAsync(year);
            var weekTask    = _catches.GetCatchesPerWeekAsync();
            var hourTask    = _catches.GetCatchesByHourAsync(year);
            var bandTask    = _catches.GetCatchesByWaterLevelAsync(year);
            var spotTask    = _catches.GetCatchStatsPerSpotAsync();

            await Task.WhenAll(trendTask, anglerTask, typeTask, teamTask, weekTask, hourTask, bandTask, spotTask);

            // Trend line – all years
            var trend = (await trendTask).ToList();
            TrendLabelsJson = Serialize(trend.Select(x => x.Year.ToString()));
            TrendTotalsJson = Serialize(trend.Select(x => x.TotalCatches));
            TrendWeightJson = Serialize(trend.Select(x => x.TotalWeightKg));

            // Bar – per angler (optionally filtered by year)
            var anglers = (await anglerTask).ToList();
            BarLabelsJson  = Serialize(anglers.Select(x => x.AnglerName));
            BarCatchesJson = Serialize(anglers.Select(x => x.TotalCatches));
            BarWeightJson  = Serialize(anglers.Select(x => x.TotalWeightKg));

            // Pie – catch type distribution
            var types = (await typeTask).ToList();
            PieLabelsJson = Serialize(types.Select(x => x.TypeName));
            PieDataJson   = Serialize(types.Select(x => x.TotalCatches));

            // Team – biggest salmons per team
            var teams = (await teamTask).ToList();
            TeamStats          = teams;
            TeamLabelsJson     = Serialize(teams.Select(x => x.TeamName));
            TeamBiggestJson    = Serialize(teams.Select(x => x.BiggestSalmonKg));
            TeamAvgWeightJson = Serialize(teams.Select(x => x.AvgSalmonWeightKg));

            // Season-progress curve – catches per ISO week, recent seasons overlaid
            var (weekLabels, weekSeries) = BuildSeasonProgress(await weekTask);
            SeasonProgressLabelsJson = Serialize(weekLabels);
            SeasonProgressSeriesJson = Serialize(weekSeries);

            // Time of day – catches per hour, filled to a full 24-hour axis
            var hourBuckets = BuildHourBuckets(await hourTask);
            HasHourData  = hourBuckets.Any(v => v > 0);
            HourDataJson = Serialize(hourBuckets);

            // Water-level bands (0.25 m)
            var bands = (await bandTask).ToList();
            HasWaterBandData   = bands.Count > 0;
            WaterBandLabelsJson = Serialize(bands.Select(b => FormatBandLabel(b.BandStartM)));
            WaterBandDataJson   = Serialize(bands.Select(b => b.TotalCatches));

            // Per-spot statistics (all-time — no year filter applied)
            var spots = (await spotTask).ToList();
            SpotStatsRows       = spots;
            HasSpotData         = spots.Count > 0;
            SpotChartLabelsJson = Serialize(spots.Select(s => s.Location));
            SpotChartCountsJson = Serialize(spots.Select(s => s.TotalCatches));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load statistics page (year={Year})", year);
        }
    }

    private static readonly JsonSerializerOptions SerializeOptions = new() { WriteIndented = false };

    private static string Serialize<T>(T data) =>
        JsonSerializer.Serialize(data, SerializeOptions);

    /// <summary>
    /// Aligns catches-per-week rows on a shared week axis covering all included
    /// seasons. Weeks without catches are null so Chart.js leaves gaps; at most
    /// the <paramref name="maxSeasons"/> most recent seasons are included.
    /// </summary>
    internal static (List<string> Labels, List<SeasonProgressSeries> Series) BuildSeasonProgress(
        IEnumerable<CatchesPerWeek> rows, int maxSeasons = 5)
    {
        var included = rows
            .GroupBy(r => r.SeasonYear)
            .OrderByDescending(g => g.Key)
            .Take(maxSeasons)
            .OrderBy(g => g.Key)
            .ToList();

        if (included.Count == 0)
        {
            return ([], []);
        }

        var minWeek = included.Min(g => g.Min(r => r.WeekNumber));
        var maxWeek = included.Max(g => g.Max(r => r.WeekNumber));
        var weeks = Enumerable.Range(minWeek, maxWeek - minWeek + 1).ToList();
        var labels = weeks.Select(w => $"Uge {w}").ToList();

        var series = included
            .Select(g =>
            {
                var byWeek = g.ToDictionary(r => r.WeekNumber, r => r.TotalCatches);
                return new SeasonProgressSeries(
                    g.Key,
                    weeks.Select(w => byWeek.TryGetValue(w, out var c) ? c : (int?)null).ToList());
            })
            .ToList();

        return (labels, series);
    }

    /// <summary>Fills hour rows into a complete 24-slot array (index = hour of day).</summary>
    internal static int[] BuildHourBuckets(IEnumerable<CatchesByHour> rows)
    {
        var buckets = new int[24];
        foreach (var row in rows)
        {
            if (row.Hour is >= 0 and < 24)
            {
                buckets[row.Hour] = row.TotalCatches;
            }
        }

        return buckets;
    }

    /// <summary>Formats a 0.25 m band as a Danish range label, e.g. "1,25–1,50 m".</summary>
    internal static string FormatBandLabel(decimal bandStart)
    {
        var da = System.Globalization.CultureInfo.GetCultureInfo("da-DK");
        return $"{bandStart.ToString("0.00", da)}–{(bandStart + 0.25m).ToString("0.00", da)} m";
    }

    public sealed record SeasonProgressSeries(int Year, List<int?> Data);
}
