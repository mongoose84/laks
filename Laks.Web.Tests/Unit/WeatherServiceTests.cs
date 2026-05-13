using System.Net;
using System.Text;
using Laks.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laks.Web.Tests.Unit;

public class WeatherServiceTests
{
    [Fact]
    public async Task GetCurrentAsync_ParsesAndCachesWeatherData()
    {
        const string payload = """
        {
          "properties": {
            "timeseries": [
              {
                "time": "2026-06-26T08:00:00Z",
                "data": {
                  "instant": {
                    "details": {
                      "air_temperature": 12.5,
                      "wind_speed": 4.3,
                      "wind_from_direction": 90
                    }
                  },
                  "next_1_hours": {
                    "summary": {
                      "symbol_code": "partlycloudy_day"
                    }
                  }
                }
              }
            ]
          }
        }
        """;

        var handler = new FakeHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.met.no/")
        };

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new WeatherService(httpClient, cache, NullLogger<WeatherService>.Instance);

        var first = await service.GetCurrentAsync();
        var second = await service.GetCurrentAsync();

        Assert.NotNull(first);
        Assert.Equal(12.5m, first!.AirTemperatureC);
        Assert.Equal(4.3m, first.WindSpeedMs);
        Assert.Equal("E", first.WindDirection);
        Assert.Equal("partlycloudy_day", first.WeatherSymbol);
        Assert.NotNull(second);
        Assert.Equal(1, handler.CallCount);
    }

    private sealed class FakeHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory = responseFactory;

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_responseFactory(request));
        }
    }
}
