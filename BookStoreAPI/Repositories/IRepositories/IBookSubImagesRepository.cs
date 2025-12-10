using BookStoreAPI.Models;

namespace BookStoreAPI.Repositories.IRepositories
{
    public interface IBookSubImagesRepository : IRepository<BookSubImages>
    {
        void RemoveRange(IEnumerable<BookSubImages> items);
        Task AddRangeAsync(IEnumerable<BookSubImages> items, CancellationToken cancellationToken = default);
    }
}
