using Microsoft.EntityFrameworkCore;

namespace ECommerc519.API.Repositories.IRepositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default);
    }
}
