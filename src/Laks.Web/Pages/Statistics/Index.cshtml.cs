using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Laks.Web.Pages.Statistics;

public class IndexModel : PageModel
{
    private readonly ICatchRepository _catches;
    private readonly ISeasonRepository _seasons;
    private readonly ILogger<IndexModel> _logger;

    // Chart data as JSON strings (safe for embedding in script tags)
    public string TrendLabelsJson { get; private set; } = "[]";
    public string TrendTotalsJson { get; private set; } = "[]";
    public string TrendWeightJson { get; private set; } = "[]";

    public string BarLabelsJson  { get; private set; } = "[]";
    public string BarCatchesJson { get; private set; } = "[]";
    public string BarWeightJson  { get; private set; } = "[]";

    public string PieLabelsJson { get; private set; } = "[]";
    public string PieDataJson   { get; private set; } = "[]";

    public string TeamLabelsJson      { get; private set; } = "[]";
    public string TeamBiggestJson     { get; private set; } = "[]";
    public string TeamAvgWeightJson { get; private set; } = "[]";
    public IEnumerable<BiggestSalmonPerTeam> TeamStats { get; private set; } = [];

    public IEnumerable<FishingSeason> Seasons { get; private set; } = [];
    public int? SelectedYear { get; private set; }

    public IndexModel(ICatchRepository catches, ISeasonRepository seasons, ILogger<IndexModel> logger)
    {
        _catches = catches;
        _seasons = seasons;
        _logger  = logger;
    }

    public async Task OnGetAsync(int? year)
    {
        SelectedYear = year;

        try
        {
            Seasons = await _seasons.GetAllAsync();

            var trendTask   = _catches.GetCatchesPerYearAsync();
            var anglerTask  = _catches.GetCatchesPerAnglerAsync(year);
            var typeTask    = _catches.GetCatchesByTypeAsync(year);
            var teamTask    = _catches.GetBiggestSalmonPerTeamAsync(year);

            await Task.WhenAll(trendTask, anglerTask, typeTask, teamTask);

            // Trend line – all years
            var trend = (await trendTask).ToList();
            TrendLabelsJson = Serialize(trend.Select(x => x.Year.ToString()));
            TrendTotalsJson = Serialize(trend.Select(x => x.TotalCatches));
            TrendWeightJson = Serialize(trend.Select(x => x.TotalWeightKg));

            // Bar – per angler (optionally filtered by year)
            var anglers = (await anglerTask).ToList();
            BarLabelsJson  = Serialize(anglers.Select(x => x.AnglerName));
            BarCatchesJson = Serialize(anglers.Select(x => x.TotalCatches));
            BarWeightJson  = Serialize(anglers.Select(x => x.TotalWeightKg));

            // Pie – catch type distribution
            var types = (await typeTask).ToList();
            PieLabelsJson = Serialize(types.Select(x => x.TypeName));
            PieDataJson   = Serialize(types.Select(x => x.TotalCatches));

            // Team – biggest salmons per team
            var teams = (await teamTask).ToList();
            TeamStats          = teams;
            TeamLabelsJson     = Serialize(teams.Select(x => x.TeamName));
            TeamBiggestJson    = Serialize(teams.Select(x => x.BiggestSalmonKg));
            TeamAvgWeightJson = Serialize(teams.Select(x => x.AvgSalmonWeightKg));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load statistics page (year={Year})", year);
        }
    }

    private static string Serialize<T>(IEnumerable<T> data) =>
        JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false });
}
