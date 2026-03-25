namespace Laks.Web.Models;

public class SeasonConfig
{
    public int Year { get; set; }
    public int GroupNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string GroupId => $"{Year}-{GroupNumber}";
}
