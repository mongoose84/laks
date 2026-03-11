using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Laks.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ITripRepository _trips;
    private readonly ICatchRepository _catches;
    private readonly ILogger<IndexModel> _logger;

    public Trip? LatestTrip { get; private set; }
    public IEnumerable<Catch> RecentCatches { get; private set; } = [];
    public int TotalCatches { get; private set; }

    public IndexModel(ITripRepository trips, ICatchRepository catches, ILogger<IndexModel> logger)
    {
        _trips = trips;
        _catches = catches;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var tripTask        = _trips.GetLatestAsync();
            var recentTask      = _catches.GetRecentAsync(5);
            var totalCountTask  = _catches.GetTotalCountAsync();

            await Task.WhenAll(tripTask, recentTask, totalCountTask);

            LatestTrip    = await tripTask;
            RecentCatches = await recentTask;
            TotalCatches  = await totalCountTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load home page data");
        }
    }
}
