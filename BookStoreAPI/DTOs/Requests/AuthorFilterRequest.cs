namespace BookStoreAPI.DTOs.Requests
{
    public record AuthorFilterRequest(string? Name,  bool status, int? Age, string Img , List<string> Skills, ICollection<AuthorCategory> AuthorCategories,  int page = 1);
    public record AuthorFilterInputRequest(string? Name, bool status, int? Age, int page = 1);
}
