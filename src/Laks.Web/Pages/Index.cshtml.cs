using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Laks.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ISeasonRepository _seasons;
    private readonly ICatchRepository _catches;
    private readonly ILogger<IndexModel> _logger;

    public FishingSeason? LatestSeason { get; private set; }
    public IEnumerable<Catch> RecentCatches { get; private set; } = [];
    public int TotalCatches { get; private set; }

    public IndexModel(ISeasonRepository seasons, ICatchRepository catches, ILogger<IndexModel> logger)
    {
        _seasons = seasons;
        _catches = catches;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var seasonTask      = _seasons.GetLatestAsync();
            var recentTask      = _catches.GetRecentAsync(5);
            var totalCountTask  = _catches.GetTotalCountAsync();

            await Task.WhenAll(seasonTask, recentTask, totalCountTask);

            LatestSeason  = await seasonTask;
            RecentCatches = await recentTask;
            TotalCatches  = await totalCountTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load home page data");
        }
    }
}
