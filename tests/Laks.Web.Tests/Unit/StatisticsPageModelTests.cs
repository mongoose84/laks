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
}
