using Laks.Web.Models;

namespace Laks.Web.Data.Repositories;

public interface ISeasonRepository
{
    Task<IEnumerable<FishingSeason>> GetAllAsync();
    Task<FishingSeason?> GetByYearAsync(int year);
    Task<FishingSeason?> GetLatestAsync();
    Task<IEnumerable<SeasonConfig>> GetSeasonConfigAsync(int year);
}