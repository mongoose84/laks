using Laks.Web.Models;

namespace Laks.Web.Tests.Unit;

public class ModelTests
{
    [Fact]
    public void Catch_DefaultValues_AreCorrect()
    {
        var c = new Catch();
        Assert.Equal(0, c.Id);
        Assert.False(c.Released);
        Assert.Null(c.Notes);
        Assert.Null(c.AnglerName);
    }

    [Fact]
    public void Trip_DefaultValues_AreCorrect()
    {
        var t = new Trip();
        Assert.Equal(0, t.Id);
        Assert.Equal(0, t.Year);
        Assert.Equal(string.Empty, t.RiverName);
        Assert.Equal(string.Empty, t.Location);
    }

    [Fact]
    public void Species_DefaultValues_AreCorrect()
    {
        var s = new Species();
        Assert.Equal(0, s.Id);
        Assert.Equal(string.Empty, s.Name);
        Assert.Null(s.NorwegianName);
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
}
