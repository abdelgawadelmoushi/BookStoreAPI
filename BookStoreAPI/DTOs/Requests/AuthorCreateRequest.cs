namespace BookStoreAPI.DTOs.Requests
{
    public class AuthorCreateRequest
    {


        public string? Name { get; set; }
        public int Age { get; set; }
        public IFormFile? Img { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public ICollection<AuthorBook> Authorbooks { get; set; } = new List<AuthorBook>();

        public ICollection<AuthorCategory> AuthorCategories { get; set; } = new List<AuthorCategory>();
        public DateTime? CreatedAt { get; set; }

        public double TotalNumberOfPages { get; set; }

        public int CurrentPage { get; set; }




    }
}