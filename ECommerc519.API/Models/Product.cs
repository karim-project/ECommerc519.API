namespace ECommerc519.API.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discont { get; set; }
        public bool Status { get; set; }
        public int Quantity { get; set; }
        public double Rate { get; set; }
        public string MainImg { get; set; } = string.Empty;


        public int CategoryId { get; set; }
        public Category category { get; set; } = null!;

        public int BrandId   { get; set; }
        public Brand brand { get; set; }=null!;

        public List<ProductColor> ProductColors { get; set; }
        public List<ProductSubImg> ProductSubImgs { get; set; }
    }
}
