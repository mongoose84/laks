using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Laks.Web.Pages.Catches;

public class IndexModel : PageModel
{
    private readonly ICatchRepository _catches;
    private readonly ITripRepository _trips;
    private readonly ILogger<IndexModel> _logger;

    public IEnumerable<Catch> CatchList { get; private set; } = [];
    public IEnumerable<Trip> Trips { get; private set; } = [];
    public int? SelectedTripId { get; private set; }
    public Trip? SelectedTrip { get; private set; }

    public IndexModel(ICatchRepository catches, ITripRepository trips, ILogger<IndexModel> logger)
    {
        _catches = catches;
        _trips   = trips;
        _logger  = logger;
    }

    public async Task OnGetAsync(int? tripId)
    {
        try
        {
            SelectedTripId = tripId;
            Trips          = await _trips.GetAllAsync();

            if (tripId.HasValue)
            {
                SelectedTrip = Trips.FirstOrDefault(t => t.Id == tripId.Value);
                CatchList    = await _catches.GetByTripAsync(tripId.Value);
            }
            else
            {
                CatchList = await _catches.GetRecentAsync(100);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load catches page (tripId={TripId})", tripId);
        }
    }
}
