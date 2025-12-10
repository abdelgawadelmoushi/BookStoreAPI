using BookStoreAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookStoreAPI.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null ?
                   await _dbSet.CountAsync() :
                   await _dbSet.CountAsync(predicate);
        }

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (expression != null)
                query = query.Where(expression);

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            if (!tracked)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default)
        {
            return (await GetAsync(expression, includes, tracked, cancellationToken))
                   .FirstOrDefault();
        }

        public async Task AddRangeAsync(List<BookSubImages> listOfNewBookSubImages)
        {
            await _context.AddRangeAsync(listOfNewBookSubImages);
        }

        public void RemoveRange(List<BookSubImages> listOfBookSubImages)
        {
            _context.RemoveRange(listOfBookSubImages);
        }
    }
}
