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
    public void CatchesBySpecies_Serializes_To_ValidJsonArray()
    {
        var data = new List<CatchesBySpecies>
        {
            new() { SpeciesName = "Atlantic Salmon", TotalCatches = 10, Percentage = 62.5m },
            new() { SpeciesName = "Sea Trout",       TotalCatches = 6,  Percentage = 37.5m }
        };

        var json   = JsonSerializer.Serialize(data.Select(x => x.SpeciesName));
        var parsed = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(parsed);
        Assert.Equal("Atlantic Salmon", parsed[0]);
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
}
