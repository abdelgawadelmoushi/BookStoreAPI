using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace BookStoreAPI.Areas.Customer
{
    [Route("api/customer/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Customer")]
    public class FavouritesController : ControllerBase
    {



        private readonly IRepository<Favourite> _favouriteRepository;
        private readonly IRepository<Book> _bookRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavouritesController(IRepository<Book> bookRepository, IRepository<Cart> cartRepository, 
            IRepository<Promotion> promotionRepository, UserManager<ApplicationUser> userManager
            , IRepository<Favourite> favouriteRepository)
        {
            _bookRepository = bookRepository;
            _cartRepository = cartRepository;
            _promotionRepository = promotionRepository;
            _userManager = userManager;
            _favouriteRepository = favouriteRepository;
        }


        [HttpPut("AddToFavouite")]
        public async Task<IActionResult> AddToFavouite(int bookId, int count)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var book = await _bookRepository.GetOneAsync(e => e.Id == bookId);

            if (book is null)
                return NotFound();

            var favoutiteInDb = await _favouriteRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.BookId == bookId);

            if (favoutiteInDb is not null)
            {
                favoutiteInDb.Count += count;
            }
            else
            {
                await _favouriteRepository.CreateAsync(new()
                {
                    ApplicationUserId = user.Id,
                    BookId = bookId,
                    Count = count,
                    BookPrice = book.Price - (book.Price * (book.Discount / 100m))
                });
            }

            await _favouriteRepository.CommitAsync();

            return NoContent();
        }


        [HttpPost("Get")]
        public async Task<IActionResult> Get(BookFilterRequest booksFilterRequest)
        {
            var bookss = await _favouriteRepository.GetAsync(tracked: false);

            BookFilterResponse booksFilterResponse = new();

            // Add Filter
            if (booksFilterRequest.booksName is not null)
            {
                var booksNameTrimmed = booksFilterRequest.booksName.Trim();

                bookss = bookss.Where(e => e.Book.Name.Contains(booksNameTrimmed));
                booksFilterResponse.BookName = booksFilterRequest.booksName;
            }

            if (booksFilterRequest.status)
            {
                bookss = bookss.Where(e => e.Book.Status);
                booksFilterResponse.Status = booksFilterRequest.status;
            }
            if (booksFilterRequest.CreatedAt != null)
            {
                bookss = bookss.Where(e => e.Book.CreatedAt.Date == booksFilterRequest.CreatedAt.Value.Date);
            }

            if (booksFilterRequest.categoryId is not null)
            {
                bookss = bookss.Where(e => e.Book.CategoryId == booksFilterRequest.categoryId);
                booksFilterResponse.CategoryId = booksFilterRequest.categoryId;
            }



            // Add Pagination
            var totalNumberOfPages = Math.Ceiling(bookss.Count() / 8.0);
            booksFilterResponse.TotalNumberOfPages = totalNumberOfPages;
            booksFilterResponse.CurrentPage = booksFilterRequest.page;

            bookss = bookss.Skip((booksFilterRequest.page - 1) * 8).Take(8);

            return Ok(new
            {
                Books = bookss,
                BookFilter = booksFilterResponse
            });
        }

        [HttpDelete("DeleteItem")]

        public async Task<IActionResult> DeleteItem(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var favouriteInDb = await _favouriteRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.BookId == bookId);

            if (favouriteInDb is null)
                return NotFound();

            _favouriteRepository.Delete(favouriteInDb);
            await _favouriteRepository.CommitAsync();

            return NoContent();

        }


    }
}
