namespace BookStoreAPI.DTOs.Requests
{
    public class AuthorFilterResponse
    {


        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Img { get; set; }
        public List<string> Skills { get; set; } = new List<string>();

        public ICollection<AuthorCategory> AuthorCategories { get; set; } = new List<AuthorCategory>();
        public DateTime? CreatedAt { get; set; }

        public double TotalNumberOfPages { get; set; }

        public int CurrentPage { get; set; }




    }
}