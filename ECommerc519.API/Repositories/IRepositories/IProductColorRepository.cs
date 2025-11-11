using Microsoft.EntityFrameworkCore;

namespace ECommerc519.API.Repositories.IRepositories
{
    public interface IProductColorRepository : IRepository<ProductColor>
    {
         void RemoveRange(IEnumerable<ProductColor> productColors);
    }
}
