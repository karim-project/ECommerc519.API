namespace ECommerc519.API.DTOs.Request
{
    public record FillProductReaquest(string name, decimal? MainPrice, decimal? MaxPrice, int? categoryId, int? brandid, bool LessQuantity, bool isHot);
}
    

