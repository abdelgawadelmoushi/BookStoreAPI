namespace BookStoreAPI.DTOs.Responses
{
    public class UserFilterResponse
    {
        public string UserName { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public double TotalNumberOfPages { get; set; }
    }
}
