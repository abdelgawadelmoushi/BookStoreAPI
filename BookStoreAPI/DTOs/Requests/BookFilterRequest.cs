namespace BookStoreAPI.DTOs.Requests
{
    public record BookFilterRequest(string? booksName, decimal? minPrice, decimal? maxPrice, int? AuthorId, bool lessQuantity, bool status, int? categoryId, int? brandId,DateTime? CreatedAt, int page = 1);
}
