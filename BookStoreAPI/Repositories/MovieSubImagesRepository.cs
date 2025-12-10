using BookStoreAPI.Models;
using BookStoreAPI.Repositories;
using BookStoreAPI.Repositories.IRepositories;

public class BookSubImagesRepository : Repository<BookSubImages>, IBookSubImagesRepository
{
    protected readonly ApplicationDbContext _context;

    public BookSubImagesRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void RemoveRange(IEnumerable<BookSubImages> items)
    {
        _context.BookSubImages.RemoveRange(items);
    }

    public async Task AddRangeAsync(IEnumerable<BookSubImages> items, CancellationToken cancellationToken = default)
    {
        await _context.BookSubImages.AddRangeAsync(items, cancellationToken);
    }
}
