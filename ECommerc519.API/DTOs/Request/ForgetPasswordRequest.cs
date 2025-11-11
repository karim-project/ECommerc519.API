using System.ComponentModel.DataAnnotations;

namespace ECommerc519.API.DTOs.Request
{
    public class ForgetPasswordRequest
    {
       
        [Required]
        public string UserNameOrEmail { get; set; } = string.Empty;
    }
}
