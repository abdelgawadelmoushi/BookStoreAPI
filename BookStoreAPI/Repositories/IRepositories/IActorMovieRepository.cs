using NuGet.Protocol.Core.Types;
using System.Linq.Expressions;

namespace BookStoreAPI.Repositories.IRepositories
{
    public interface IAuthorBookRepository : IRepository<AuthorBook>
    {

        Task AddRangeAsync(IEnumerable<AuthorBook> items, CancellationToken cancellationToken = default);
        void RemoveRange(IEnumerable<AuthorBook> items);
    }
}
