using BookStoreAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace BookStoreAPI.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, CustomLength(5, 30)]
        public string Name { get; set; } = string.Empty;

        [MinLength(5)]
        public string? Description { get; set; }

        public bool Status { get; set; } = true;
        public ICollection<AuthorCategory> AuthorCategories { get; set; } = new List<AuthorCategory>();

    }
}
