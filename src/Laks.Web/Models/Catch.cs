namespace Laks.Web.Models;

public class Catch
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int AnglerId { get; set; }
    public int SpeciesId { get; set; }
    public DateTime CatchDate { get; set; }
    public decimal WeightKg { get; set; }
    public decimal LengthCm { get; set; }
    public bool Released { get; set; }
    public string? Notes { get; set; }

    // Navigation / joined fields
    public string? AnglerName { get; set; }
    public string? SpeciesName { get; set; }
    public int TripYear { get; set; }
    public string? RiverName { get; set; }
}
