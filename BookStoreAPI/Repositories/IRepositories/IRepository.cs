using System.Linq.Expressions;

namespace BookStoreAPI.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task CreateAsync(T entity, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetAsync(
                Expression<Func<T, bool>>? expression = null,
                Expression<Func<T, object>>[]? includes = null,
                bool tracked = true,
                CancellationToken cancellationToken = default);

        Task<T?> GetOneAsync(
                Expression<Func<T, bool>>? expression = null,
                Expression<Func<T, object>>[]? includes = null,
                bool tracked = true,
                CancellationToken cancellationToken = default);

        void Update(T entity);
        void Delete(T entity);

        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        Task CommitAsync(CancellationToken cancellationToken = default);

        Task AddRangeAsync(List<BookSubImages> listOfNewBookSubImages);

        void RemoveRange(List<BookSubImages> listOfBookSubImages);
    }
}
