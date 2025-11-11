using System.ComponentModel.DataAnnotations;

namespace ECommerc519.API.Models
{
    public class Brand
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Img { get; set; } = "defultImg.jpeg";
        
        public string? Description { get; set; }
        public bool Status { get; set; }
    }
}
