using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Laks.Web.Pages.Statistics;
using Laks.Web.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laks.Web.Tests.Unit;

public class LeaderboardPageModelTests
{
    [Fact]
    public async Task OnGetAsync_MyGroup_ReturnsFullLeaderboard()
    {
        var model = CreateModel(scope: "my-group", groupNumber: 1);

        await model.OnGetAsync(CancellationToken.None);

        Assert.NotEmpty(model.Leaderboard);
        Assert.All(model.Leaderboard, e => Assert.True(e.Rank > 0));
    }

    [Fact]
    public async Task OnGetAsync_AllGroups_ReturnsFullLeaderboard()
    {
        var model = CreateModel(scope: "all-groups");

        await model.OnGetAsync(CancellationToken.None);

        Assert.NotEmpty(model.Leaderboard);
        Assert.Equal("all-groups", model.LeaderboardScope);
    }

    [Fact]
    public async Task OnGetAsync_LastYear_ReturnsFullLeaderboard()
    {
        var model = CreateModel(scope: "last-year");

        await model.OnGetAsync(CancellationToken.None);

        Assert.NotEmpty(model.Leaderboard);
        Assert.Equal("last-year", model.LeaderboardScope);
    }

    [Fact]
    public async Task OnGetAsync_LastYear_QueriesYearShownOnPillLabel()
    {
        // The "last year" pill displays LastSeasonLabelYear; the table must be
        // loaded for that same year — also when the previous season had no catches.
        var catches = new RecordingCatchRepository();
        var model = new LeaderboardModel(
            catches,
            new GapYearSeasonRepository(),
            NullLogger<LeaderboardModel>.Instance,
            new FakeTimeProvider(new DateTimeOffset(2026, 6, 26, 12, 0, 0, TimeSpan.Zero)))
        {
            LeaderboardScope = "last-year"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(2024, model.LastSeasonLabelYear);
        Assert.Equal(new[] { model.LastSeasonLabelYear }, catches.RequestedYears);
    }

    [Fact]
    public async Task OnGetAsync_LastYear_ExplicitYearParameterWins()
    {
        // The "see full leaderboard" link from the homepage passes ?year=…
        var catches = new RecordingCatchRepository();
        var model = new LeaderboardModel(
            catches,
            new GapYearSeasonRepository(),
            NullLogger<LeaderboardModel>.Instance,
            new FakeTimeProvider(new DateTimeOffset(2026, 6, 26, 12, 0, 0, TimeSpan.Zero)))
        {
            LeaderboardScope = "last-year",
            Year = 2023
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(new[] { 2023 }, catches.RequestedYears);
    }

    [Fact]
    public async Task OnGetAsync_InvalidScope_DefaultsToMyGroup()
    {
        var model = CreateModel(scope: "nonsense");

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal("my-group", model.LeaderboardScope);
    }

    [Fact]
    public async Task OnGetAsync_ResultsOrderedByRank()
    {
        var model = CreateModel(scope: "all-groups");

        await model.OnGetAsync(CancellationToken.None);

        var ranks = model.Leaderboard.Select(e => e.Rank).ToList();
        Assert.Equal(ranks.OrderBy(r => r).ToList(), ranks);
    }

    [Fact]
    public async Task OnGetAsync_AvailableGroupsPopulated()
    {
        var model = CreateModel(scope: "my-group", groupNumber: 2);

        await model.OnGetAsync(CancellationToken.None);

        Assert.NotEmpty(model.AvailableGroups);
    }

    [Fact]
    public async Task OnGetAsync_ValidGroupNumber_SetsSelectedGroup()
    {
        var model = CreateModel(scope: "my-group", groupNumber: 2);

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(2, model.SelectedGroup);
    }

    [Fact]
    public async Task OnGetAsync_InvalidGroupNumber_FallsBackToFirstGroup()
    {
        var model = CreateModel(scope: "my-group", groupNumber: 99);

        await model.OnGetAsync(CancellationToken.None);

        Assert.NotNull(model.SelectedGroup);
        Assert.Equal(1, model.SelectedGroup);
    }

    private static LeaderboardModel CreateModel(string scope = "my-group", int? groupNumber = null) =>
        new LeaderboardModel(
            new FakeLeaderboardCatchRepository(),
            new FakeLeaderboardSeasonRepository(),
            NullLogger<LeaderboardModel>.Instance)
        {
            LeaderboardScope = scope,
            GroupNumber = groupNumber
        };

    /// <summary>Season table where 2025 exists but has no catches; 2024 is the last season with data.</summary>
    private sealed class GapYearSeasonRepository : ISeasonRepository
    {
        public Task<IEnumerable<FishingSeason>> GetAllAsync() =>
            Task.FromResult<IEnumerable<FishingSeason>>(
            [
                new FishingSeason { Year = 2023, TotalCatches = 12 },
                new FishingSeason { Year = 2024, TotalCatches = 18 },
                new FishingSeason { Year = 2025, TotalCatches = 0 },
                new FishingSeason { Year = 2026, TotalCatches = 0 }
            ]);

        public Task<FishingSeason?> GetByYearAsync(int year) =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = year });

        public Task<FishingSeason?> GetLatestAsync() =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = 2026 });

        public Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year) =>
            Task.FromResult<IEnumerable<SeasonConfig>>(
            [
                new SeasonConfig { Year = year, GroupNumber = 1, StartDate = new DateTime(year, 6, 21), EndDate = new DateTime(year, 6, 25) }
            ]);

        public Task<int?> GetAnglerGroupAsync(int year, int anglerId) =>
            Task.FromResult<int?>(1);
    }

    private sealed class RecordingCatchRepository : ICatchRepository
    {
        public List<int> RequestedYears { get; } = [];

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null)
        {
            RequestedYears.Add(year);
            return Task.FromResult<IEnumerable<LeaderboardEntry>>(
            [
                new LeaderboardEntry { Rank = 1, AnglerId = 1, AnglerName = "Fisker 1", FishCount = 2, TotalWeightKg = 11m, BestWeightKg = 7m }
            ]);
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

    private sealed class FakeLeaderboardSeasonRepository : ISeasonRepository
    {
        private const int Year = 2026;

        public Task<IEnumerable<FishingSeason>> GetAllAsync() =>
            Task.FromResult<IEnumerable<FishingSeason>>([new FishingSeason { Year = Year }]);

        public Task<FishingSeason?> GetByYearAsync(int year) =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = year });

        public Task<FishingSeason?> GetLatestAsync() =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = Year });

        public Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year) =>
            Task.FromResult<IEnumerable<SeasonConfig>>(
            [
                new SeasonConfig { Year = year, GroupNumber = 1, StartDate = new DateTime(year, 6, 21), EndDate = new DateTime(year, 6, 25) },
                new SeasonConfig { Year = year, GroupNumber = 2, StartDate = new DateTime(year, 6, 26), EndDate = new DateTime(year, 6, 30) },
                new SeasonConfig { Year = year, GroupNumber = 3, StartDate = new DateTime(year, 7, 1), EndDate = new DateTime(year, 7, 5) }
            ]);

        public Task<int?> GetAnglerGroupAsync(int year, int anglerId) =>
            Task.FromResult<int?>(1);
    }

    private sealed class FakeLeaderboardCatchRepository : ICatchRepository
    {
        private static readonly IEnumerable<LeaderboardEntry> _entries =
            Enumerable.Range(1, 8).Select(i => new LeaderboardEntry
            {
                Rank = i,
                AnglerId = i,
                AnglerName = $"Fisker {i}",
                FishCount = 8 - i + 1,
                TotalWeightKg = (8 - i + 1) * 5m,
                BestWeightKg = 9m
            });

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null) =>
            Task.FromResult(_entries);

        public Task<IEnumerable<Catch>> GetRecentAsync(int count = 20) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByYearAsync(int year) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<int> GetTotalCountAsync() => Task.FromResult(8);
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
}
