namespace BookStoreAPI.Models
{
    public class Book
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MainImg { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } 
        public DateTime Datetime { get; set; }
        public decimal Discount { get; set; }
        public int Quantity { get; set; }
        public double Rate { get; set; }

        public bool Status { get; set; } = true;
        public List<BookSubImages> BookSubImages { get; set; } = [];

        public List<Author> Authors { get; set; } = [];

        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        
     

        public ICollection<BookRating> Ratings { get; set; } = new List<BookRating>();

    }
}

