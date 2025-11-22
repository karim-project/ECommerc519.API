namespace ECommerc519.API.DTOs.Response
{
    public class FillProductResponse
    {
       
            public string? Name { get; set; }
            public decimal? MainPrice { get; set; }
            public decimal? MaxPrice { get; set; }
            public int? CategoryId { get; set; }
            public int? Brandid { get; set; }
            public bool LessQuantity { get; set; }
            public bool IsHot { get; set; }
        

    }
}
