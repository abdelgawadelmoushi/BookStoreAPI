using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreAPI.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Img { get; set; }
        [NotMapped]
        public IFormFile? ImgFile { get; set; }
    }
}
