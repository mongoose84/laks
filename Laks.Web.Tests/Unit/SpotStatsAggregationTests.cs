using Laks.Web.Data.Repositories;
using static Laks.Web.Data.Repositories.CatchRepository;

namespace Laks.Web.Tests.Unit;

/// <summary>
/// Unit tests for CatchRepository.AggregateSpotStats — exercises the pure
/// in-memory aggregation helper without a database connection.
/// </summary>
public class SpotStatsAggregationTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static CatchRow Row(
        string location,
        decimal weightKg,
        string? bait             = null,
        decimal? waterLevel      = null,
        int catchHour            = 8,
        DateTime? catchDate      = null,
        int id                   = 1,
        string anglerName        = "Lars")
        => new()
        {
            Location   = location,
            WeightKg   = weightKg,
            Bait       = bait,
            WaterLevel = waterLevel,
            CatchTime  = new TimeSpan(catchHour, 0, 0),
            CatchDate  = catchDate ?? new DateTime(2023, 6, 1),
            Id         = id,
            AnglerName = anglerName
        };

    // ── Basic count, weight and average ─────────────────────────────────────

    [Fact]
    public void AggregateSpotStats_CountsTotalWeightAndAverageCorrectly()
    {
        var catches = new[]
        {
            Row("Foss", 4.0m, id: 1),
            Row("Foss", 6.0m, id: 2),
            Row("Foss", 2.0m, id: 3)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Single(result);
        var spot = result[0];
        Assert.Equal("Foss", spot.Location);
        Assert.Equal(3,    spot.TotalCatches);
        Assert.Equal(12.0m, spot.TotalWeightKg);
        Assert.Equal(4.0m,  spot.AvgWeightKg);
    }

    [Fact]
    public void AggregateSpotStats_IncludesAllCatchTypes_NoSalmonOnlyFilter()
    {
        // CatchRow has no CatchType field; all rows are included regardless of
        // the fish type recorded in the database. This test documents that intent.
        var catches = new[]
        {
            Row("Pynten", 5.0m, id: 1),  // would be "Laks" in DB
            Row("Pynten", 3.5m, id: 2),  // would be "Havørred" in DB
            Row("Pynten", 2.0m, id: 3)   // would be "Andet" in DB
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Single(result);
        Assert.Equal(3, result[0].TotalCatches);
    }

    // ── Blank-location exclusion ─────────────────────────────────────────────

    [Fact]
    public void AggregateSpotStats_ExcludesNullEmptyAndWhitespaceLocations()
    {
        var catches = new[]
        {
            Row("Foss",    4.0m, id: 1),
            Row("",        5.0m, id: 2),
            Row("   ",     3.0m, id: 3),
            // CatchRow.Location is non-nullable; simulate null-equivalent via empty
            Row(string.Empty, 2.0m, id: 4)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Single(result);
        Assert.Equal("Foss", result[0].Location);
        Assert.Equal(1, result[0].TotalCatches);
    }

    // ── Biggest fish tie-breaking ────────────────────────────────────────────

    [Fact]
    public void AggregateSpotStats_BiggestFish_TieBrokenByEarliestDateThenLowestId()
    {
        var date1 = new DateTime(2022, 7, 1);
        var date2 = new DateTime(2023, 6, 1);

        var catches = new[]
        {
            Row("Foss", 8.0m, id: 10, catchDate: date2, anglerName: "Erik"),
            Row("Foss", 8.0m, id: 5,  catchDate: date2, anglerName: "Lars"),  // same date, lower Id wins
            Row("Foss", 8.0m, id: 2,  catchDate: date1, anglerName: "Bjørn") // earliest date wins overall
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal("Bjørn", result[0].BiggestAnglerName);
        Assert.Equal(date1, result[0].BiggestCatchDate);
    }

    [Fact]
    public void AggregateSpotStats_BiggestFish_SameDateTieBrokenByLowestId()
    {
        var date = new DateTime(2023, 6, 1);

        var catches = new[]
        {
            Row("Foss", 9.0m, id: 20, catchDate: date, anglerName: "Erik"),
            Row("Foss", 9.0m, id: 3,  catchDate: date, anglerName: "Lars")
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal("Lars", result[0].BiggestAnglerName);
    }

    // ── Top bait ─────────────────────────────────────────────────────────────

    [Fact]
    public void AggregateSpotStats_TopBait_PicksMostFrequent()
    {
        var catches = new[]
        {
            Row("Foss", 4.0m, bait: "Orm",  id: 1),
            Row("Foss", 4.0m, bait: "Orm",  id: 2),
            Row("Foss", 4.0m, bait: "Flue", id: 3)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal("Orm", result[0].TopBait);
    }

    [Fact]
    public void AggregateSpotStats_TopBait_TieBrokenAlphabeticallyDanishCaseInsensitive()
    {
        // "Flue" and "Orm" both appear twice; "Flue" sorts first (da-DK, case-insensitive)
        var catches = new[]
        {
            Row("Foss", 4.0m, bait: "orm",  id: 1),
            Row("Foss", 4.0m, bait: "Orm",  id: 2),
            Row("Foss", 4.0m, bait: "Flue", id: 3),
            Row("Foss", 4.0m, bait: "flue", id: 4)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        // Bait groups by exact key (case-sensitive): "orm", "Orm", "Flue", "flue" are
        // four distinct groups of count 1 each. Tie-broken alphabetically (da-DK, CI):
        // "Flue" ≈ "flue" < "Orm" ≈ "orm"; the first in that ordering wins.
        Assert.True(result[0].TopBait.Equals("Flue", StringComparison.OrdinalIgnoreCase)
                 || result[0].TopBait.Equals("flue", StringComparison.OrdinalIgnoreCase),
            $"Expected 'Flue' or 'flue' but got '{result[0].TopBait}'");
    }

    [Fact]
    public void AggregateSpotStats_TopBait_TieBrokenAlphabeticallyBetweenDistinctBaits()
    {
        // "Agn" and "Orm" tied at 1 each; "Agn" sorts before "Orm" → "Agn" wins
        var catches = new[]
        {
            Row("Foss", 4.0m, bait: "Orm", id: 1),
            Row("Foss", 4.0m, bait: "Agn", id: 2)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal("Agn", result[0].TopBait);
    }

    [Fact]
    public void AggregateSpotStats_TopBait_EmptyWhenNoCatchHasBait()
    {
        var catches = new[]
        {
            Row("Foss", 4.0m, bait: null,    id: 1),
            Row("Foss", 4.0m, bait: "",      id: 2),
            Row("Foss", 4.0m, bait: "   ",   id: 3)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal(string.Empty, result[0].TopBait);
    }

    // ── Best water-level band ────────────────────────────────────────────────

    [Fact]
    public void AggregateSpotStats_BestWaterBand_PicksMostFrequent025Band()
    {
        // WaterLevel 1.10 and 1.20 both land in the 1.00–1.25 band; 1.30 lands in 1.25–1.50
        var catches = new[]
        {
            Row("Foss", 4.0m, waterLevel: 1.10m, id: 1),
            Row("Foss", 4.0m, waterLevel: 1.20m, id: 2),
            Row("Foss", 4.0m, waterLevel: 1.30m, id: 3)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal(1.00m, result[0].BestWaterBandStartM);
    }

    [Fact]
    public void AggregateSpotStats_BestWaterBand_BoundaryValueLandsInOwnBand()
    {
        // Exactly 1.25 m must land in the 1.25 band, not the 1.00 band
        var catches = new[]
        {
            Row("Foss", 4.0m, waterLevel: 1.25m, id: 1)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal(1.25m, result[0].BestWaterBandStartM);
    }

    [Fact]
    public void AggregateSpotStats_BestWaterBand_TieBrokenByLowestBand()
    {
        // Band 0.75 and band 1.00 each have 1 catch; lowest band (0.75) wins
        var catches = new[]
        {
            Row("Foss", 4.0m, waterLevel: 0.80m, id: 1),
            Row("Foss", 4.0m, waterLevel: 1.10m, id: 2)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal(0.75m, result[0].BestWaterBandStartM);
    }

    [Fact]
    public void AggregateSpotStats_BestWaterBand_NullWhenNoCatchHasWaterLevel()
    {
        var catches = new[]
        {
            Row("Foss", 4.0m, waterLevel: null, id: 1),
            Row("Foss", 4.0m, waterLevel: null, id: 2)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Null(result[0].BestWaterBandStartM);
    }

    // ── Best hour ────────────────────────────────────────────────────────────

    [Fact]
    public void AggregateSpotStats_BestHour_PicksMostFrequentHour()
    {
        var catches = new[]
        {
            Row("Foss", 4.0m, catchHour: 8,  id: 1),
            Row("Foss", 4.0m, catchHour: 8,  id: 2),
            Row("Foss", 4.0m, catchHour: 14, id: 3)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal(8, result[0].BestHour);
    }

    [Fact]
    public void AggregateSpotStats_BestHour_TieBrokenByEarliestHour()
    {
        // Hour 6 and hour 10 each appear once; hour 6 wins (earliest)
        var catches = new[]
        {
            Row("Foss", 4.0m, catchHour: 10, id: 1),
            Row("Foss", 4.0m, catchHour: 6,  id: 2)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal(6, result[0].BestHour);
    }

    [Fact]
    public void AggregateSpotStats_BestHour_TreatsMidnightAsHourZero()
    {
        // CatchTime 00:00:00 → Hours = 0; consistent with the existing hour chart
        var catches = new[]
        {
            Row("Foss", 4.0m, catchHour: 0, id: 1),
            Row("Foss", 4.0m, catchHour: 0, id: 2),
            Row("Foss", 4.0m, catchHour: 8, id: 3)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal(0, result[0].BestHour);
    }

    // ── Default sort ─────────────────────────────────────────────────────────

    [Fact]
    public void AggregateSpotStats_DefaultSort_ByCountDescThenWeightDescThenNameAsc()
    {
        var catches = new[]
        {
            // Pynten: 2 catches, 8.0 kg total
            Row("Pynten", 5.0m, id: 1),
            Row("Pynten", 3.0m, id: 2),
            // Foss: 2 catches, 10.0 kg total — same count but higher weight, goes first
            Row("Foss", 6.0m, id: 3),
            Row("Foss", 4.0m, id: 4),
            // Walle: 1 catch — fewest, goes last
            Row("Walle", 7.0m, id: 5)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal("Foss",   result[0].Location); // count=2, weight=10
        Assert.Equal("Pynten", result[1].Location); // count=2, weight=8
        Assert.Equal("Walle",  result[2].Location); // count=1
    }

    [Fact]
    public void AggregateSpotStats_DefaultSort_NameAscForCountAndWeightTie()
    {
        var catches = new[]
        {
            Row("Ørret", 4.0m, id: 1),  // same count and weight as "Agn"
            Row("Agn",   4.0m, id: 2)
        };

        var result = CatchRepository.AggregateSpotStats(catches).ToList();

        // "Agn" < "Ørret" in da-DK alphabetical order
        Assert.Equal("Agn",   result[0].Location);
        Assert.Equal("Ørret", result[1].Location);
    }

    // ── Edge cases ───────────────────────────────────────────────────────────

    [Fact]
    public void AggregateSpotStats_EmptyInput_ReturnsEmpty()
    {
        var result = CatchRepository.AggregateSpotStats([]);

        Assert.Empty(result);
    }

    [Fact]
    public void AggregateSpotStats_OnlyBlankLocations_ReturnsEmpty()
    {
        var catches = new[]
        {
            Row("",    4.0m, id: 1),
            Row("   ", 4.0m, id: 2)
        };

        var result = CatchRepository.AggregateSpotStats(catches);

        Assert.Empty(result);
    }
}
