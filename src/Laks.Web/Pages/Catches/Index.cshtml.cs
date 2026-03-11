using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Laks.Web.Pages.Catches;

public class IndexModel : PageModel
{
    private readonly ICatchRepository _catches;
    private readonly ISeasonRepository _seasons;
    private readonly ILogger<IndexModel> _logger;

    public IEnumerable<Catch> CatchList { get; private set; } = [];
    public IEnumerable<FishingSeason> Seasons { get; private set; } = [];
    public int? SelectedYear { get; private set; }
    public FishingSeason? SelectedSeason { get; private set; }

    public IndexModel(ICatchRepository catches, ISeasonRepository seasons, ILogger<IndexModel> logger)
    {
        _catches = catches;
        _seasons = seasons;
        _logger  = logger;
    }

    public async Task OnGetAsync(int? year)
    {
        try
        {
            SelectedYear = year;
            Seasons      = await _seasons.GetAllAsync();

            if (year.HasValue)
            {
                SelectedSeason = Seasons.FirstOrDefault(s => s.Year == year.Value);
                CatchList      = await _catches.GetByYearAsync(year.Value);
            }
            else
            {
                CatchList = await _catches.GetRecentAsync(100);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load catches page (year={Year})", year);
        }
    }
}
