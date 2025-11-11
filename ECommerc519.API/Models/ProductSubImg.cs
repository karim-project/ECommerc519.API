namespace ECommerc519.API.Models
{
    public class ProductSubImg
    {
        public int ProductId { get; set; }
        public Product product { get;  } = null!;
        public string Img { get; set; } = string.Empty;
    }
}
