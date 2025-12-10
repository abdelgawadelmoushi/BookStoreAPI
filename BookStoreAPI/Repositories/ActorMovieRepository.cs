using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookStoreAPI.Repositories.IRepositories
{
    public class AuthorBookRepository : Repository<AuthorBook> , IAuthorBookRepository
    {
        protected readonly ApplicationDbContext _context;

        public AuthorBookRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void RemoveRange(IEnumerable<AuthorBook> items)
        {
            _context.Authorbooks.RemoveRange(items);
        }

        public async Task AddRangeAsync(IEnumerable<AuthorBook> items, CancellationToken cancellationToken = default)
        {
            await _context.AddAsync(items, cancellationToken);
        }
    }
}
