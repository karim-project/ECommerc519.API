using System.ComponentModel.DataAnnotations;

namespace ECommerc519.API.DTOs.Request
{
    public class UpdateBrandRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public IFormFile? NewImg { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        public bool Status { get; set; }
    }
}
