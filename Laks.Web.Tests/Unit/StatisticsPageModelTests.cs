using Laks.Web.Models;
using Laks.Web.Pages.Statistics;
using System.Text.Json;

namespace Laks.Web.Tests.Unit;

/// <summary>
/// Unit tests for the Statistics page model – verifies that JSON
/// serialization produces valid arrays without relying on a database.
/// </summary>
public class StatisticsPageModelTests
{
    [Fact]
    public void CatchesPerYear_Serializes_To_ValidJsonArray()
    {
        var data = new List<CatchesPerYear>
        {
            new() { Year = 2022, TotalCatches = 12, TotalWeightKg = 48.5m, AvgWeightKg = 4.04m },
            new() { Year = 2023, TotalCatches = 18, TotalWeightKg = 72.0m, AvgWeightKg = 4.00m }
        };

        var json    = JsonSerializer.Serialize(data.Select(x => x.TotalCatches));
        var parsed  = JsonSerializer.Deserialize<int[]>(json);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Length);
        Assert.Equal(12, parsed[0]);
        Assert.Equal(18, parsed[1]);
    }

    [Fact]
    public void CatchesByType_Serializes_To_ValidJsonArray()
    {
        var data = new List<CatchesByType>
        {
            new() { TypeName = "Salmon",   TotalCatches = 10, Percentage = 62.5m },
            new() { TypeName = "Sea Trout", TotalCatches = 6,  Percentage = 37.5m }
        };

        var json   = JsonSerializer.Serialize(data.Select(x => x.TypeName));
        var parsed = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(parsed);
        Assert.Equal("Salmon", parsed[0]);
        Assert.Equal("Sea Trout", parsed[1]);
    }

    [Fact]
    public void CatchesPerAngler_OrderedByDescendingCatches()
    {
        var data = new List<CatchesPerAngler>
        {
            new() { AnglerName = "Lars",  TotalCatches = 8,  TotalWeightKg = 32m, BestCatchKg = 7m },
            new() { AnglerName = "Erik",  TotalCatches = 15, TotalWeightKg = 60m, BestCatchKg = 9m },
            new() { AnglerName = "Bjørn", TotalCatches = 5,  TotalWeightKg = 20m, BestCatchKg = 6m }
        };

        var ordered = data.OrderByDescending(a => a.TotalCatches).ToList();

        Assert.Equal("Erik",  ordered[0].AnglerName);
        Assert.Equal("Lars",  ordered[1].AnglerName);
        Assert.Equal("Bjørn", ordered[2].AnglerName);
    }

    [Fact]
    public void BiggestSalmonPerTeam_Serializes_To_ValidJsonArrays()
    {
        var data = new List<BiggestSalmonPerTeam>
        {
            new() { TeamName = "Team Alpha", BiggestSalmonKg = 12.5m, AnglerName = "Lars", TotalSalmonCount = 5, AvgSalmonWeightKg = 8.0m },
            new() { TeamName = "Team Beta",  BiggestSalmonKg = 9.8m,  AnglerName = "Erik", TotalSalmonCount = 3, AvgSalmonWeightKg = 8.5m }
        };

        var labelsJson  = JsonSerializer.Serialize(data.Select(x => x.TeamName));
        var biggestJson = JsonSerializer.Serialize(data.Select(x => x.BiggestSalmonKg));

        var labels  = JsonSerializer.Deserialize<string[]>(labelsJson);
        var biggest = JsonSerializer.Deserialize<decimal[]>(biggestJson);

        Assert.NotNull(labels);
        Assert.Equal(2, labels.Length);
        Assert.Equal("Team Alpha", labels[0]);
        Assert.Equal("Team Beta", labels[1]);

        Assert.NotNull(biggest);
        Assert.Equal(12.5m, biggest[0]);
        Assert.Equal(9.8m, biggest[1]);
    }

    [Fact]
    public void BiggestSalmonPerTeam_OrderedByDescendingBiggestSalmon()
    {
        var data = new List<BiggestSalmonPerTeam>
        {
            new() { TeamName = "Team C", BiggestSalmonKg = 5.0m },
            new() { TeamName = "Team A", BiggestSalmonKg = 15.0m },
            new() { TeamName = "Team B", BiggestSalmonKg = 10.0m }
        };

        var ordered = data.OrderByDescending(t => t.BiggestSalmonKg).ToList();

        Assert.Equal("Team A", ordered[0].TeamName);
        Assert.Equal("Team B", ordered[1].TeamName);
        Assert.Equal("Team C", ordered[2].TeamName);
    }

    // ── Season-progress curve ────────────────────────────────────────

    [Fact]
    public void BuildSeasonProgress_AlignsSeasonsOnSharedWeekAxis()
    {
        var rows = new List<CatchesPerWeek>
        {
            new() { SeasonYear = 2024, WeekNumber = 25, TotalCatches = 4 },
            new() { SeasonYear = 2024, WeekNumber = 27, TotalCatches = 6 },
            new() { SeasonYear = 2025, WeekNumber = 26, TotalCatches = 9 }
        };

        var (labels, series) = IndexModel.BuildSeasonProgress(rows);

        Assert.Equal(["Uge 25", "Uge 26", "Uge 27"], labels);
        Assert.Equal(2, series.Count);

        var s2024 = series.Single(s => s.Year == 2024);
        Assert.Equal([4, null, 6], s2024.Data);

        var s2025 = series.Single(s => s.Year == 2025);
        Assert.Equal([null, 9, null], s2025.Data);
    }

    [Fact]
    public void BuildSeasonProgress_LimitsToMostRecentSeasons()
    {
        var rows = Enumerable.Range(2018, 8)
            .Select(year => new CatchesPerWeek { SeasonYear = year, WeekNumber = 26, TotalCatches = 1 })
            .ToList();

        var (_, series) = IndexModel.BuildSeasonProgress(rows, maxSeasons: 5);

        Assert.Equal(5, series.Count);
        Assert.Equal(2021, series.First().Year);
        Assert.Equal(2025, series.Last().Year);
    }

    [Fact]
    public void BuildSeasonProgress_NoData_ReturnsEmpty()
    {
        var (labels, series) = IndexModel.BuildSeasonProgress([]);

        Assert.Empty(labels);
        Assert.Empty(series);
    }

    // ── Time-of-day buckets ──────────────────────────────────────────

    [Fact]
    public void BuildHourBuckets_FillsAllTwentyFourHours()
    {
        var rows = new List<CatchesByHour>
        {
            new() { Hour = 6, TotalCatches = 8 },
            new() { Hour = 21, TotalCatches = 3 }
        };

        var buckets = IndexModel.BuildHourBuckets(rows);

        Assert.Equal(24, buckets.Length);
        Assert.Equal(8, buckets[6]);
        Assert.Equal(3, buckets[21]);
        Assert.Equal(0, buckets[12]);
    }

    [Fact]
    public void BuildHourBuckets_IgnoresOutOfRangeHours()
    {
        var rows = new List<CatchesByHour>
        {
            new() { Hour = -1, TotalCatches = 5 },
            new() { Hour = 24, TotalCatches = 7 }
        };

        var buckets = IndexModel.BuildHourBuckets(rows);

        Assert.All(buckets, c => Assert.Equal(0, c));
    }

    // ── Water-level bands ────────────────────────────────────────────

    [Fact]
    public void FormatBandLabel_UsesDanishDecimalComma()
    {
        Assert.Equal("1,25–1,50 m", IndexModel.FormatBandLabel(1.25m));
        Assert.Equal("0,00–0,25 m", IndexModel.FormatBandLabel(0m));
    }
}
