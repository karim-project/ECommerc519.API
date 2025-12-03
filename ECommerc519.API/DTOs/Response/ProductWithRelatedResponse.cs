namespace ECommerc519.API.DTOs.Response
{
    public class ProductWithRelatedResponse
    {
        public Product Product { get; set; } = default!;

        public List<Product> Relatedproducts { get; set; } = [];
    }
}
