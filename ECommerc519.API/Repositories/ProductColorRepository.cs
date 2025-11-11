

namespace ECommerc519.API.Repositories
{
    public class ProductColorRepository : Repository<ProductColor> , IProductColorRepository
    {
        private ApplicationDBContext _context;// = new();

        public ProductColorRepository(ApplicationDBContext context) :base(context) 
        {
            _context = context;
        }

        public void RemoveRange(IEnumerable<ProductColor> productColors)
        {
             _context.RemoveRange(productColors);

        }
    }
}
