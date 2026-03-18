namespace Laks.Web.Models;

public class SeasonDay
{
    public bool IsOffSeason { get; set; }
    public int? GroupNumber { get; set; }
    public int? DayInGroup { get; set; }
    public int GroupLengthDays { get; set; }
    public DateTime? NextSeasonStart { get; set; }

    public string DisplayText => IsOffSeason
        ? (NextSeasonStart.HasValue ? $"Season starts {NextSeasonStart.Value:dd MMM}" : "Season not configured")
        : $"Day {DayInGroup} of {GroupLengthDays} · Group {GroupNumber}";
}
