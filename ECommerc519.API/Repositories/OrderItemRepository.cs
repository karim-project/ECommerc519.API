
using ECommerc519.API.Models;
using System.Threading.Tasks;

namespace ECommerc519.API.Repositories
{
    public class OrderItemRepository : Repository<OrderItem> , IOrderItemRepository
    {
        private ApplicationDBContext _context;// = new();

        public OrderItemRepository(ApplicationDBContext context) :base(context) 
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<OrderItem> orderItems , CancellationToken cancellationToken = default)
        {
            await _context.AddRangeAsync(orderItems , cancellationToken);

        }
    }
}
