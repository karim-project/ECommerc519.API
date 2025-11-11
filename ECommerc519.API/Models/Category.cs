using ECommerc519.API.Validation;
using System.ComponentModel.DataAnnotations;

namespace ECommerc519.API.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        //[MinLength(3)]
        //[MaxLength(100)]
        [CustomLength(5,100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Description { get; set; }
        public bool Status { get; set; }



    }
}
