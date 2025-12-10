using System.ComponentModel.DataAnnotations;

namespace BookStoreAPI.Models
{
    public class BookSubImages
    {
        public int Id { get; set; }

        [Required]
        public string Img { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }
    }
}
