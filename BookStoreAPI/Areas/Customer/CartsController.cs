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
    public class CartsController : ControllerBase
    {



        private readonly IRepository<Book> _bookRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartsController(IRepository<Book> bookRepository, IRepository<Cart> cartRepository, IRepository<Promotion> promotionRepository, UserManager<ApplicationUser> userManager)
        {
            _bookRepository = bookRepository;
            _cartRepository = cartRepository;
            _promotionRepository = promotionRepository;
            _userManager = userManager;
        }


        [HttpPut("AddToCart")]
        public async Task<IActionResult> AddToCart(int bookId, int count)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var book = await _bookRepository.GetOneAsync(e => e.Id == bookId);

            if (book is null)
                return NotFound();

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.BookId == bookId);

            if (cartInDb is not null)
            {
                cartInDb.Count += count;
            }
            else
            {
                await _cartRepository.CreateAsync(new()
                {
                    ApplicationUserId = user.Id,
                    BookId = bookId,
                    Count = count,
                    BookPrice = book.Price - (book.Price * (book.Discount / 100m))
                });
            }

            await _cartRepository.CommitAsync();

            return NoContent();
        }

           [HttpGet("")]
        public async Task<IActionResult> Get(string code)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cartInDb = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Book]);

            if (code is not null)
            {
                var promotion = await _promotionRepository.GetOneAsync(e => e.Code == code && e.IsValid && e.ValidTo > DateTime.UtcNow && e.MaxUsage > 0);

                if (promotion is null)
                return BadRequest(new ErrorModel
                {
                    Code= "Invalid Code",
                    Message= "Invalid Code",

                });
                else
                {
                    bool founded = false;

                    foreach (var item in cartInDb)
                    {
                        if (item.BookId == promotion.BookId)
                        {
                            item.BookPrice = item.Book.Price - (item.Book.Price * (promotion.Discount / 100m));
                            promotion.MaxUsage -= 1;
                            await _cartRepository.CommitAsync();
                            return Ok(new SuccessModel
                            {
                                Message = "Apply Code Successfully",
                            });
                            founded = true;
                            break;
                        }
                    }

                    if (!founded)
                        return BadRequest(new ErrorModel
                        {
                            Code = "Invalid Code",
                            Message = "Invalid Code",

                        });
                }
            }

            return Ok(cartInDb);
        }

        [HttpGet("IncrementCount")]
        public async Task<IActionResult> IncrementCount(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.BookId == bookId);

            if (cartInDb is null)
                return NotFound();

            cartInDb.Count += 1;
            await _cartRepository.CommitAsync();

            return NoContent();
        }
        [HttpGet("DecrementCount")]
        public async Task<IActionResult> DecrementCount(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.BookId == bookId);

            if (cartInDb is null)
                return NotFound();

            if (cartInDb.Count > 1)
            {
                cartInDb.Count -= 1;
                await _cartRepository.CommitAsync();
            }

            return NoContent();
        }

        [HttpDelete("DeleteItem")]

        public async Task<IActionResult> DeleteItem(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.BookId == bookId);

            if (cartInDb is null)
                return NotFound();

            _cartRepository.Delete(cartInDb);
            await _cartRepository.CommitAsync();

            return NoContent();

        }

        [HttpGet("Pay")]
        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cartInDb = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Book]);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/customer/checkout/success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/customer/checkout/cancel",
            };

            foreach (var item in cartInDb)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Book.Name,
                            Description = item.Book.Description,
                        },
                        UnitAmount = (long)item.BookPrice * 100,
                    },
                    Quantity = item.Count,
                });
            }

            var service = new SessionService();
            var session = service.Create(options);
            return Ok(new { url = session.Url });
        }



    }
}
