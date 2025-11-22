namespace ECommerc519.API.DTOs.Request
{
    public class UpdateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discont { get; set; }
        public bool Status { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }


        public IFormFile? img { get; set; }
        public List<IFormFile>? subImgs { get; set; }
        public List<string>? Colors { get; set; }
    }
}
