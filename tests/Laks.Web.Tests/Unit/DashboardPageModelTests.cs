using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Laks.Web.Pages;
using Laks.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laks.Web.Tests.Unit;

public class DashboardPageModelTests
{
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
            NullLogger<IndexModel>.Instance)
        {
            GroupNumber = 2,
            LeaderboardScope = "my-group"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(2026, model.CurrentYear);
        Assert.NotNull(model.CurrentWeather);
        Assert.NotNull(model.CurrentWaterLevel);
        Assert.NotEmpty(model.Leaderboard);
        Assert.NotEmpty(model.RecentCatches);
        Assert.NotEqual("[]", model.WaterLevelChartJson);
        Assert.Contains("59.18", model.CatchLocationsCurrentSeasonJson);
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

    private sealed class FakeSeasonRepository : ISeasonRepository
    {
        public Task<IEnumerable<FishingSeason>> GetAllAsync() =>
            Task.FromResult<IEnumerable<FishingSeason>>([new FishingSeason { Year = 2026 }]);

        public Task<FishingSeason?> GetByYearAsync(int year) =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = year, ParticipantCount = 36 });

        public Task<FishingSeason?> GetLatestAsync() =>
            Task.FromResult<FishingSeason?>(new FishingSeason { Year = 2026, ParticipantCount = 36 });

        public Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year) =>
            Task.FromResult<IEnumerable<SeasonConfig>>(
            [
                new SeasonConfig { Year = year, GroupNumber = 1, StartDate = new DateTime(year, 6, 21), EndDate = new DateTime(year, 6, 25) },
                new SeasonConfig { Year = year, GroupNumber = 2, StartDate = new DateTime(year, 6, 26), EndDate = new DateTime(year, 6, 30) },
                new SeasonConfig { Year = year, GroupNumber = 3, StartDate = new DateTime(year, 7, 1), EndDate = new DateTime(year, 7, 5) }
            ]);
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
    }

    private sealed class FakeWeatherService : IWeatherService
    {
        public Task<WeatherData?> GetCurrentAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<WeatherData?>(new WeatherData
            {
                AirTemperatureC = 13.5m,
                WindSpeedMs = 4.2m,
                WindDirection = "NW",
                WeatherSymbol = "clearsky_day",
                MeasuredAt = new DateTime(2026, 6, 26, 8, 0, 0, DateTimeKind.Utc)
            });
    }

    private sealed class ThrowingWeatherService : IWeatherService
    {
        public Task<WeatherData?> GetCurrentAsync(CancellationToken cancellationToken = default)
            => throw new HttpRequestException("Weather API unavailable");
    }

    private sealed class FakeWaterLevelService : IWaterLevelService
    {
        public Task<WaterLevelSnapshot?> GetCurrentAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<WaterLevelSnapshot?>(new WaterLevelSnapshot
            {
                LevelMeters = 1.82m,
                WaterTemperatureC = 10.4m,
                Trend = WaterLevelTrend.Rising,
                MeasuredAt = new DateTime(2026, 6, 26, 8, 0, 0, DateTimeKind.Utc),
                LastKnownAt = new DateTime(2026, 6, 26, 8, 0, 0, DateTimeKind.Utc)
            });

        public Task<IReadOnlyList<WaterLevelReading>> GetLast24HoursAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<WaterLevelReading>>(
            [
                new WaterLevelReading { Time = new DateTime(2026, 6, 25, 8, 0, 0, DateTimeKind.Utc), LevelMeters = 1.70m },
                new WaterLevelReading { Time = new DateTime(2026, 6, 26, 8, 0, 0, DateTimeKind.Utc), LevelMeters = 1.82m }
            ]);
    }
}
