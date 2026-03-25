using System.Text.Json;
using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Laks.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Laks.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ISeasonRepository _seasons;
    private readonly ICatchRepository _catches;
    private readonly IWeatherService _weatherService;
    private readonly IWaterLevelService _waterLevelService;
    private readonly ILogger<IndexModel> _logger;

    public int CurrentYear { get; private set; }
    public WeatherData? CurrentWeather { get; private set; }
    public WaterLevelSnapshot? CurrentWaterLevel { get; private set; }
    public SeasonDay SeasonDay { get; private set; } = new();
    public IEnumerable<LeaderboardEntry> Leaderboard { get; private set; } = [];
    public List<GroupSummary> GroupSummaries { get; private set; } = [];
    public int? SelectedGroup { get; private set; }
    public IEnumerable<Catch> RecentCatches { get; private set; } = [];
    public SeasonSummary? CurrentSeasonSummary { get; private set; }
    public AllTimeRecords? Records { get; private set; }
    public string WaterLevelChartJson { get; private set; } = "[]";
    public string CatchLocationsCurrentSeasonJson { get; private set; } = "[]";
    public string CatchLocationsAllTimeJson { get; private set; } = "[]";
    public List<SeasonConfig> AvailableGroups { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public int? GroupNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public string LeaderboardScope { get; set; } = "my-group";

    public IndexModel(
        ISeasonRepository seasons,
        ICatchRepository catches,
        IWeatherService weatherService,
        IWaterLevelService waterLevelService,
        ILogger<IndexModel> logger)
    {
        _seasons = seasons;
        _catches = catches;
        _weatherService = weatherService;
        _waterLevelService = waterLevelService;
        _logger = logger;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var latestSeason = await SafeCallAsync(() => _seasons.GetLatestAsync(), "load latest season");
        CurrentYear = latestSeason?.Year ?? DateTime.UtcNow.Year;

        var weatherTask = SafeCallAsync(() => _weatherService.GetCurrentAsync(cancellationToken), "load weather");
        var waterSnapshotTask = SafeCallAsync(() => _waterLevelService.GetCurrentAsync(cancellationToken), "load current water level");
        var waterReadingsTask = SafeCallAsync(() => _waterLevelService.GetLast24HoursAsync(cancellationToken), "load water level series");
        var seasonConfigTask = SafeCallAsync(() => _seasons.GetSeasonConfigAsync(CurrentYear), "load season config");
        var recentTask = SafeCallAsync(() => _catches.GetRecentAsync(10), "load recent catches");
        var seasonSummaryTask = SafeCallAsync(() => _catches.GetSeasonSummaryAsync(CurrentYear), "load season summary");
        var recordsTask = SafeCallAsync(() => _catches.GetAllTimeRecordsAsync(), "load all time records");
        var seasonLocationsTask = SafeCallAsync(() => _catches.GetCatchLocationsAsync(CurrentYear), "load current season map locations");
        var allTimeLocationsTask = SafeCallAsync(() => _catches.GetCatchLocationsAsync(), "load all-time map locations");

        await Task.WhenAll(
            weatherTask,
            waterSnapshotTask,
            waterReadingsTask,
            seasonConfigTask,
            recentTask,
            seasonSummaryTask,
            recordsTask,
            seasonLocationsTask,
            allTimeLocationsTask);

        CurrentWeather = await weatherTask;
        CurrentWaterLevel = await waterSnapshotTask;
        RecentCatches = await recentTask ?? [];
        CurrentSeasonSummary = await seasonSummaryTask;
        Records = await recordsTask;

        var seasonConfig = (await seasonConfigTask)?.ToList() ?? [];
        AvailableGroups = seasonConfig;
        SeasonDay = BuildSeasonDay(seasonConfig, DateTime.UtcNow.Date);

        var selectedGroup = ResolveSelectedGroup(seasonConfig, GroupNumber, SeasonDay.GroupNumber);
        SelectedGroup = selectedGroup;
        var (leaderboardYear, leaderboardGroup) = ResolveLeaderboardScope(selectedGroup);

        Leaderboard = await SafeCallAsync(
                          () => _catches.GetLeaderboardAsync(leaderboardYear, leaderboardGroup),
                          "load leaderboard")
                      ?? [];

        var groupSummaryTasks = seasonConfig
            .Select(g => SafeCallAsync(
                () => _catches.GetGroupSummaryAsync(CurrentYear, g.GroupNumber),
                $"load group {g.GroupNumber} summary"))
            .ToList();

        await Task.WhenAll(groupSummaryTasks);

        GroupSummaries = (await Task.WhenAll(groupSummaryTasks))
            .Where(g => g is not null)
            .Select(g => g!)
            .OrderBy(g => g.GroupNumber)
            .ToList();

        var waterReadings = await waterReadingsTask ?? [];
        WaterLevelChartJson = JsonSerializer.Serialize(
            waterReadings.Select(r => new
            {
                t = r.Time,
                v = r.LevelMeters
            }));

        var now = DateTime.UtcNow;
        CatchLocationsCurrentSeasonJson = JsonSerializer.Serialize((await seasonLocationsTask ?? [])
            .Select(c => new
            {
                lat = c.Latitude,
                lng = c.Longitude,
                w = c.WeightKg,
                angler = c.AnglerName,
                type = c.CatchType,
                location = c.Location,
                bait = c.Bait,
                date = c.CatchDate,
                daysAgo = Math.Max(0, (now.Date - c.CatchDate.Date).Days)
            }));

        CatchLocationsAllTimeJson = JsonSerializer.Serialize((await allTimeLocationsTask ?? [])
            .Select(c => new
            {
                lat = c.Latitude,
                lng = c.Longitude,
                w = c.WeightKg,
                angler = c.AnglerName,
                type = c.CatchType,
                location = c.Location,
                bait = c.Bait,
                date = c.CatchDate,
                daysAgo = Math.Max(0, (now.Date - c.CatchDate.Date).Days)
            }));
    }

    private (int year, int? group) ResolveLeaderboardScope(int? selectedGroup)
    {
        LeaderboardScope = LeaderboardScope?.ToLowerInvariant() switch
        {
            "my-group" => "my-group",
            "all-groups" => "all-groups",
            "last-year" => "last-year",
            _ => "my-group"
        };

        return LeaderboardScope switch
        {
            "all-groups" => (CurrentYear, null),
            "last-year" => (CurrentYear - 1, null),
            _ => (CurrentYear, selectedGroup)
        };
    }

    private static int? ResolveSelectedGroup(IEnumerable<SeasonConfig> configs, int? requestedGroup, int? currentGroup)
    {
        var groups = configs.Select(c => c.GroupNumber).Distinct().OrderBy(x => x).ToList();
        if (groups.Count == 0)
        {
            return null;
        }

        if (requestedGroup.HasValue && groups.Contains(requestedGroup.Value))
        {
            return requestedGroup;
        }

        if (currentGroup.HasValue && groups.Contains(currentGroup.Value))
        {
            return currentGroup;
        }

        return groups[0];
    }

    internal static SeasonDay BuildSeasonDay(IEnumerable<SeasonConfig> configs, DateTime currentDate)
    {
        var sorted = configs.OrderBy(c => c.StartDate).ToList();
        if (sorted.Count == 0)
        {
            return new SeasonDay { IsOffSeason = true };
        }

        var totalGroups = sorted.Select(c => c.GroupNumber).Distinct().Count();
        var active = sorted.FirstOrDefault(c => currentDate >= c.StartDate.Date && currentDate <= c.EndDate.Date);
        if (active is not null)
        {
            return new SeasonDay
            {
                IsOffSeason = false,
                GroupNumber = active.GroupNumber,
                GroupLengthDays = Math.Max(1, (active.EndDate.Date - active.StartDate.Date).Days + 1),
                DayInGroup = Math.Max(1, (currentDate - active.StartDate.Date).Days + 1),
                TotalGroups = totalGroups
            };
        }

        var next = sorted.FirstOrDefault(c => c.StartDate.Date > currentDate);
        var last = sorted.Last();

        // Buffer day: current date is between groups within the season span
        var isBufferDay = currentDate > sorted.First().StartDate.Date && currentDate < last.EndDate.Date;

        return new SeasonDay
        {
            IsOffSeason = !isBufferDay,
            IsBufferDay = isBufferDay,
            NextGroupStart = next?.StartDate,
            TotalGroups = totalGroups
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
            _logger.LogWarning(ex, "Dashboard section failed: {Operation}", operation);
            return default;
        }
    }
}
