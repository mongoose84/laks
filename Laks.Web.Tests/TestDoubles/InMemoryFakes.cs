using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Laks.Web.Services;

namespace Laks.Web.Tests.TestDoubles;

/// <summary>
/// Deterministic in-memory fakes shared by the integration tests.
/// Weights and water levels are chosen with decimals so Danish
/// number formatting (comma separator) is observable in rendered HTML.
/// </summary>
public sealed class InMemoryCatchRepository : ICatchRepository
{
    public const int CurrentYear = 2026;

    private static readonly List<Catch> _catches =
    [
        new Catch
        {
            Id = 1, AnglerId = 1, SeasonYear = CurrentYear,
            CatchDate = new DateTime(CurrentYear, 6, 26), CatchTime = new TimeSpan(9, 15, 0),
            WeightKg = 8.4m, Location = "Holmfoss Øvre", Weather = "Overskyet",
            WaterLevel = 1.234m, Bait = "Spinner", Latitude = 59.186959, Longitude = 9.993806,
            CatchType = "Laks", TeamName = "Hold Rød", AnglerName = "Erik Andersen"
        },
        new Catch
        {
            Id = 2, AnglerId = 2, SeasonYear = CurrentYear,
            CatchDate = new DateTime(CurrentYear, 6, 25), CatchTime = new TimeSpan(20, 30, 0),
            WeightKg = 6.1m, Location = "Holmfoss Nedre", Weather = "Regn",
            WaterLevel = 1.512m, Bait = "Flue", Latitude = 59.1871, Longitude = 9.9921,
            CatchType = "Laks", TeamName = "Hold Blå", AnglerName = "Lars Johansen"
        }
    ];

    public Task<IEnumerable<Catch>> GetRecentAsync(int count = 20) =>
        Task.FromResult<IEnumerable<Catch>>(_catches.Take(count).ToList());

    public Task<IEnumerable<Catch>> GetByYearAsync(int year) =>
        Task.FromResult<IEnumerable<Catch>>(_catches.Where(c => c.SeasonYear == year).ToList());

    public Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId) =>
        Task.FromResult<IEnumerable<Catch>>(_catches.Where(c => c.AnglerId == anglerId).ToList());

    public Task<int> GetTotalCountAsync() => Task.FromResult(_catches.Count);

    public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int year, int? groupNumber = null) =>
        Task.FromResult<IEnumerable<LeaderboardEntry>>(
        [
            new LeaderboardEntry { Rank = 1, AnglerId = 1, AnglerName = "Erik Andersen", FishCount = 3, TotalWeightKg = 18.2m, BestWeightKg = 8.4m },
            new LeaderboardEntry { Rank = 2, AnglerId = 2, AnglerName = "Lars Johansen", FishCount = 2, TotalWeightKg = 12.0m, BestWeightKg = 6.1m }
        ]);

    public Task<GroupSummary?> GetGroupSummaryAsync(int year, int groupNumber) =>
        Task.FromResult<GroupSummary?>(new GroupSummary
        {
            Year = year,
            GroupNumber = groupNumber,
            StartDate = new DateTime(year, 6, 21),
            EndDate = new DateTime(year, 6, 25),
            FishCount = 12,
            TotalWeightKg = 76.5m,
            BestWeightKg = 8.4m
        });

    public Task<SeasonSummary?> GetSeasonSummaryAsync(int year) =>
        Task.FromResult<SeasonSummary?>(new SeasonSummary
        {
            TotalFish = 47,
            TotalWeightKg = 312.4m,
            AvgWeightKg = 6.6m,
            BiggestFishKg = 12.1m,
            BiggestFishAngler = "Bjørn",
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
                CatchId = 1, Latitude = 59.186959, Longitude = 9.993806, WeightKg = 8.4m,
                AnglerName = "Erik Andersen", CatchType = "Laks", Location = "Holmfoss Øvre",
                Bait = "Spinner", CatchDate = new DateTime(CurrentYear, 6, 26), SeasonYear = CurrentYear
            }
        ]);

    public Task<IEnumerable<CatchesPerYear>> GetCatchesPerYearAsync() =>
        Task.FromResult<IEnumerable<CatchesPerYear>>(
        [
            new CatchesPerYear { Year = CurrentYear - 2, TotalCatches = 18, TotalWeightKg = 96.3m, AvgWeightKg = 5.4m },
            new CatchesPerYear { Year = CurrentYear - 1, TotalCatches = 31, TotalWeightKg = 188.6m, AvgWeightKg = 6.1m }
        ]);

    public Task<IEnumerable<CatchesPerAngler>> GetCatchesPerAnglerAsync(int? year = null) =>
        Task.FromResult<IEnumerable<CatchesPerAngler>>(
        [
            new CatchesPerAngler { AnglerName = "Erik Andersen", TotalCatches = 3, TotalWeightKg = 18.2m, BestCatchKg = 8.4m },
            new CatchesPerAngler { AnglerName = "Lars Johansen", TotalCatches = 2, TotalWeightKg = 12.0m, BestCatchKg = 6.1m }
        ]);

    public Task<IEnumerable<CatchesByType>> GetCatchesByTypeAsync(int? year = null) =>
        Task.FromResult<IEnumerable<CatchesByType>>(
        [
            new CatchesByType { TypeName = "Laks", TotalCatches = 40, Percentage = 85.1m },
            new CatchesByType { TypeName = "Ørred", TotalCatches = 7, Percentage = 14.9m }
        ]);

    public Task<IEnumerable<BiggestSalmonPerTeam>> GetBiggestSalmonPerTeamAsync(int? year = null) =>
        Task.FromResult<IEnumerable<BiggestSalmonPerTeam>>(
        [
            new BiggestSalmonPerTeam { TeamName = "Hold Rød", BiggestSalmonKg = 9.5m, AnglerName = "Erik Andersen", TotalSalmonCount = 14, AvgSalmonWeightKg = 6.3m },
            new BiggestSalmonPerTeam { TeamName = "Hold Blå", BiggestSalmonKg = 7.8m, AnglerName = "Lars Johansen", TotalSalmonCount = 11, AvgSalmonWeightKg = 5.7m }
        ]);

    public Task<IEnumerable<CatchesPerWeek>> GetCatchesPerWeekAsync() =>
        Task.FromResult<IEnumerable<CatchesPerWeek>>(
        [
            new CatchesPerWeek { SeasonYear = CurrentYear - 1, WeekNumber = 25, TotalCatches = 9 },
            new CatchesPerWeek { SeasonYear = CurrentYear - 1, WeekNumber = 26, TotalCatches = 14 },
            new CatchesPerWeek { SeasonYear = CurrentYear, WeekNumber = 26, TotalCatches = 5 }
        ]);

    public Task<IEnumerable<CatchesByHour>> GetCatchesByHourAsync(int? year = null) =>
        Task.FromResult<IEnumerable<CatchesByHour>>(
        [
            new CatchesByHour { Hour = 6, TotalCatches = 8 },
            new CatchesByHour { Hour = 9, TotalCatches = 12 },
            new CatchesByHour { Hour = 21, TotalCatches = 6 }
        ]);

    public Task<IEnumerable<CatchesByWaterLevel>> GetCatchesByWaterLevelAsync(int? year = null) =>
        Task.FromResult<IEnumerable<CatchesByWaterLevel>>(
        [
            new CatchesByWaterLevel { BandStartM = 1.25m, TotalCatches = 11 },
            new CatchesByWaterLevel { BandStartM = 1.50m, TotalCatches = 19 }
        ]);
}

public sealed class InMemorySeasonRepository : ISeasonRepository
{
    public const int CurrentYear = InMemoryCatchRepository.CurrentYear;

    public Task<IEnumerable<FishingSeason>> GetAllAsync() =>
        Task.FromResult<IEnumerable<FishingSeason>>(
        [
            new FishingSeason { Year = CurrentYear - 2, TotalCatches = 18, ParticipantCount = 30 },
            new FishingSeason { Year = CurrentYear - 1, TotalCatches = 31, ParticipantCount = 34 },
            new FishingSeason { Year = CurrentYear, TotalCatches = 2, ParticipantCount = 36 }
        ]);

    public Task<FishingSeason?> GetByYearAsync(int year) =>
        Task.FromResult<FishingSeason?>(new FishingSeason { Year = year, TotalCatches = 2, ParticipantCount = 36 });

    public Task<FishingSeason?> GetLatestAsync() =>
        Task.FromResult<FishingSeason?>(new FishingSeason { Year = CurrentYear, TotalCatches = 2, ParticipantCount = 36 });

    public Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year) =>
        Task.FromResult<IEnumerable<SeasonConfig>>(
        [
            new SeasonConfig { Year = year, GroupNumber = 1, StartDate = new DateTime(year, 6, 21), EndDate = new DateTime(year, 6, 25) },
            new SeasonConfig { Year = year, GroupNumber = 2, StartDate = new DateTime(year, 6, 26), EndDate = new DateTime(year, 6, 30) },
            new SeasonConfig { Year = year, GroupNumber = 3, StartDate = new DateTime(year, 7, 1), EndDate = new DateTime(year, 7, 5) }
        ]);

    public Task<int?> GetAnglerGroupAsync(int year, int anglerId) => Task.FromResult<int?>(1);
}

public sealed class StubWeatherService : IWeatherService
{
    public Task<WeatherData?> GetCurrentAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<WeatherData?>(new WeatherData
        {
            AirTemperatureC = 13.5m,
            PrecipitationMm = 0.4m,
            WindSpeedMs = 4.2m,
            WindDirection = "NV",
            WeatherSymbol = "clearsky_day",
            MeasuredAt = new DateTime(InMemoryCatchRepository.CurrentYear, 6, 26, 8, 0, 0, DateTimeKind.Utc)
        });
}

public sealed class StubWaterLevelService : IWaterLevelService
{
    public Task<WaterLevelSnapshot?> GetCurrentAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<WaterLevelSnapshot?>(new WaterLevelSnapshot
        {
            LevelMeters = 1.82m,
            WaterTemperatureC = 10.4m,
            Trend = WaterLevelTrend.Rising,
            MeasuredAt = new DateTime(InMemoryCatchRepository.CurrentYear, 6, 26, 8, 0, 0, DateTimeKind.Utc),
            LastKnownAt = new DateTime(InMemoryCatchRepository.CurrentYear, 6, 26, 8, 0, 0, DateTimeKind.Utc)
        });

    public Task<IReadOnlyList<WaterLevelReading>> GetLast24HoursAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<WaterLevelReading>>(
        [
            new WaterLevelReading { Time = new DateTime(InMemoryCatchRepository.CurrentYear, 6, 25, 8, 0, 0, DateTimeKind.Utc), LevelMeters = 1.70m },
            new WaterLevelReading { Time = new DateTime(InMemoryCatchRepository.CurrentYear, 6, 26, 8, 0, 0, DateTimeKind.Utc), LevelMeters = 1.82m }
        ]);
}
