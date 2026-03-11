using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Laks.Web.Pages.Statistics;

public class IndexModel : PageModel
{
    private readonly ICatchRepository _catches;
    private readonly ITripRepository _trips;
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

    public IEnumerable<Trip> Trips { get; private set; } = [];
    public int? SelectedYear { get; private set; }

    public IndexModel(ICatchRepository catches, ITripRepository trips, ILogger<IndexModel> logger)
    {
        _catches = catches;
        _trips   = trips;
        _logger  = logger;
    }

    public async Task OnGetAsync(int? year)
    {
        SelectedYear = year;

        try
        {
            Trips = await _trips.GetAllAsync();

            var trendTask   = _catches.GetCatchesPerYearAsync();
            var anglerTask  = _catches.GetCatchesPerAnglerAsync(year);
            var speciesTask = _catches.GetCatchesBySpeciesAsync(year);

            await Task.WhenAll(trendTask, anglerTask, speciesTask);

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

            // Pie – species distribution
            var species = (await speciesTask).ToList();
            PieLabelsJson = Serialize(species.Select(x => x.SpeciesName));
            PieDataJson   = Serialize(species.Select(x => x.TotalCatches));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load statistics page (year={Year})", year);
        }
    }

    private static string Serialize<T>(IEnumerable<T> data) =>
        JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false });
}
