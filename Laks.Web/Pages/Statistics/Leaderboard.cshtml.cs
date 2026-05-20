using Laks.Web.Data.Repositories;
using Laks.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Laks.Web.Pages.Statistics;

public class LeaderboardModel : PageModel
{
    private readonly ICatchRepository _catches;
    private readonly ISeasonRepository _seasons;
    private readonly ILogger<LeaderboardModel> _logger;

    public IEnumerable<LeaderboardEntry> Leaderboard { get; private set; } = [];
    public List<SeasonConfig> AvailableGroups { get; private set; } = [];
    public int? SelectedGroup { get; private set; }
    public int CurrentYear { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string LeaderboardScope { get; set; } = "my-group";

    [BindProperty(SupportsGet = true)]
    public int? GroupNumber { get; set; }

    public LeaderboardModel(
        ICatchRepository catches,
        ISeasonRepository seasons,
        ILogger<LeaderboardModel> logger)
    {
        _catches = catches;
        _seasons = seasons;
        _logger = logger;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        CurrentYear = DateTime.UtcNow.Year;

        try
        {
            var seasonConfig = (await _seasons.GetSeasonConfigAsync(CurrentYear)).ToList();
            AvailableGroups = seasonConfig;

            LeaderboardScope = LeaderboardScope?.ToLowerInvariant() switch
            {
                "my-group" => "my-group",
                "all-groups" => "all-groups",
                "last-year" => "last-year",
                _ => "my-group"
            };

            var groups = seasonConfig.Select(c => c.GroupNumber).Distinct().OrderBy(x => x).ToList();
            if (GroupNumber.HasValue && groups.Contains(GroupNumber.Value))
            {
                SelectedGroup = GroupNumber.Value;
            }
            else if (groups.Count > 0)
            {
                SelectedGroup = groups[0];
            }

            var (year, group) = LeaderboardScope switch
            {
                "all-groups" => (CurrentYear, (int?)null),
                "last-year" => (CurrentYear - 1, (int?)null),
                _ => (CurrentYear, SelectedGroup)
            };

            Leaderboard = await _catches.GetLeaderboardAsync(year, group) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved indlæsning af rangliste-side (scope={Scope})", LeaderboardScope);
        }
    }
}
