using Laks.Web.Models;

namespace Laks.Web.Services;

public interface IWaterLevelService
{
    Task<WaterLevelSnapshot?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WaterLevelReading>> GetLast24HoursAsync(CancellationToken cancellationToken = default);
}
