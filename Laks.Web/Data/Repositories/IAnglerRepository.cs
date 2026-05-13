using Laks.Web.Models;

namespace Laks.Web.Data.Repositories;

public interface IAnglerRepository
{
    Task<IEnumerable<Angler>> GetAllAsync();
    Task<Angler?> GetByIdAsync(int id);
}
