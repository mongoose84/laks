using Laks.Web.Models;
using Laks.Web.Services;

namespace Laks.Web.Tests.Unit;

public class WaterLevelServiceTests
{
    [Theory]
    [InlineData(1.30, 1.20, WaterLevelTrend.Rising)]
    [InlineData(1.20, 1.30, WaterLevelTrend.Falling)]
    [InlineData(1.20, 1.19, WaterLevelTrend.Stable)]
    [InlineData(1.20, null, WaterLevelTrend.Stable)]
    public void CalculateTrend_ReturnsExpectedTrend(double latest, double? prior, WaterLevelTrend expected)
    {
        var trend = WaterLevelService.CalculateTrend((decimal)latest, prior is null ? null : (decimal?)prior.Value);
        Assert.Equal(expected, trend);
    }
}
