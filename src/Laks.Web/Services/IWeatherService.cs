using Laks.Web.Models;

namespace Laks.Web.Services;

public interface IWeatherService
{
    Task<WeatherData?> GetCurrentAsync(CancellationToken cancellationToken = default);
}
