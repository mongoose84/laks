namespace Laks.Web.Models;

public class CatchLocation
{
    public int CatchId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal WeightKg { get; set; }
    public string AnglerName { get; set; } = string.Empty;
    public int AnglerId { get; set; }
    public string CatchType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Bait { get; set; } = string.Empty;
    public DateTime CatchDate { get; set; }
    public int SeasonYear { get; set; }
}
