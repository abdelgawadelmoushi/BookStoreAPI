using System.Text.Json.Serialization;

namespace BookStoreAPI.Models
{
    public class BookRating
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public byte Value { get; set; }

        [JsonIgnore]  
        public Book Book { get; set; }

        public string ApplicationUserId { get; set; }

        [JsonIgnore] 
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
