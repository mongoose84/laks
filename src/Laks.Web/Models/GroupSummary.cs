namespace Laks.Web.Models;

public class GroupSummary
{
    public int Year { get; set; }
    public int GroupNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int FishCount { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal BestWeightKg { get; set; }

    public string GroupId => $"{Year}-{GroupNumber}";
    public string Label => $"{StartDate:dd MMM}-{EndDate:dd MMM}";
}
