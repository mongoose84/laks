using Laks.Web.Models;
using Laks.Web.Pages.Anglers;

namespace Laks.Web.Tests.Unit;

public class AnglerProfileHelpersTests
{
    // ── ClassifyHour Theory ──────────────────────────────────────────────────

    [Theory]
    [InlineData(0, "Nat")]
    [InlineData(1, "Nat")]
    [InlineData(2, "Nat")]
    [InlineData(3, "Nat")]
    [InlineData(4, "Morgen")]
    [InlineData(5, "Morgen")]
    [InlineData(6, "Morgen")]
    [InlineData(7, "Morgen")]
    [InlineData(8, "Morgen")]
    [InlineData(9, "Morgen")]
    [InlineData(10, "Dag")]
    [InlineData(11, "Dag")]
    [InlineData(12, "Dag")]
    [InlineData(13, "Dag")]
    [InlineData(14, "Dag")]
    [InlineData(15, "Dag")]
    [InlineData(16, "Aften")]
    [InlineData(17, "Aften")]
    [InlineData(18, "Aften")]
    [InlineData(19, "Aften")]
    [InlineData(20, "Aften")]
    [InlineData(21, "Aften")]
    [InlineData(22, "Nat")]
    [InlineData(23, "Nat")]
    public void ClassifyHour_ReturnsCorrectBucket(int hour, string expected)
    {
        Assert.Equal(expected, IndexModel.ClassifyHour(hour));
    }

    // ── ClassifyHour boundary ────────────────────────────────────────────────

    [Fact]
    public void ClassifyHour_Hour03_IsNat()
    {
        Assert.Equal("Nat", IndexModel.ClassifyHour(3));
    }

    [Fact]
    public void ClassifyHour_Hour04_IsMorgen()
    {
        Assert.Equal("Morgen", IndexModel.ClassifyHour(4));
    }

    [Fact]
    public void ClassifyHour_Hour09_IsMorgen()
    {
        Assert.Equal("Morgen", IndexModel.ClassifyHour(9));
    }

    [Fact]
    public void ClassifyHour_Hour10_IsDag()
    {
        Assert.Equal("Dag", IndexModel.ClassifyHour(10));
    }

    [Fact]
    public void ClassifyHour_Hour15_IsDag()
    {
        Assert.Equal("Dag", IndexModel.ClassifyHour(15));
    }

    [Fact]
    public void ClassifyHour_Hour16_IsAften()
    {
        Assert.Equal("Aften", IndexModel.ClassifyHour(16));
    }

    [Fact]
    public void ClassifyHour_Hour21_IsAften()
    {
        Assert.Equal("Aften", IndexModel.ClassifyHour(21));
    }

    [Fact]
    public void ClassifyHour_Hour22_IsNat()
    {
        Assert.Equal("Nat", IndexModel.ClassifyHour(22));
    }

    // ── BuildCareerStats ─────────────────────────────────────────────────────

    [Fact]
    public void BuildCareerStats_EmptyList_ReturnsZeroStats()
    {
        var result = IndexModel.BuildCareerStats([]);

        Assert.Equal(0, result.FishCount);
        Assert.Equal(0m, result.TotalWeightKg);
        Assert.Equal(0m, result.BestWeightKg);
        Assert.Equal(0, result.SeasonsActive);
        Assert.Null(result.FirstSeasonYear);
        Assert.Null(result.LastSeasonYear);
    }

    [Fact]
    public void BuildCareerStats_MultiSeason_ComputesCorrectly()
    {
        var catches = new List<Catch>
        {
            new() { SeasonYear = 2020, WeightKg = 5.0m, CatchDate = new DateTime(2020, 6, 20), CatchTime = TimeSpan.Zero },
            new() { SeasonYear = 2022, WeightKg = 10.0m, CatchDate = new DateTime(2022, 6, 22), CatchTime = TimeSpan.Zero },
            new() { SeasonYear = 2022, WeightKg = 7.5m, CatchDate = new DateTime(2022, 6, 23), CatchTime = TimeSpan.Zero },
            new() { SeasonYear = 2024, WeightKg = 3.0m, CatchDate = new DateTime(2024, 7, 1), CatchTime = TimeSpan.Zero }
        };

        var result = IndexModel.BuildCareerStats(catches);

        Assert.Equal(4, result.FishCount);
        Assert.Equal(25.5m, result.TotalWeightKg);
        Assert.Equal(10.0m, result.BestWeightKg);
        Assert.Equal(2022, result.BestWeightYear);
        Assert.Equal(3, result.SeasonsActive);
        Assert.Equal(2020, result.FirstSeasonYear);
        Assert.Equal(2024, result.LastSeasonYear);
    }

    // ── BuildSeasonHistory ───────────────────────────────────────────────────

    [Fact]
    public void BuildSeasonHistory_GroupsByYear_NewestFirst()
    {
        var catches = new List<Catch>
        {
            new() { AnglerId = 1, SeasonYear = 2023, WeightKg = 6.0m, CatchDate = new DateTime(2023, 6, 20), CatchTime = TimeSpan.Zero },
            new() { AnglerId = 1, SeasonYear = 2024, WeightKg = 8.0m, CatchDate = new DateTime(2024, 6, 20), CatchTime = TimeSpan.Zero },
            new() { AnglerId = 1, SeasonYear = 2024, WeightKg = 4.0m, CatchDate = new DateTime(2024, 6, 25), CatchTime = TimeSpan.Zero }
        };

        var leaderboards = new Dictionary<int, IReadOnlyList<LeaderboardEntry>>
        {
            [2023] = [new LeaderboardEntry { AnglerId = 1, Rank = 3, FishCount = 1, TotalWeightKg = 6m, BestWeightKg = 6m }],
            [2024] = [new LeaderboardEntry { AnglerId = 1, Rank = 1, FishCount = 2, TotalWeightKg = 12m, BestWeightKg = 8m }]
        };

        var result = IndexModel.BuildSeasonHistory(catches, leaderboards, anglerId: 1);

        Assert.Equal(2, result.Count);
        Assert.Equal(2024, result[0].Year);
        Assert.Equal(2023, result[1].Year);
        Assert.Equal(1, result[0].Rank);
        Assert.Equal(3, result[1].Rank);
    }

    [Fact]
    public void BuildSeasonHistory_MissingLeaderboard_RankIsNull()
    {
        var catches = new List<Catch>
        {
            new() { AnglerId = 1, SeasonYear = 2020, WeightKg = 5.0m, CatchDate = new DateTime(2020, 6, 20), CatchTime = TimeSpan.Zero }
        };

        var result = IndexModel.BuildSeasonHistory(catches, new Dictionary<int, IReadOnlyList<LeaderboardEntry>>(), anglerId: 1);

        Assert.Single(result);
        Assert.Null(result[0].Rank);
    }

    [Fact]
    public void BuildSeasonHistory_AnglerNotOnLeaderboard_RankIsNull()
    {
        var catches = new List<Catch>
        {
            new() { AnglerId = 99, SeasonYear = 2021, WeightKg = 4.0m, CatchDate = new DateTime(2021, 6, 20), CatchTime = TimeSpan.Zero }
        };

        var leaderboards = new Dictionary<int, IReadOnlyList<LeaderboardEntry>>
        {
            [2021] = [new LeaderboardEntry { AnglerId = 1, Rank = 1, FishCount = 3, TotalWeightKg = 20m, BestWeightKg = 9m }]
        };

        var result = IndexModel.BuildSeasonHistory(catches, leaderboards, anglerId: 99);

        Assert.Single(result);
        Assert.Null(result[0].Rank);
    }

    // ── BuildCurrentSeasonStats ──────────────────────────────────────────────

    [Fact]
    public void BuildCurrentSeasonStats_NoCatchesNoGroup_ReturnsNull()
    {
        var result = IndexModel.BuildCurrentSeasonStats([], 2026, [], null, anglerId: 0);

        Assert.Null(result);
    }

    [Fact]
    public void BuildCurrentSeasonStats_GroupRegisteredNoCatches_ReturnsStripWithZeroCatches()
    {
        var result = IndexModel.BuildCurrentSeasonStats([], 2026, [], groupNumber: 2, anglerId: 0);

        Assert.NotNull(result);
        Assert.Equal(0, result!.FishCount);
        Assert.Equal(2, result.GroupNumber);
        Assert.Null(result.Rank);
    }

    [Fact]
    public void BuildCurrentSeasonStats_WithCatches_ComputesRankFromLeaderboard()
    {
        var catches = new List<Catch>
        {
            new() { AnglerId = 5, SeasonYear = 2026, WeightKg = 9.0m, CatchDate = new DateTime(2026, 6, 20), CatchTime = TimeSpan.Zero },
            new() { AnglerId = 5, SeasonYear = 2026, WeightKg = 4.5m, CatchDate = new DateTime(2026, 6, 21), CatchTime = TimeSpan.Zero }
        };

        var leaderboard = new List<LeaderboardEntry>
        {
            new() { AnglerId = 3, Rank = 1, FishCount = 5, TotalWeightKg = 40m, BestWeightKg = 12m },
            new() { AnglerId = 5, Rank = 2, FishCount = 2, TotalWeightKg = 13.5m, BestWeightKg = 9m },
            new() { AnglerId = 7, Rank = 3, FishCount = 1, TotalWeightKg = 5m, BestWeightKg = 5m }
        };

        var result = IndexModel.BuildCurrentSeasonStats(catches, 2026, leaderboard, null, anglerId: 5);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Rank);
        Assert.Equal(3, result.LeaderboardSize);
        Assert.Equal(2, result.FishCount);
        Assert.Equal(13.5m, result.TotalWeightKg);
        Assert.Equal(9.0m, result.BestWeightKg);
        Assert.Null(result.GroupNumber);
    }

    [Fact]
    public void BuildCurrentSeasonStats_AnglerNotOnLeaderboard_RankIsNull()
    {
        var catches = new List<Catch>
        {
            new() { AnglerId = 99, SeasonYear = 2026, WeightKg = 5m, CatchDate = new DateTime(2026, 6, 20), CatchTime = TimeSpan.Zero }
        };

        var leaderboard = new List<LeaderboardEntry>
        {
            new() { AnglerId = 1, Rank = 1, FishCount = 3, TotalWeightKg = 20m, BestWeightKg = 9m }
        };

        var result = IndexModel.BuildCurrentSeasonStats(catches, 2026, leaderboard, null, anglerId: 99);

        Assert.NotNull(result);
        Assert.Null(result!.Rank);
    }

    // ── BuildTopBaits ────────────────────────────────────────────────────────

    [Fact]
    public void BuildTopBaits_EmptyList_ReturnsEmpty()
    {
        var result = IndexModel.BuildTopBaits([]);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildTopBaits_AllBlankBaits_ReturnsEmpty()
    {
        var catches = new List<Catch>
        {
            new() { Bait = "", CatchDate = new DateTime(2026, 6, 1), CatchTime = TimeSpan.Zero },
            new() { Bait = "   ", CatchDate = new DateTime(2026, 6, 1), CatchTime = TimeSpan.Zero }
        };
        var result = IndexModel.BuildTopBaits(catches);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildTopBaits_CaseInsensitiveMerge_GroupsCorrectly()
    {
        var catches = new List<Catch>
        {
            new() { Bait = "Flue", CatchDate = new DateTime(2026, 6, 28), CatchTime = TimeSpan.Zero },
            new() { Bait = "flue", CatchDate = new DateTime(2026, 6, 27), CatchTime = TimeSpan.Zero },
            new() { Bait = "FLUE", CatchDate = new DateTime(2026, 6, 26), CatchTime = TimeSpan.Zero },
            new() { Bait = "Spinner", CatchDate = new DateTime(2026, 6, 25), CatchTime = TimeSpan.Zero }
        };

        var result = IndexModel.BuildTopBaits(catches);

        Assert.Equal(2, result.Count);
        Assert.Equal("Flue", result[0].Bait);  // first in source (date-DESC order)
        Assert.Equal(3, result[0].CatchCount);
        Assert.Equal(75.0m, result[0].SharePct);
        Assert.Equal("Spinner", result[1].Bait);
    }

    [Fact]
    public void BuildTopBaits_WhitespaceTrimmedAndExcludedFromDenominator()
    {
        var catches = new List<Catch>
        {
            new() { Bait = "Flue", CatchDate = new DateTime(2026, 6, 1), CatchTime = TimeSpan.Zero },
            new() { Bait = "Flue", CatchDate = new DateTime(2026, 6, 2), CatchTime = TimeSpan.Zero },
            new() { Bait = "", CatchDate = new DateTime(2026, 6, 3), CatchTime = TimeSpan.Zero }   // excluded from denominator
        };

        var result = IndexModel.BuildTopBaits(catches);

        Assert.Single(result);
        Assert.Equal("Flue", result[0].Bait);
        // Denominator is 2 (with bait), not 3
        Assert.Equal(100.0m, result[0].SharePct);
    }

    [Fact]
    public void BuildTopBaits_TieBreakByNameOrdinalIgnoreCase()
    {
        var catches = new List<Catch>
        {
            new() { Bait = "Spinner", CatchDate = new DateTime(2026, 6, 28), CatchTime = TimeSpan.Zero },
            new() { Bait = "Flue", CatchDate = new DateTime(2026, 6, 27), CatchTime = TimeSpan.Zero }
        };

        var result = IndexModel.BuildTopBaits(catches);

        // Both have count=1; Flue < Spinner alphabetically
        Assert.Equal(2, result.Count);
        Assert.Equal("Flue", result[0].Bait);
        Assert.Equal("Spinner", result[1].Bait);
    }

    [Fact]
    public void BuildTopBaits_CapsAtTopN()
    {
        var catches = Enumerable.Range(1, 5)
            .Select(i => new Catch { Bait = $"Agn{i}", CatchDate = new DateTime(2026, 6, i), CatchTime = TimeSpan.Zero })
            .ToList();

        var result = IndexModel.BuildTopBaits(catches, top: 3);

        Assert.Equal(3, result.Count);
    }

    // ── BuildTimeOfDay ───────────────────────────────────────────────────────

    [Fact]
    public void BuildTimeOfDay_EmptyList_ReturnsNullWinner()
    {
        var result = IndexModel.BuildTimeOfDay([]);

        Assert.Null(result.WinnerBucket);
    }

    [Fact]
    public void BuildTimeOfDay_AllZero_ReturnsNullWinner()
    {
        // This shouldn't normally happen (empty list handled above), but guard
        var result = IndexModel.BuildTimeOfDay([]);
        Assert.Null(result.WinnerBucket);
    }

    [Fact]
    public void BuildTimeOfDay_BoundaryHour03_IsNat()
    {
        var catches = new List<Catch> { new() { CatchTime = new TimeSpan(3, 59, 0), CatchDate = new DateTime(2026, 6, 1) } };
        var result = IndexModel.BuildTimeOfDay(catches);
        Assert.Equal(1, result.NightCount);
        Assert.Equal(0, result.MorningCount);
    }

    [Fact]
    public void BuildTimeOfDay_BoundaryHour04_IsMorgen()
    {
        var catches = new List<Catch> { new() { CatchTime = new TimeSpan(4, 0, 0), CatchDate = new DateTime(2026, 6, 1) } };
        var result = IndexModel.BuildTimeOfDay(catches);
        Assert.Equal(1, result.MorningCount);
        Assert.Equal(0, result.NightCount);
    }

    [Fact]
    public void BuildTimeOfDay_BoundaryHour09_IsMorgen()
    {
        var catches = new List<Catch> { new() { CatchTime = new TimeSpan(9, 59, 0), CatchDate = new DateTime(2026, 6, 1) } };
        var result = IndexModel.BuildTimeOfDay(catches);
        Assert.Equal(1, result.MorningCount);
    }

    [Fact]
    public void BuildTimeOfDay_BoundaryHour10_IsDag()
    {
        var catches = new List<Catch> { new() { CatchTime = new TimeSpan(10, 0, 0), CatchDate = new DateTime(2026, 6, 1) } };
        var result = IndexModel.BuildTimeOfDay(catches);
        Assert.Equal(1, result.DayCount);
        Assert.Equal(0, result.MorningCount);
    }

    [Fact]
    public void BuildTimeOfDay_BoundaryHour15_IsDag()
    {
        var catches = new List<Catch> { new() { CatchTime = new TimeSpan(15, 59, 0), CatchDate = new DateTime(2026, 6, 1) } };
        var result = IndexModel.BuildTimeOfDay(catches);
        Assert.Equal(1, result.DayCount);
    }

    [Fact]
    public void BuildTimeOfDay_BoundaryHour16_IsAften()
    {
        var catches = new List<Catch> { new() { CatchTime = new TimeSpan(16, 0, 0), CatchDate = new DateTime(2026, 6, 1) } };
        var result = IndexModel.BuildTimeOfDay(catches);
        Assert.Equal(1, result.EveningCount);
        Assert.Equal(0, result.DayCount);
    }

    [Fact]
    public void BuildTimeOfDay_BoundaryHour21_IsAften()
    {
        var catches = new List<Catch> { new() { CatchTime = new TimeSpan(21, 59, 0), CatchDate = new DateTime(2026, 6, 1) } };
        var result = IndexModel.BuildTimeOfDay(catches);
        Assert.Equal(1, result.EveningCount);
    }

    [Fact]
    public void BuildTimeOfDay_BoundaryHour22_IsNat()
    {
        var catches = new List<Catch> { new() { CatchTime = new TimeSpan(22, 0, 0), CatchDate = new DateTime(2026, 6, 1) } };
        var result = IndexModel.BuildTimeOfDay(catches);
        Assert.Equal(1, result.NightCount);
        Assert.Equal(0, result.EveningCount);
    }

    [Fact]
    public void BuildTimeOfDay_Hour00_LegacyCountsAsNat()
    {
        var catches = new List<Catch> { new() { CatchTime = new TimeSpan(0, 0, 0), CatchDate = new DateTime(2026, 6, 1) } };
        var result = IndexModel.BuildTimeOfDay(catches);
        Assert.Equal(1, result.NightCount);
        Assert.Equal("Nat", result.WinnerBucket);
    }

    [Fact]
    public void BuildTimeOfDay_TieMorgenWins()
    {
        // Morgen and Dag both have 1 catch — tie resolves to Morgen
        var catches = new List<Catch>
        {
            new() { CatchTime = new TimeSpan(6, 0, 0), CatchDate = new DateTime(2026, 6, 1) },   // Morgen
            new() { CatchTime = new TimeSpan(12, 0, 0), CatchDate = new DateTime(2026, 6, 2) }   // Dag
        };

        var result = IndexModel.BuildTimeOfDay(catches);

        Assert.Equal("Morgen", result.WinnerBucket);
    }

    [Fact]
    public void BuildTimeOfDay_WinnerSharePctIsCorrect()
    {
        var catches = new List<Catch>
        {
            new() { CatchTime = new TimeSpan(6, 0, 0), CatchDate = new DateTime(2026, 6, 1) },
            new() { CatchTime = new TimeSpan(7, 0, 0), CatchDate = new DateTime(2026, 6, 2) },
            new() { CatchTime = new TimeSpan(12, 0, 0), CatchDate = new DateTime(2026, 6, 3) }
        };

        var result = IndexModel.BuildTimeOfDay(catches);

        Assert.Equal("Morgen", result.WinnerBucket);
        // 2 of 3 = 66.7 %
        Assert.Equal(66.7m, result.WinnerSharePct);
    }

    [Fact]
    public void BuildTimeOfDay_TieDagWinsWhenMorgenIsZero()
    {
        // Dag and Aften both have 1 catch, Morgen = 0 → Dag wins (second priority after Morgen)
        var catches = new List<Catch>
        {
            new() { CatchTime = new TimeSpan(11, 0, 0), CatchDate = new DateTime(2026, 6, 1) },  // Dag
            new() { CatchTime = new TimeSpan(17, 0, 0), CatchDate = new DateTime(2026, 6, 2) }   // Aften
        };

        var result = IndexModel.BuildTimeOfDay(catches);

        Assert.Equal("Dag", result.WinnerBucket);
    }

    [Fact]
    public void BuildTimeOfDay_TieAftenWinsWhenMorgenAndDagAreZero()
    {
        // Only Aften and Nat have catches; Aften = Nat = 1 → Aften wins (third priority)
        var catches = new List<Catch>
        {
            new() { CatchTime = new TimeSpan(18, 0, 0), CatchDate = new DateTime(2026, 6, 1) },  // Aften
            new() { CatchTime = new TimeSpan(23, 0, 0), CatchDate = new DateTime(2026, 6, 2) }   // Nat
        };

        var result = IndexModel.BuildTimeOfDay(catches);

        Assert.Equal("Aften", result.WinnerBucket);
    }

    // ── BuildCareerStats edge cases ──────────────────────────────────────────

    [Fact]
    public void BuildCareerStats_SingleSeasonAngler_FirstAndLastYearSame()
    {
        var catches = new List<Catch>
        {
            new() { SeasonYear = 2023, WeightKg = 7.0m, CatchDate = new DateTime(2023, 6, 20), CatchTime = TimeSpan.Zero },
            new() { SeasonYear = 2023, WeightKg = 5.0m, CatchDate = new DateTime(2023, 6, 22), CatchTime = TimeSpan.Zero }
        };

        var result = IndexModel.BuildCareerStats(catches);

        Assert.Equal(1, result.SeasonsActive);
        Assert.Equal(2023, result.FirstSeasonYear);
        Assert.Equal(2023, result.LastSeasonYear);
    }

    // ── BuildSeasonHistory edge cases ────────────────────────────────────────

    [Fact]
    public void BuildSeasonHistory_EmptyCatches_ReturnsEmptyList()
    {
        var result = IndexModel.BuildSeasonHistory(
            [],
            new Dictionary<int, IReadOnlyList<LeaderboardEntry>>(),
            anglerId: 1);

        Assert.Empty(result);
    }

    // ── BuildCurrentSeasonStats edge cases ───────────────────────────────────

    [Fact]
    public void BuildCurrentSeasonStats_CatchesOnlyInPreviousYear_ReturnsNull()
    {
        // All catches are from 2025; current year is 2026; no group registered
        var catches = new List<Catch>
        {
            new() { AnglerId = 1, SeasonYear = 2025, WeightKg = 5m, CatchDate = new DateTime(2025, 6, 20), CatchTime = TimeSpan.Zero }
        };

        var result = IndexModel.BuildCurrentSeasonStats(catches, currentYear: 2026, [], null, anglerId: 1);

        Assert.Null(result);
    }

    [Fact]
    public void BuildCurrentSeasonStats_CurrentYearCatchAndGroup_BothPresent()
    {
        // Both a catch and a group registration exist; GroupNumber should be surfaced
        var catches = new List<Catch>
        {
            new() { AnglerId = 7, SeasonYear = 2026, WeightKg = 6m, CatchDate = new DateTime(2026, 6, 21), CatchTime = TimeSpan.Zero }
        };
        var leaderboard = new List<LeaderboardEntry>
        {
            new() { AnglerId = 7, Rank = 2, FishCount = 1, TotalWeightKg = 6m, BestWeightKg = 6m }
        };

        var result = IndexModel.BuildCurrentSeasonStats(catches, 2026, leaderboard, groupNumber: 1, anglerId: 7);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Rank);
        Assert.Equal(1, result.GroupNumber);
    }

    // ── BuildTopBaits denominator ────────────────────────────────────────────

    [Fact]
    public void BuildTopBaits_PercentageUsesOnlyBaitedCatchesAsDenominator()
    {
        // 3 catches with bait "Flue", 1 catch with no bait → denominator = 3, pct = 100%
        var catches = new List<Catch>
        {
            new() { Bait = "Flue", CatchDate = new DateTime(2026, 6, 1), CatchTime = TimeSpan.Zero },
            new() { Bait = "Flue", CatchDate = new DateTime(2026, 6, 2), CatchTime = TimeSpan.Zero },
            new() { Bait = "Flue", CatchDate = new DateTime(2026, 6, 3), CatchTime = TimeSpan.Zero },
            new() { Bait = "",     CatchDate = new DateTime(2026, 6, 4), CatchTime = TimeSpan.Zero }
        };

        var result = IndexModel.BuildTopBaits(catches);

        Assert.Single(result);
        Assert.Equal(100.0m, result[0].SharePct);
        Assert.Equal(3, result[0].CatchCount);
    }
}
