namespace Laks.Web.Models;

public class SeasonDay
{
    public bool IsOffSeason { get; set; }
    public bool IsBufferDay { get; set; }
    public int? GroupNumber { get; set; }
    public int? DayInGroup { get; set; }
    public int GroupLengthDays { get; set; }
    public DateTime? NextGroupStart { get; set; }
    public int TotalGroups { get; set; }

    public string DisplayText
    {
        get
        {
            if (!IsOffSeason && !IsBufferDay)
                return $"Day {DayInGroup} of {GroupLengthDays} · Group {GroupNumber}";

            if (NextGroupStart.HasValue)
            {
                var days = (NextGroupStart.Value.Date - DateTime.UtcNow.Date).Days;
                return days switch
                {
                    <= 0 => "Fishing starts today",
                    1 => "Fishing starts tomorrow",
                    _ => $"Fishing starts in {days} days"
                };
            }

            return "Season not configured";
        }
    }
}
