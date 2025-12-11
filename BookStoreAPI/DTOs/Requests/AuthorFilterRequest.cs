namespace BookStoreAPI.DTOs.Requests
{
    public record AuthorFilterRequest(string? Name,  bool status, int? Age, string Img , List<string> Skills, ICollection<AuthorCategory> AuthorCategories,  int page = 1);
}
