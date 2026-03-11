namespace Laks.Web.Models;

public class Trip
{
    public int Id { get; set; }
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string RiverName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalCatches { get; set; }
}
