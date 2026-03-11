namespace Laks.Web.Models;

public class FishingSeason
{
    public int Year { get; set; }
    public DateTime? FirstCatchDate { get; set; }
    public DateTime? LastCatchDate { get; set; }
    public int TotalCatches { get; set; }
    public int ParticipantCount { get; set; }
}