namespace Laks.Web.Models;

public class SeasonDay
{
    public bool IsOffSeason { get; set; }
    public bool IsBufferDay { get; set; }
    public int? GroupNumber { get; set; }
    public int? DayInGroup { get; set; }
    public int GroupLengthDays { get; set; }
    public DateTime? NextGroupStart { get; set; }
    public string? NextGroupTeamName { get; set; }
    public int TotalGroups { get; set; }

    public string DisplayText
    {
        get
        {
            if (!IsOffSeason && !IsBufferDay)
                return $"Dag {DayInGroup} af {GroupLengthDays} · Hold {GroupNumber}";

            if (NextGroupStart.HasValue)
            {
                var days = (NextGroupStart.Value.Date - DateTime.UtcNow.Date).Days;
                var teamSuffix = !string.IsNullOrWhiteSpace(NextGroupTeamName)
                    ? $" · {NextGroupTeamName}"
                    : "";
                return days switch
                {
                    <= 0 => $"Fiskeriet starter i dag{teamSuffix}",
                    1 => $"Fiskeriet starter i morgen{teamSuffix}",
                    _ => $"Fiskeriet starter om {days} dage{teamSuffix}"
                };
            }

            return "Sæson ikke konfigureret";
        }
    }
}
