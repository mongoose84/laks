using System.Net;
using System.Text;
using Laks.Web.Models;
using Laks.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laks.Web.Tests.Unit;

public class WaterLevelServiceTests
{
        [Fact]
        public async Task GetLast24HoursAsync_ParsesAndCachesObservationSeries()
        {
                const string payload = """
                {
                    "data": [
                        {
                            "observations": [
                                { "time": "2026-03-25T08:00:00Z", "value": 1.24 },
                                { "time": "2026-03-25T09:00:00Z", "value": 1.28 },
                                { "time": "2026-03-25T10:00:00Z", "value": 1.31 }
                            ]
                        }
                    ]
                }
                """;

                var handler = new FakeHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
                {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });

                var service = CreateService(handler);

                var first = await service.GetLast24HoursAsync();
                var second = await service.GetLast24HoursAsync();

                Assert.Collection(
                        first,
                        reading =>
                        {
                                Assert.Equal(new DateTime(2026, 3, 25, 8, 0, 0, DateTimeKind.Utc), reading.Time);
                                Assert.Equal(1.24m, reading.LevelMeters);
                        },
                        reading =>
                        {
                                Assert.Equal(new DateTime(2026, 3, 25, 9, 0, 0, DateTimeKind.Utc), reading.Time);
                                Assert.Equal(1.28m, reading.LevelMeters);
                        },
                        reading =>
                        {
                                Assert.Equal(new DateTime(2026, 3, 25, 10, 0, 0, DateTimeKind.Utc), reading.Time);
                                Assert.Equal(1.31m, reading.LevelMeters);
                        });
                Assert.Equal(first.Count, second.Count);
                Assert.Equal(1, handler.CallCount);
        }

        [Fact]
        public async Task GetCurrentAsync_UsesSeriesForLevelTrendAndTemperature()
        {
                const string levelPayload = """
                {
                    "data": [
                        {
                            "observations": [
                                { "time": "2026-03-25T07:00:00Z", "value": 1.10 },
                                { "time": "2026-03-25T08:00:00Z", "value": 1.16 },
                                { "time": "2026-03-25T09:00:00Z", "value": 1.21 },
                                { "time": "2026-03-25T10:00:00Z", "value": 1.24 }
                            ]
                        }
                    ]
                }
                """;

                const string temperaturePayload = """
                {
                    "data": [
                        {
                            "observations": [
                                { "time": "2026-03-25T10:00:00Z", "value": 5.4 }
                            ]
                        }
                    ]
                }
                """;

                var handler = new FakeHttpHandler(request =>
                {
                        var payload = request.RequestUri!.Query.Contains("Parameter=1003", StringComparison.Ordinal)
                                ? temperaturePayload
                                : levelPayload;

                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                                Content = new StringContent(payload, Encoding.UTF8, "application/json")
                        };
                });

                var service = CreateService(handler);

                var snapshot = await service.GetCurrentAsync();

                Assert.NotNull(snapshot);
                Assert.Equal(1.24m, snapshot!.LevelMeters);
                Assert.Equal(5.4m, snapshot.WaterTemperatureC);
                Assert.Equal(WaterLevelTrend.Rising, snapshot.Trend);
                Assert.Equal(new DateTime(2026, 3, 25, 10, 0, 0, DateTimeKind.Utc), snapshot.MeasuredAt);
                Assert.Equal(2, handler.CallCount);
        }

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

    private static WaterLevelService CreateService(FakeHttpHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://hydapi.nve.no/")
        };

        return new WaterLevelService(
            httpClient,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<WaterLevelService>.Instance);
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
