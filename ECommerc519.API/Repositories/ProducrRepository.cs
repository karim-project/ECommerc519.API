

namespace ECommerc519.API.Repositories
{
    public class ProducrRepository : Repository<Product> , IProductRepository
    {
        private ApplicationDBContext _context;// = new();

        public ProducrRepository(ApplicationDBContext context):base(context) {
        
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<Product> products , CancellationToken cancellationToken = default)
        {
            await _context.AddRangeAsync(products, cancellationToken);

        }
    }
}
