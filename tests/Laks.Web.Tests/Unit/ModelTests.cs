using Laks.Web.Models;

namespace Laks.Web.Tests.Unit;

public class ModelTests
{
    [Fact]
    public void Catch_DefaultValues_AreCorrect()
    {
        var c = new Catch();
        Assert.Equal(0, c.Id);
        Assert.Equal(0, c.SeasonYear);
        Assert.Equal(string.Empty, c.Notes);
        Assert.Null(c.AnglerName);
    }

    [Fact]
    public void FishingSeason_DefaultValues_AreCorrect()
    {
        var season = new FishingSeason();
        Assert.Equal(0, season.Year);
        Assert.Equal(0, season.TotalCatches);
        Assert.Equal(0, season.ParticipantCount);
        Assert.Null(season.FirstCatchDate);
    }

    [Fact]
    public void Angler_DefaultValues_AreCorrect()
    {
        var a = new Angler();
        Assert.Equal(0, a.Id);
        Assert.Equal(string.Empty, a.Name);
        Assert.Null(a.Country);
    }

    [Fact]
    public void CatchesPerYear_AllPropertiesSettable()
    {
        var c = new CatchesPerYear
        {
            Year = 2024,
            TotalCatches = 20,
            TotalWeightKg = 80.5m,
            AvgWeightKg = 4.025m
        };
        Assert.Equal(2024, c.Year);
        Assert.Equal(20, c.TotalCatches);
        Assert.Equal(80.5m, c.TotalWeightKg);
    }

    // SeasonDay.DisplayText – Danish strings

    [Fact]
    public void SeasonDay_DisplayText_ActiveDay_ReturnsDanish()
    {
        var day = new SeasonDay
        {
            IsOffSeason = false,
            IsBufferDay = false,
            DayInGroup = 3,
            GroupLengthDays = 7,
            GroupNumber = 2
        };

        Assert.Equal("Dag 3 af 7 · Hold 2", day.DisplayText);
    }

    [Fact]
    public void SeasonDay_DisplayText_FishingStartsToday_ReturnsDanish()
    {
        var day = new SeasonDay
        {
            IsOffSeason = true,
            IsBufferDay = true,
            NextGroupStart = DateTime.UtcNow.Date
        };

        Assert.Equal("Fiskeriet starter i dag", day.DisplayText);
    }

    [Fact]
    public void SeasonDay_DisplayText_FishingStartsTomorrow_ReturnsDanish()
    {
        var day = new SeasonDay
        {
            IsOffSeason = true,
            IsBufferDay = true,
            NextGroupStart = DateTime.UtcNow.Date.AddDays(1)
        };

        Assert.Equal("Fiskeriet starter i morgen", day.DisplayText);
    }

    [Fact]
    public void SeasonDay_DisplayText_FishingStartsInNDays_ReturnsDanish()
    {
        var day = new SeasonDay
        {
            IsOffSeason = true,
            IsBufferDay = true,
            NextGroupStart = DateTime.UtcNow.Date.AddDays(5)
        };

        Assert.Equal("Fiskeriet starter om 5 dage", day.DisplayText);
    }

    [Fact]
    public void SeasonDay_DisplayText_FishingStartsToday_WithTeamName_ReturnsDanish()
    {
        var day = new SeasonDay
        {
            IsOffSeason = true,
            IsBufferDay = true,
            NextGroupStart = DateTime.UtcNow.Date,
            NextGroupTeamName = "Hold Rød"
        };

        Assert.Equal("Fiskeriet starter i dag · Hold Rød", day.DisplayText);
    }

    [Fact]
    public void SeasonDay_DisplayText_NoNextGroupStart_ReturnsOffSeasonDanish()
    {
        var day = new SeasonDay
        {
            IsOffSeason = true,
            IsBufferDay = false,
            NextGroupStart = null
        };

        Assert.Equal("Sæson ikke konfigureret", day.DisplayText);
    }
}
