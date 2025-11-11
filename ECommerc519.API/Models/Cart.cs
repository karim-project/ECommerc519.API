namespace ECommerc519.API.Models
{
    public class Cart
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
