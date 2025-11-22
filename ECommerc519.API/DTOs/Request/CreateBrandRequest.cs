using System.ComponentModel.DataAnnotations;

namespace ECommerc519.API.DTOs.Request
{
    public class CreateBrandRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public IFormFile Img { get; set; } = default!;
        [MaxLength(1000)]
        public string? Description { get; set; }
        public bool Status { get; set; }
    }
}
