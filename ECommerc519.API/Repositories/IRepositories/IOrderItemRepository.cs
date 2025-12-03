using Microsoft.EntityFrameworkCore;

namespace ECommerc519.API.Repositories.IRepositories
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
         Task AddRangeAsync(IEnumerable<OrderItem> orderItems,CancellationToken cancellationToken =default );
    }
}
