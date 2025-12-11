namespace BookStoreAPI.Models
{
    public class AuthorCategory
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; } 
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
