using Laks.Web.Models;

namespace Laks.Web.Data.Repositories;

public interface ITripRepository
{
    Task<IEnumerable<Trip>> GetAllAsync();
    Task<Trip?> GetByIdAsync(int id);
    Task<Trip?> GetByYearAsync(int year);
    Task<Trip?> GetLatestAsync();
}
