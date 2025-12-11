using System.Text.Json.Serialization;

namespace BookStoreAPI.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Img { get; set; } = string.Empty;

        public List<string> Skills { get; set; } = new List<string>();

     

        public ICollection<AuthorCategory> AuthorCategories { get; set; } = new List<AuthorCategory>();
    }
}
