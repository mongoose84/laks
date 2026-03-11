using Laks.Web.Models;

namespace Laks.Web.Data.Repositories;

public interface ICatchRepository
{
    Task<IEnumerable<Catch>> GetRecentAsync(int count = 20);
    Task<IEnumerable<Catch>> GetByYearAsync(int year);
    Task<IEnumerable<Catch>> GetByAnglerAsync(int anglerId);
    Task<int> GetTotalCountAsync();

    // Chart data
    Task<IEnumerable<CatchesPerYear>> GetCatchesPerYearAsync();
    Task<IEnumerable<CatchesPerAngler>> GetCatchesPerAnglerAsync(int? year = null);
    Task<IEnumerable<CatchesByType>> GetCatchesByTypeAsync(int? year = null);
}
