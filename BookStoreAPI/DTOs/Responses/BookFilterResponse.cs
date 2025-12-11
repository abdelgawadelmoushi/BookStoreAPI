namespace BookStoreAPI.DTOs.Responses
{
    public class BookFilterResponse
    {
        public string? BookName { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool LessQuantity { get; set; }
        public bool Status { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? AuthorId { get; set; }
        public int CurrentPage { get; set; }
        public DateTime? CreatedAt { get; set; }

        public double TotalNumberOfPages { get; set; }
    }
}
