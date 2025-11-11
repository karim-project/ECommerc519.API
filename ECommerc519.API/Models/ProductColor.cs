namespace ECommerc519.API.Models
{
    public class ProductColor
    {
        public int ProductId { get; set; }
        public Product product { get;  } = null!;
        public string Color { get; set; } = string.Empty;
    }
}
