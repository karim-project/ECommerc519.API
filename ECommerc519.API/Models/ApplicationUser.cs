using Microsoft.AspNetCore.Identity;

namespace ECommerc519.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? Address { get; set; }
    }
}
