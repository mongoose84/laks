using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Laks.Web.Pages;
using Laks.Web.Services;
using Laks.Web.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laks.Web.Tests.Unit;

public class DashboardPageModelTests
{
    private const int TestCurrentYear = 2026;

    [Fact]
    public async Task OnGetAsync_LoadsDashboardSections()
    {
        var catches = new FakeCatchRepository();
        var seasons = new FakeSeasonRepository();
        var weather = new FakeWeatherService();
        var water = new FakeWaterLevelService();

        var model = new IndexModel(
            seasons,
            catches,
            weather,
            water,
            NullLogger<IndexModel>.Instance,
            new FakeTimeProvider(new DateTimeOffset(TestCurrentYear, 6, 26, 12, 0, 0, TimeSpan.Zero)))
        {
            GroupNumber = 2,
            LeaderboardScope = "my-group"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(TestCurrentYear, model.CurrentYear);
        Assert.NotNull(model.CurrentWeather);
        Assert.NotNull(model.CurrentWaterLevel);
        Assert.NotEmpty(model.Leaderboard);
        Assert.NotEmpty(model.RecentCatches);
        Assert.NotEqual("[]", model.WaterLevelChartJson);
        Assert.Contains("59.18", model.CatchLocationsCurrentSeasonJson);
    }

    [Fact]
    public async Task OnGetAsync_PopulatesEditorialLabels()
    {
        var model = new IndexModel(
            new FakeSeasonRepository(),
            new FakeCatchRepository(),
            new FakeWeatherService(),
            new FakeWaterLevelService(),
            NullLogger<IndexModel>.Instance);

        await model.OnGetAsync(CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(model.IssueDateLabel));
        Assert.StartsWith("Sidste opdatering", model.LastUpdatedLabel);
    }

    [Fact]
    public void BuildIssueDateLabel_FormatsInDanish()
    {
        var label = IndexModel.BuildIssueDateLabel(new DateTime(2026, 4, 29, 10, 0, 0, DateTimeKind.Utc));

        Assert.Contains("april", label);
        Assert.Contains("2026", label);
        Assert.Contains("29", label);
    }

    [Fact]
    public void BuildLastUpdatedLabel_NoData_ReturnsUnknown()
    {
        var label = IndexModel.BuildLastUpdatedLabel(null, null);

        Assert.Equal("Sidste opdatering · ukendt", label);
    }

    [Fact]
    public void BuildLastUpdatedLabel_PicksLatestTimestamp()
    {
        var label = IndexModel.BuildLastUpdatedLabel(
            new DateTime(2026, 6, 26, 5, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 26, 6, 42, 0, DateTimeKind.Utc));

        Assert.StartsWith("Sidste opdatering ·", label);
        Assert.Matches(@"\d{2}[:.]\d{2}$", label);
    }

    [Fact]
    public async Task OnGetAsync_HandlesServiceFailureGracefully()
    {
        var model = new IndexModel(
            new FakeSeasonRepository(),
            new FakeCatchRepository(),
            new ThrowingWeatherService(),
            new FakeWaterLevelService(),
            NullLogger<IndexModel>.Instance);

        await model.OnGetAsync(CancellationToken.None);

        Assert.Null(model.CurrentWeather);
        Assert.NotNull(model.CurrentWaterLevel);
        Assert.NotEmpty(model.RecentCatches);
    }

    [Fact]
    public void BuildSeasonDay_ActiveGroup_ReturnsDayInfo()
    {
        var configs = ThreeGroupConfig(2026);
        var result = IndexModel.BuildSeasonDay(configs, new DateTime(2026, 6, 23));

        Assert.False(result.IsOffSeason);
        Assert.False(result.IsBufferDay);
        Assert.Equal(1, result.GroupNumber);
        Assert.Equal(3, result.DayInGroup);
        Assert.Equal(5, result.GroupLengthDays);
        Assert.Equal(3, result.TotalGroups);
    }

    [Fact]
    public void BuildSeasonDay_BufferDay_ShowsCountdown()
    {
        // Groups with buffer days: G1=Jun21-25, G2=Jun27-Jul1, G3=Jul3-7
        var configs = new List<SeasonConfig>
        {
            new() { Year = 2026, GroupNumber = 1, StartDate = new DateTime(2026, 6, 21), EndDate = new DateTime(2026, 6, 25) },
            new() { Year = 2026, GroupNumber = 2, StartDate = new DateTime(2026, 6, 27), EndDate = new DateTime(2026, 7, 1) },
            new() { Year = 2026, GroupNumber = 3, StartDate = new DateTime(2026, 7, 3), EndDate = new DateTime(2026, 7, 7) }
        };

        var result = IndexModel.BuildSeasonDay(configs, new DateTime(2026, 6, 26));

        Assert.False(result.IsOffSeason);
        Assert.True(result.IsBufferDay);
        Assert.Equal(new DateTime(2026, 6, 27), result.NextGroupStart);
        Assert.Equal(3, result.TotalGroups);
    }

    [Fact]
    public void BuildSeasonDay_BeforeSeason_ShowsCountdown()
    {
        var configs = ThreeGroupConfig(2026);
        var result = IndexModel.BuildSeasonDay(configs, new DateTime(2026, 3, 15));

        Assert.True(result.IsOffSeason);
        Assert.False(result.IsBufferDay);
        Assert.Equal(new DateTime(2026, 6, 21), result.NextGroupStart);
    }

    [Fact]
    public void BuildSeasonDay_AfterSeason_ReturnsOffSeason()
    {
        var configs = ThreeGroupConfig(2026);
        var result = IndexModel.BuildSeasonDay(configs, new DateTime(2026, 8, 1));

        Assert.True(result.IsOffSeason);
        Assert.False(result.IsBufferDay);
        Assert.Null(result.NextGroupStart);
        // The season existed and is over — not "unconfigured".
        Assert.True(result.SeasonHasEnded);
    }

    [Fact]
    public void BuildSeasonDay_TwoGroups_WorksDynamically()
    {
        var configs = new List<SeasonConfig>
        {
            new() { Year = 2001, GroupNumber = 1, StartDate = new DateTime(2001, 6, 15), EndDate = new DateTime(2001, 6, 24) },
            new() { Year = 2001, GroupNumber = 2, StartDate = new DateTime(2001, 6, 25), EndDate = new DateTime(2001, 7, 4) }
        };

        var result = IndexModel.BuildSeasonDay(configs, new DateTime(2001, 6, 28));

        Assert.False(result.IsOffSeason);
        Assert.Equal(2, result.GroupNumber);
        Assert.Equal(4, result.DayInGroup);
        Assert.Equal(10, result.GroupLengthDays);
        Assert.Equal(2, result.TotalGroups);
    }

    [Fact]
    public void BuildSeasonDay_NoConfig_ReturnsOffSeason()
    {
        var result = IndexModel.BuildSeasonDay([], new DateTime(2026, 6, 25));

        Assert.True(result.IsOffSeason);
    }

    [Fact]
    public async Task OnGetAsync_LeaderboardPreview_CappedAtFive()
    {
        var model = new IndexModel(
            new FakeSeasonRepository(),
            new LargeLeaderboardCatchRepository(),
            new FakeWeatherService(),
            new FakeWaterLevelService(),
            NullLogger<IndexModel>.Instance)
        {
            LeaderboardScope = "my-group"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.True(model.LeaderboardPreview.Count() <= 5);
        Assert.Equal(5, model.LeaderboardPreview.Count());
        Assert.Equal(10, model.LeaderboardTotalCount);
    }

    [Fact]
    public async Task OnGetAsync_LeaderboardPreview_AllGroupsScope_CappedAtFive()
    {
        var model = new IndexModel(
            new FakeSeasonRepository(),
            new LargeLeaderboardCatchRepository(),
            new FakeWeatherService(),
            new FakeWaterLevelService(),
            NullLogger<IndexModel>.Instance)
        {
            LeaderboardScope = "all-groups"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.True(model.LeaderboardPreview.Count() <= 5);
        Assert.Equal(10, model.LeaderboardTotalCount);
    }

    [Fact]
    public async Task OnGetAsync_LeaderboardPreview_LastYearScope_CappedAtFive()
    {
        var model = new IndexModel(
            new FakeSeasonRepository(),
            new LargeLeaderboardCatchRepository(),
            new FakeWeatherService(),
            new FakeWaterLevelService(),
            NullLogger<IndexModel>.Instance)
        {
            LeaderboardScope = "last-year"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.True(model.LeaderboardPreview.Count() <= 5);
        Assert.Equal(10, model.LeaderboardTotalCount);
    }

    [Fact]
    public async Task OnGetAsync_LeaderboardPreview_TotalCountMatchesFullLeaderboard()
    {
        var model = new IndexModel(
            new FakeSeasonRepository(),
            new FakeCatchRepository(),
            new FakeWeatherService(),
            new FakeWaterLevelService(),
            NullLogger<IndexModel>.Instance);

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(model.Leaderboard.Count(), model.LeaderboardTotalCount);
        Assert.Equal(Math.Min(5, model.LeaderboardTotalCount), model.LeaderboardPreview.Count());
        Assert.True(model.LeaderboardPreview.Count() <= 5);
    }

    [Fact]
    public async Task OnGetAsync_LastSeasonLabelYear_ReflectsMostRecentSeasonWithCatches()
    {
        var model = new IndexModel(
            new FakeSeasonRepository(),
            new FakeCatchRepository(),
            new FakeWeatherService(),
            new FakeWaterLevelService(),
            NullLogger<IndexModel>.Instance,
            new FakeTimeProvider(new DateTimeOffset(TestCurrentYear, 6, 26, 12, 0, 0, TimeSpan.Zero)));

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(TestCurrentYear - 1, model.LastSeasonLabelYear);
    }

    [Fact]
    public async Task OnGetAsync_LastYear_SkipsEmptySeasonsAndQueriesLastSeasonWithData()
    {
        var frozenNow = new DateTimeOffset(2026, 5, 20, 12, 0, 0, TimeSpan.Zero);
        var startYear = frozenNow.Year - 1;
        var catches = new HistoricalFallbackCatchRepository(startYear);
        // Season table: 2024 has catches, 2025 and 2026 are empty,
        // so "last year" must resolve to 2024 — the last season with data.
        var model = new IndexModel(
            new GapYearSeasonRepository(frozenNow.Year),
            catches,
            new FakeWeatherService(),
            new FakeWaterLevelService(),
            NullLogger<IndexModel>.Instance,
            new FakeTimeProvider(frozenNow))
        {
            LeaderboardScope = "last-year"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(startYear - 1, model.LeaderboardYear);
        Assert.NotEmpty(model.Leaderboard);
        // Year is resolved from allSeasons.TotalCatches — only one GetLeaderboardAsync call for the resolved year.
        Assert.Equal(new[] { startYear - 1 }, catches.RequestedYears);
    }

    [Fact]
    public async Task OnGetAsync_LastYear_PillLabelMatchesQueriedYear()
    {
        // The "last year" pill displays LastSeasonLabelYear, so the leaderboard
        // it links to must be loaded for that same year — under any season state.
        var frozenNow = new DateTimeOffset(2026, 5, 20, 12, 0, 0, TimeSpan.Zero);
        var model = new IndexModel(
            new FakeSeasonRepository(frozenNow.Year),
            new FakeCatchRepository(),
            new FakeWeatherService(),
            new FakeWaterLevelService(),
            NullLogger<IndexModel>.Instance,
            new FakeTimeProvider(frozenNow))
        {
            LeaderboardScope = "last-year"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(model.LastSeasonLabelYear, model.LeaderboardYear);
    }

    [Fact]
    public async Task OnGetAsync_LastYear_ActiveSeasonQueriesPreviousYear()
    {
        var frozenNow = new DateTimeOffset(2026, 5, 15, 12, 0, 0, TimeSpan.Zero);
        var startYear = frozenNow.Year - 1;
        var catches = new HistoricalFallbackCatchRepository(startYear);
        // Freeze time inside the active season (May 1–30); only the current year
        // exists in the season table, so "last year" defaults to currentYear - 1.
        var model = new IndexModel(
            new ActiveSeasonRepository(frozenNow.Year),
            catches,
            new FakeWeatherService(),
            new FakeWaterLevelService(),
            NullLogger<IndexModel>.Instance,
            new FakeTimeProvider(frozenNow))
        {
            LeaderboardScope = "last-year"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(startYear, model.LeaderboardYear);
        Assert.Equal(new[] { startYear }, catches.RequestedYears);
    }

    private sealed class LargeLeaderboardCatchRepository : ICatchRepository
    {
        private static readonly IEnumerable<LeaderboardEntry> _leaderboard =
            Enumerable.Range(1, 10).Select(i => new LeaderboardEntry
            {
                Rank = i,
                AnglerId = i,
                AnglerName = $"Fisker {i}",
                FishCount = 10 - i + 1,
                TotalWeightKg = (10 - i + 1) * 6m,
                BestWeightKg = 8m
            });

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null) =>
            Task.FromResult(_leaderboard);

        public Task<IEnumerable<Catch>> GetRecentAsync(int count = 20) =>
            Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByYearAsync(int year) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<int> GetTotalCountAsync() => Task.FromResult(10);
        public Task<GroupSummary?> GetGroupSummaryAsync(int year, int groupNumber) =>
            Task.FromResult<GroupSummary?>(null);
        public Task<SeasonSummary?> GetSeasonSummaryAsync(int year) =>
            Task.FromResult<SeasonSummary?>(null);
        public Task<AllTimeRecords?> GetAllTimeRecordsAsync() =>
            Task.FromResult<AllTimeRecords?>(null);
        public Task<IEnumerable<CatchLocation>> GetCatchLocationsAsync(int? year = null) =>
            Task.FromResult<IEnumerable<CatchLocation>>([]);
        public Task<IEnumerable<CatchesPerYear>> GetCatchesPerYearAsync() => Task.FromResult<IEnumerable<CatchesPerYear>>([]);
        public Task<IEnumerable<CatchesPerAngler>> GetCatchesPerAnglerAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesPerAngler>>([]);
        public Task<IEnumerable<CatchesByType>> GetCatchesByTypeAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesByType>>([]);
        public Task<IEnumerable<BiggestSalmonPerTeam>> GetBiggestSalmonPerTeamAsync(int? year = null) => Task.FromResult<IEnumerable<BiggestSalmonPerTeam>>([]);
        public Task<IEnumerable<CatchesPerWeek>> GetCatchesPerWeekAsync() => Task.FromResult<IEnumerable<CatchesPerWeek>>([]);
        public Task<IEnumerable<CatchesByHour>> GetCatchesByHourAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesByHour>>([]);
        public Task<IEnumerable<CatchesByWaterLevel>> GetCatchesByWaterLevelAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesByWaterLevel>>([]);
        public Task<IEnumerable<SpotStats>> GetCatchStatsPerSpotAsync() => Task.FromResult<IEnumerable<SpotStats>>([]);
    }

    private sealed class HistoricalFallbackCatchRepository(int startYear) : ICatchRepository
    {
        private static readonly IEnumerable<LeaderboardEntry> Empty = [];
        private static readonly IEnumerable<LeaderboardEntry> Data =
            Enumerable.Range(1, 3).Select(i => new LeaderboardEntry
            {
                Rank = i,
                AnglerId = i,
                AnglerName = $"Fisker {i}",
                FishCount = 4 - i,
                TotalWeightKg = (4 - i) * 7.5m,
                BestWeightKg = 9m
            });

        public List<int> RequestedYears { get; } = [];

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null)
        {
            RequestedYears.Add(year);

            return Task.FromResult(year switch
            {
                var y when y == startYear => Empty,
                var y when y == startYear - 1 => Data,
                _ => Empty
            });
        }

        public Task<IEnumerable<Catch>> GetRecentAsync(int count = 20) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByYearAsync(int year) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<int> GetTotalCountAsync() => Task.FromResult(0);
        public Task<GroupSummary?> GetGroupSummaryAsync(int year, int groupNumber) => Task.FromResult<GroupSummary?>(null);
        public Task<SeasonSummary?> GetSeasonSummaryAsync(int year) => Task.FromResult<SeasonSummary?>(null);
        public Task<AllTimeRecords?> GetAllTimeRecordsAsync() => Task.FromResult<AllTimeRecords?>(null);
        public Task<IEnumerable<CatchLocation>> GetCatchLocationsAsync(int? year = null) => Task.FromResult<IEnumerable<CatchLocation>>([]);
        public Task<IEnumerable<CatchesPerYear>> GetCatchesPerYearAsync() => Task.FromResult<IEnumerable<CatchesPerYear>>([]);
        public Task<IEnumerable<CatchesPerAngler>> GetCatchesPerAnglerAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesPerAngler>>([]);
        public Task<IEnumerable<CatchesByType>> GetCatchesByTypeAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesByType>>([]);
        public Task<IEnumerable<BiggestSalmonPerTeam>> GetBiggestSalmonPerTeamAsync(int? year = null) => Task.FromResult<IEnumerable<BiggestSalmonPerTeam>>([]);
        public Task<IEnumerable<CatchesPerWeek>> GetCatchesPerWeekAsync() => Task.FromResult<IEnumerable<CatchesPerWeek>>([]);
        public Task<IEnumerable<CatchesByHour>> GetCatchesByHourAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesByHour>>([]);
        public Task<IEnumerable<CatchesByWaterLevel>> GetCatchesByWaterLevelAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesByWaterLevel>>([]);
        public Task<IEnumerable<SpotStats>> GetCatchStatsPerSpotAsync() => Task.FromResult<IEnumerable<SpotStats>>([]);
    }

    private static List<SeasonConfig> ThreeGroupConfig(int year) =>
    [
        new() { Year = year, GroupNumber = 1, StartDate = new DateTime(year, 6, 21), EndDate = new DateTime(year, 6, 25) },
        new() { Year = year, GroupNumber = 2, StartDate = new DateTime(year, 6, 26), EndDate = new DateTime(year, 6, 30) },
        new() { Year = year, GroupNumber = 3, StartDate = new DateTime(year, 7, 1), EndDate = new DateTime(year, 7, 5) }
    ];

    private sealed class FakeSeasonRepository(int currentYear = TestCurrentYear) : ISeasonRepository
    {
        private readonly int _currentYear = currentYear;

        public Task<IEnumerable<FishingSeason>> GetAllAsync() =>
            Task.FromResult<IEnumerable<FishingSeason>>(
            [
                new FishingSeason { Year = _currentYear - 2, TotalCatches = 18 },
                new FishingSeason { Year = _currentYear - 1, TotalCatches = 31 },
                new FishingSeason { Year = _currentYear, TotalCatches = 0 }
            ]);

        public Task<FishingSeason?> GetByYearAsync(int year) =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = year, ParticipantCount = 36 });

        public Task<FishingSeason?> GetLatestAsync() =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = _currentYear, ParticipantCount = 36 });

        public Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year) =>
            Task.FromResult<IEnumerable<SeasonConfig>>(
            [
                new SeasonConfig { Year = year, GroupNumber = 1, StartDate = new DateTime(year, 6, 21), EndDate = new DateTime(year, 6, 25) },
                new SeasonConfig { Year = year, GroupNumber = 2, StartDate = new DateTime(year, 6, 26), EndDate = new DateTime(year, 6, 30) },
                new SeasonConfig { Year = year, GroupNumber = 3, StartDate = new DateTime(year, 7, 1), EndDate = new DateTime(year, 7, 5) }
            ]);

        public Task<int?> GetAnglerGroupAsync(int year, int anglerId) =>
            Task.FromResult<int?>(2);
    }

    /// <summary>Season table where the previous year exists but has no catches.</summary>
    private sealed class GapYearSeasonRepository(int currentYear) : ISeasonRepository
    {
        public Task<IEnumerable<FishingSeason>> GetAllAsync() =>
            Task.FromResult<IEnumerable<FishingSeason>>(
            [
                new FishingSeason { Year = currentYear - 2, TotalCatches = 18 },
                new FishingSeason { Year = currentYear - 1, TotalCatches = 0 },
                new FishingSeason { Year = currentYear, TotalCatches = 0 }
            ]);

        public Task<FishingSeason?> GetByYearAsync(int year) =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = year, ParticipantCount = 36 });

        public Task<FishingSeason?> GetLatestAsync() =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = currentYear, ParticipantCount = 36 });

        public Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year) =>
            Task.FromResult<IEnumerable<SeasonConfig>>(
            [
                new SeasonConfig { Year = year, GroupNumber = 1, StartDate = new DateTime(year, 6, 21), EndDate = new DateTime(year, 6, 25) },
                new SeasonConfig { Year = year, GroupNumber = 2, StartDate = new DateTime(year, 6, 26), EndDate = new DateTime(year, 6, 30) }
            ]);

        public Task<int?> GetAnglerGroupAsync(int year, int anglerId) =>
            Task.FromResult<int?>(1);
    }

    private sealed class ActiveSeasonRepository(int currentYear) : ISeasonRepository
    {
        public Task<IEnumerable<FishingSeason>> GetAllAsync() =>
            Task.FromResult<IEnumerable<FishingSeason>>([new FishingSeason { Year = currentYear }]);

        public Task<FishingSeason?> GetByYearAsync(int year) =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = year, ParticipantCount = 36 });

        public Task<FishingSeason?> GetLatestAsync() =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = currentYear, ParticipantCount = 36 });

        public Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year) =>
            Task.FromResult<IEnumerable<SeasonConfig>>(
            [
                new SeasonConfig { Year = year, GroupNumber = 1, StartDate = new DateTime(year, 5, 1), EndDate = new DateTime(year, 5, 30) }
            ]);

        public Task<int?> GetAnglerGroupAsync(int year, int anglerId) =>
            Task.FromResult<int?>(2);
    }

    private sealed class FakeCatchRepository : ICatchRepository
    {
        public Task<IEnumerable<Catch>> GetRecentAsync(int count = 20) =>
            Task.FromResult<IEnumerable<Catch>>(
            [
                new Catch { Id = 1, CatchDate = new DateTime(2026, 6, 26), CatchTime = new TimeSpan(9, 0, 0), WeightKg = 8.4m, CatchType = "Atlantic Salmon", AnglerName = "Erik Andersen", Location = "Holmfoss Ovre" },
                new Catch { Id = 2, CatchDate = new DateTime(2026, 6, 25), CatchTime = new TimeSpan(20, 0, 0), WeightKg = 6.1m, CatchType = "Atlantic Salmon", AnglerName = "Lars Johansen", Location = "Holmfoss Nedre" }
            ]);

        public Task<IEnumerable<Catch>> GetByYearAsync(int year) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<int> GetTotalCountAsync() => Task.FromResult(100);

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null) =>
            Task.FromResult<IEnumerable<LeaderboardEntry>>(
            [
                new LeaderboardEntry { Rank = 1, AnglerName = "Erik Andersen", FishCount = 3, TotalWeightKg = 18.2m, BestWeightKg = 8.4m },
                new LeaderboardEntry { Rank = 2, AnglerName = "Lars Johansen", FishCount = 2, TotalWeightKg = 12.0m, BestWeightKg = 6.1m }
            ]);

        public Task<GroupSummary?> GetGroupSummaryAsync(int year, int groupNumber) =>
            Task.FromResult<GroupSummary?>(new GroupSummary
            {
                Year = year,
                GroupNumber = groupNumber,
                StartDate = new DateTime(year, 6, 26),
                EndDate = new DateTime(year, 6, 30),
                FishCount = groupNumber == 2 ? 23 : 18,
                TotalWeightKg = groupNumber == 2 ? 156m : 112m,
                BestWeightKg = 12.1m
            });

        public Task<SeasonSummary?> GetSeasonSummaryAsync(int year) =>
            Task.FromResult<SeasonSummary?>(new SeasonSummary
            {
                TotalFish = 47,
                TotalWeightKg = 312.4m,
                AvgWeightKg = 6.6m,
                BiggestFishKg = 12.1m,
                BiggestFishAngler = "Bjorn",
                ActiveAnglers = 24,
                TotalAnglers = 36
            });

        public Task<AllTimeRecords?> GetAllTimeRecordsAsync() =>
            Task.FromResult<AllTimeRecords?>(new AllTimeRecords
            {
                BiggestFishKg = 14.3m,
                BiggestFishAngler = "Ole Kristiansen",
                BiggestFishYear = 2018,
                MostProlificAngler = "Erik Andersen",
                MostProlificFishCount = 127,
                BestSeasonYear = 2019,
                BestSeasonFishCount = 63,
                BestSeasonTotalKg = 412m
            });

        public Task<IEnumerable<CatchLocation>> GetCatchLocationsAsync(int? year = null) =>
            Task.FromResult<IEnumerable<CatchLocation>>(
            [
                new CatchLocation
                {
                    CatchId = 1,
                    Latitude = 59.186959,
                    Longitude = 9.993806,
                    WeightKg = 8.5m,
                    AnglerName = "Erik Andersen",
                    CatchType = "Atlantic Salmon",
                    Location = "Holmfoss Ovre",
                    Bait = "Spinner",
                    CatchDate = new DateTime(2026, 6, 26),
                    SeasonYear = 2026
                }
            ]);

        public Task<IEnumerable<CatchesPerYear>> GetCatchesPerYearAsync() => Task.FromResult<IEnumerable<CatchesPerYear>>([]);
        public Task<IEnumerable<CatchesPerAngler>> GetCatchesPerAnglerAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesPerAngler>>([]);
        public Task<IEnumerable<CatchesByType>> GetCatchesByTypeAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesByType>>([]);
        public Task<IEnumerable<BiggestSalmonPerTeam>> GetBiggestSalmonPerTeamAsync(int? year = null) => Task.FromResult<IEnumerable<BiggestSalmonPerTeam>>([]);
        public Task<IEnumerable<CatchesPerWeek>> GetCatchesPerWeekAsync() => Task.FromResult<IEnumerable<CatchesPerWeek>>([]);
        public Task<IEnumerable<CatchesByHour>> GetCatchesByHourAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesByHour>>([]);
        public Task<IEnumerable<CatchesByWaterLevel>> GetCatchesByWaterLevelAsync(int? year = null) => Task.FromResult<IEnumerable<CatchesByWaterLevel>>([]);
        public Task<IEnumerable<SpotStats>> GetCatchStatsPerSpotAsync() => Task.FromResult<IEnumerable<SpotStats>>([]);
    }

    private sealed class ThrowingWeatherService : IWeatherService
    {
        public Task<WeatherData?> GetCurrentAsync(CancellationToken cancellationToken = default)
            => throw new HttpRequestException("Weather API unavailable");
    }

}
