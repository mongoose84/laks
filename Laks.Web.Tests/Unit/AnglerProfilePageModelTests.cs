using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Laks.Web.Pages.Anglers;
using Laks.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laks.Web.Tests.Unit;

public class AnglerProfilePageModelTests
{
    private const int CurrentYear = 2026;

    private static IndexModel BuildModel(
        IAnglerRepository? anglers = null,
        ICatchRepository? catches = null,
        ISeasonRepository? seasons = null,
        TimeProvider? timeProvider = null)
    {
        return new IndexModel(
            anglers ?? new FakeAnglerRepository(),
            catches ?? new FakeAnglerCatchRepository(),
            seasons ?? new FakeAnglerSeasonRepository(),
            NullLogger<IndexModel>.Instance,
            timeProvider ?? new FakeTimeProvider(new DateTimeOffset(CurrentYear, 6, 26, 12, 0, 0, TimeSpan.Zero)));
    }

    // ── Unknown id → 404 ─────────────────────────────────────────────────────

    [Fact]
    public async Task OnGetAsync_UnknownId_ReturnsNotFound()
    {
        var model = BuildModel();
        model.Id = 999;  // not in FakeAnglerRepository

        var result = await model.OnGetAsync(CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── Known angler → PageResult + populated profile ─────────────────────────

    [Fact]
    public async Task OnGetAsync_KnownAngler_ReturnsPageResult()
    {
        var model = BuildModel();
        model.Id = 1;

        var result = await model.OnGetAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnGetAsync_KnownAngler_ProfileIsPopulated()
    {
        var model = BuildModel();
        model.Id = 1;

        await model.OnGetAsync(CancellationToken.None);

        Assert.NotNull(model.Profile);
        Assert.Equal("Erik Andersen", model.Profile.Angler.Name);
    }

    [Fact]
    public async Task OnGetAsync_KnownAngler_RecentCatchesCappedAtTen()
    {
        var model = BuildModel(catches: new ManyAnglerCatchRepository());
        model.Id = 1;

        await model.OnGetAsync(CancellationToken.None);

        Assert.True(model.Profile.RecentCatches.Count <= 10);
    }

    // ── Zero catches → empty state ────────────────────────────────────────────

    [Fact]
    public async Task OnGetAsync_ZeroCatches_HasAnyCatchesFalse()
    {
        var model = BuildModel(catches: new NoCatchRepository());
        model.Id = 1;

        var result = await model.OnGetAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.False(model.Profile.HasAnyCatches);
    }

    [Fact]
    public async Task OnGetAsync_ZeroCatches_CurrentSeasonIsNull()
    {
        var model = BuildModel(catches: new NoCatchRepository(), seasons: new NoGroupSeasonRepository());
        model.Id = 1;

        await model.OnGetAsync(CancellationToken.None);

        Assert.Null(model.Profile.CurrentSeason);
    }

    // ── Current-season catches → strip populated with rank ───────────────────

    [Fact]
    public async Task OnGetAsync_CurrentSeasonCatches_StripPopulated()
    {
        var model = BuildModel();
        model.Id = 1;

        await model.OnGetAsync(CancellationToken.None);

        // FakeAnglerCatchRepository has current-year catches for angler 1
        Assert.NotNull(model.Profile.CurrentSeason);
        Assert.True(model.Profile.CurrentSeason!.FishCount > 0);
    }

    [Fact]
    public async Task OnGetAsync_CurrentSeasonCatches_RankComputedFromLeaderboard()
    {
        var model = BuildModel();
        model.Id = 1;

        await model.OnGetAsync(CancellationToken.None);

        // FakeAnglerSeasonRepository leaderboard has angler 1 at rank 1
        Assert.NotNull(model.Profile.CurrentSeason);
        Assert.Equal(1, model.Profile.CurrentSeason!.Rank);
    }

    // ── Group-registered, no catches → strip shows group ─────────────────────

    [Fact]
    public async Task OnGetAsync_GroupRegisteredNoCatches_StripShownWithGroupNumber()
    {
        var model = BuildModel(catches: new NoCatchRepository(), seasons: new GroupRegisteredSeasonRepository(groupNumber: 3));
        model.Id = 1;

        await model.OnGetAsync(CancellationToken.None);

        Assert.NotNull(model.Profile.CurrentSeason);
        Assert.Equal(0, model.Profile.CurrentSeason!.FishCount);
        Assert.Equal(3, model.Profile.CurrentSeason.GroupNumber);
    }

    // ── Neither catches nor group → strip hidden ──────────────────────────────

    [Fact]
    public async Task OnGetAsync_NeitherCatchesNorGroup_CurrentSeasonNull()
    {
        var model = BuildModel(catches: new NoCatchRepository(), seasons: new NoGroupSeasonRepository());
        model.Id = 1;

        await model.OnGetAsync(CancellationToken.None);

        Assert.Null(model.Profile.CurrentSeason);
    }

    // ── Season history newest-first ───────────────────────────────────────────

    [Fact]
    public async Task OnGetAsync_SeasonHistory_NewestFirst()
    {
        var model = BuildModel();
        model.Id = 1;

        await model.OnGetAsync(CancellationToken.None);

        var years = model.Profile.SeasonHistory.Select(r => r.Year).ToList();
        for (var i = 0; i < years.Count - 1; i++)
        {
            Assert.True(years[i] >= years[i + 1], $"Year {years[i]} should be >= {years[i + 1]}");
        }
    }

    // ── One year's leaderboard throws → that row Rank null, page still loads ──

    [Fact]
    public async Task OnGetAsync_LeaderboardForYearThrows_RowRankNullPageLoads()
    {
        var model = BuildModel(catches: new ThrowingLeaderboardCatchRepository());
        model.Id = 1;

        var result = await model.OnGetAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        // The season history row for the year whose leaderboard threw should have null rank
        var rowWithNullRank = model.Profile.SeasonHistory.FirstOrDefault(r => r.Rank == null);
        Assert.NotNull(rowWithNullRank);
    }

    // ──────────────── Test doubles ─────────────────────────────────────────────

    private sealed class FakeAnglerCatchRepository : ICatchRepository
    {
        private static readonly List<Catch> _catches =
        [
            new() { Id = 1, AnglerId = 1, SeasonYear = CurrentYear,
                    CatchDate = new DateTime(CurrentYear, 6, 26), CatchTime = new TimeSpan(7, 0, 0),
                    WeightKg = 9.5m, Location = "Holmfoss Øvre", Bait = "Flue", CatchType = "Laks", AnglerName = "Erik Andersen" },
            new() { Id = 2, AnglerId = 1, SeasonYear = CurrentYear,
                    CatchDate = new DateTime(CurrentYear, 6, 25), CatchTime = new TimeSpan(14, 0, 0),
                    WeightKg = 6.0m, Location = "Holmfoss Nedre", Bait = "Spinner", CatchType = "Laks", AnglerName = "Erik Andersen" },
            new() { Id = 3, AnglerId = 1, SeasonYear = CurrentYear - 1,
                    CatchDate = new DateTime(CurrentYear - 1, 6, 28), CatchTime = new TimeSpan(19, 0, 0),
                    WeightKg = 8.2m, Location = "Holmfoss Øvre", Bait = "Flue", CatchType = "Laks", AnglerName = "Erik Andersen" }
        ];

        public Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId) =>
            Task.FromResult<IEnumerable<Catch>>(anglerId == 1 ? _catches : []);

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null) =>
            Task.FromResult<IEnumerable<LeaderboardEntry>>(
            [
                new() { AnglerId = 1, Rank = 1, AnglerName = "Erik Andersen", FishCount = 2, TotalWeightKg = 15.5m, BestWeightKg = 9.5m },
                new() { AnglerId = 2, Rank = 2, AnglerName = "Lars Johansen", FishCount = 1, TotalWeightKg = 5.0m, BestWeightKg = 5.0m }
            ]);

        public Task<IEnumerable<Catch>> GetRecentAsync(int count = 20) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByYearAsync(int year) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<int> GetTotalCountAsync() => Task.FromResult(3);
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

    private sealed class NoCatchRepository : ICatchRepository
    {
        public Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null) => Task.FromResult<IEnumerable<LeaderboardEntry>>([]);
        public Task<IEnumerable<Catch>> GetRecentAsync(int count = 20) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByYearAsync(int year) => Task.FromResult<IEnumerable<Catch>>([]);
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

    /// <summary>Returns 15 catches for angler 1 so the ≤10 cap can be tested.</summary>
    private sealed class ManyAnglerCatchRepository : ICatchRepository
    {
        private static readonly List<Catch> _manyCatches =
            Enumerable.Range(1, 15).Select(i => new Catch
            {
                Id = i, AnglerId = 1, SeasonYear = CurrentYear,
                CatchDate = new DateTime(CurrentYear, 6, 1).AddDays(i),
                CatchTime = new TimeSpan(10, 0, 0),
                WeightKg = 5m + i * 0.1m, CatchType = "Laks",
                AnglerName = "Erik Andersen", Bait = "Flue"
            }).OrderByDescending(c => c.CatchDate).ToList();

        public Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId) =>
            Task.FromResult<IEnumerable<Catch>>(anglerId == 1 ? _manyCatches : []);

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null) =>
            Task.FromResult<IEnumerable<LeaderboardEntry>>([]);

        public Task<IEnumerable<Catch>> GetRecentAsync(int count = 20) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByYearAsync(int year) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<int> GetTotalCountAsync() => Task.FromResult(15);
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

    /// <summary>Returns catches for multiple years; leaderboard throws for the oldest year.</summary>
    private sealed class ThrowingLeaderboardCatchRepository : ICatchRepository
    {
        private static readonly List<Catch> _catches =
        [
            new() { Id = 1, AnglerId = 1, SeasonYear = CurrentYear,
                    CatchDate = new DateTime(CurrentYear, 6, 26), CatchTime = new TimeSpan(9, 0, 0),
                    WeightKg = 8.0m, CatchType = "Laks", AnglerName = "Erik Andersen", Bait = "Flue" },
            new() { Id = 2, AnglerId = 1, SeasonYear = CurrentYear - 1,
                    CatchDate = new DateTime(CurrentYear - 1, 6, 26), CatchTime = new TimeSpan(14, 0, 0),
                    WeightKg = 5.0m, CatchType = "Laks", AnglerName = "Erik Andersen", Bait = "Spinner" }
        ];

        public Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId) =>
            Task.FromResult<IEnumerable<Catch>>(anglerId == 1 ? _catches : []);

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null)
        {
            // Throw on the second leaderboard call (oldest year = CurrentYear - 1)
            if (year == CurrentYear - 1)
            {
                throw new InvalidOperationException("Simulated leaderboard failure");
            }

            return Task.FromResult<IEnumerable<LeaderboardEntry>>(
            [
                new() { AnglerId = 1, Rank = 1, AnglerName = "Erik Andersen", FishCount = 1, TotalWeightKg = 8m, BestWeightKg = 8m }
            ]);
        }

        public Task<IEnumerable<Catch>> GetRecentAsync(int count = 20) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<IEnumerable<Catch>> GetByYearAsync(int year) => Task.FromResult<IEnumerable<Catch>>([]);
        public Task<int> GetTotalCountAsync() => Task.FromResult(2);
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

    private sealed class FakeAnglerSeasonRepository : ISeasonRepository
    {
        public Task<IEnumerable<FishingSeason>> GetAllAsync() =>
            Task.FromResult<IEnumerable<FishingSeason>>([new FishingSeason { Year = CurrentYear }]);

        public Task<FishingSeason?> GetByYearAsync(int year) =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = year });

        public Task<FishingSeason?> GetLatestAsync() =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = CurrentYear });

        public Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year) =>
            Task.FromResult<IEnumerable<SeasonConfig>>(
            [
                new SeasonConfig { Year = year, GroupNumber = 1, StartDate = new DateTime(year, 6, 21), EndDate = new DateTime(year, 7, 5) }
            ]);

        public Task<int?> GetAnglerGroupAsync(int year, int anglerId) =>
            Task.FromResult<int?>(1);
    }

    private sealed class NoGroupSeasonRepository : ISeasonRepository
    {
        public Task<IEnumerable<FishingSeason>> GetAllAsync() => Task.FromResult<IEnumerable<FishingSeason>>([]);
        public Task<FishingSeason?> GetByYearAsync(int year) => Task.FromResult<FishingSeason?>(null);
        public Task<FishingSeason?> GetLatestAsync() => Task.FromResult<FishingSeason?>(null);
        public Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year) => Task.FromResult<IEnumerable<SeasonConfig>>([]);
        public Task<int?> GetAnglerGroupAsync(int year, int anglerId) => Task.FromResult<int?>(null);
    }

    private sealed class GroupRegisteredSeasonRepository(int groupNumber) : ISeasonRepository
    {
        public Task<IEnumerable<FishingSeason>> GetAllAsync() => Task.FromResult<IEnumerable<FishingSeason>>([]);
        public Task<FishingSeason?> GetByYearAsync(int year) => Task.FromResult<FishingSeason?>(null);
        public Task<FishingSeason?> GetLatestAsync() => Task.FromResult<FishingSeason?>(null);
        public Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year) => Task.FromResult<IEnumerable<SeasonConfig>>([]);
        public Task<int?> GetAnglerGroupAsync(int year, int anglerId) => Task.FromResult<int?>(groupNumber);
    }
}
