using System.Net;
using System.Text.RegularExpressions;
using Laks.Web.Data.Repositories;
using Laks.Web.Services;
using Laks.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Laks.Web.Tests.Integration;

/// <summary>
/// Renders the real Razor Pages against in-memory fakes and asserts on the
/// produced HTML — catches markup, language, and formatting errors that
/// PageModel unit tests cannot see.
/// </summary>
public sealed class LaksWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Laks", "Server=localhost;Database=laks_test;Uid=test;Pwd=test;");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICatchRepository>();
            services.RemoveAll<ISeasonRepository>();
            services.RemoveAll<IWeatherService>();
            services.RemoveAll<IWaterLevelService>();

            services.AddSingleton<ICatchRepository, InMemoryCatchRepository>();
            services.AddSingleton<ISeasonRepository, InMemorySeasonRepository>();
            services.AddSingleton<IWeatherService, StubWeatherService>();
            services.AddSingleton<IWaterLevelService, StubWaterLevelService>();
        });
    }
}

public class PageRenderingTests : IClassFixture<LaksWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PageRenderingTests(LaksWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetHtmlAsync(string url)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsStringAsync();
    }

    // ── Landmarks & document structure ──────────────────────────────

    [Fact]
    public async Task Index_HasExactlyOneMainLandmark()
    {
        var html = await GetHtmlAsync("/");

        var mainCount = Regex.Matches(html, "<main\\b").Count;
        Assert.Equal(1, mainCount);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Catches")]
    [InlineData("/Statistics")]
    [InlineData("/Statistics/Leaderboard")]
    public async Task AllPages_DeclareDanishLanguage(string url)
    {
        var html = await GetHtmlAsync(url);

        Assert.Contains("<html lang=\"da\">", html);
    }

    [Fact]
    public async Task Statistics_SectionTitlesAreRealHeadings()
    {
        var html = await GetHtmlAsync("/Statistics");

        // Section titles must be <h2> elements, not styled <div>s,
        // so screen readers get a heading outline below the <h1>.
        Assert.DoesNotContain("<div class=\"ed-section-title\">", html);
        Assert.Matches("<h2[^>]*class=\"ed-section-title\"", html);
    }

    [Fact]
    public async Task Index_NavigationMarksCurrentPage()
    {
        var html = await GetHtmlAsync("/");

        Assert.Contains("aria-current=\"page\"", html);
    }

    [Fact]
    public async Task Catches_NavigationMarksCurrentPage()
    {
        var html = await GetHtmlAsync("/Catches");

        Assert.Contains("aria-current=\"page\"", html);
    }

    // ── Danish number formatting (comma decimal separator) ──────────

    [Fact]
    public async Task Catches_WeightUsesDanishDecimalComma()
    {
        var html = await GetHtmlAsync("/Catches");

        // Fake data: 8.4 kg catch → must render as "8,40", never "8.40".
        Assert.Contains("8,40", html);
        Assert.DoesNotContain("8.40", html);
    }

    [Fact]
    public async Task Catches_WaterLevelUsesDanishDecimalComma()
    {
        var html = await GetHtmlAsync("/Catches");

        // Fake data: water level 1.234 m → must render as "1,234".
        Assert.Contains("1,234", html);
        Assert.DoesNotContain("1.234", html);
    }

    [Fact]
    public async Task Statistics_TeamTableUsesDanishDecimalComma()
    {
        var html = await GetHtmlAsync("/Statistics");

        // Fake data: biggest salmon 9.5 kg → must render as "9,5".
        Assert.Contains("9,5", html);
        Assert.DoesNotContain(">9.5<", html);
    }

    // ── Conditions strip ─────────────────────────────────────────────

    [Fact]
    public async Task Index_WindShowsUnit()
    {
        var html = await GetHtmlAsync("/");

        Assert.Contains("m/s", html);
    }

    [Fact]
    public async Task Index_ShowsDanishConditionsLabels()
    {
        var html = await GetHtmlAsync("/");

        Assert.Contains("Vandstand", html);
        Assert.Contains("Nedbør", html);
        Assert.Contains("Aktuelle forhold", html);
    }

    // ── Danish-only user-facing text ─────────────────────────────────

    [Fact]
    public async Task Error_ContainsNoEnglishText()
    {
        var html = await GetHtmlAsync("/Error");

        Assert.DoesNotContain("Development Mode", html);
        Assert.DoesNotContain("Swapping to", html);
        Assert.Contains("Der opstod en fejl", html);
    }

    // ── Leaderboard ──────────────────────────────────────────────────

    [Fact]
    public async Task Leaderboard_RendersRankedAnglers()
    {
        var html = await GetHtmlAsync("/Statistics/Leaderboard");

        Assert.Contains("Rangliste", html);
        Assert.Contains("Erik Andersen", html);
        Assert.Contains("18,2 kg", html);
    }

    [Fact]
    public async Task Index_RendersLeaderboardPreviewAndSeasonSummary()
    {
        var html = await GetHtmlAsync("/");

        Assert.Contains("Sæsontavlen", html);
        Assert.Contains("Sæsonen i tal", html);
        Assert.Contains("Alle tiders rekorder", html);
    }

    // ── Fishing-insight statistics modules ───────────────────────────

    [Fact]
    public async Task Statistics_RendersSeasonProgressModule()
    {
        var html = await GetHtmlAsync("/Statistics");

        Assert.Contains("Sæsonens forløb", html);
        Assert.Contains("seasonProgressChart", html);
        Assert.Contains("Uge 25", html);
    }

    [Fact]
    public async Task Statistics_RendersTimeOfDayModule()
    {
        var html = await GetHtmlAsync("/Statistics");

        Assert.Contains("Fangster fordelt på døgnet", html);
        Assert.Contains("hourChart", html);
    }

    [Fact]
    public async Task Statistics_RendersWaterLevelBandModule()
    {
        var html = await GetHtmlAsync("/Statistics");

        Assert.Contains("Fangster fordelt på vandstand", html);
        // Band labels are Danish-formatted ranges from the fake data (1.25–1.50 m).
        Assert.Contains("1,25\\u20131,50 m", html);
    }
}
