using BookStoreAPI.DTOs.Requests;
using BookStoreAPI.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreAPI.Areas.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{SD.Super_Admin_Role},{SD.Admin_Role},{SD.Employee_Role}")]
    [Area("Admin")]
    public class BooksController : ControllerBase
    {
        private readonly IRepository<Book> _booksRepository;
        private readonly IBookSubImagesRepository _booksSubImageRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<BookRating> _ratingRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(IRepository<Book> booksRepository,
            IBookSubImagesRepository booksSubImageRepository, IRepository<Category> categoryRepository
           , UserManager<ApplicationUser> userManager, IRepository<BookRating> ratingRepository
          )
        {
            _booksRepository = booksRepository;
            _booksSubImageRepository = booksSubImageRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _ratingRepository = ratingRepository;
        }

        [HttpPost("Get")]
        public async Task<IActionResult> Get(BookFilterRequest booksFilterRequest)
        {
            var bookss = await _booksRepository.GetAsync(tracked: false);

            BookFilterResponse booksFilterResponse = new();

            // Add Filter
            if (booksFilterRequest.booksName is not null)
            {
                var booksNameTrimmed = booksFilterRequest.booksName.Trim();

                bookss = bookss.Where(e => e.Name.Contains(booksNameTrimmed));
                booksFilterResponse.BookName = booksFilterRequest.booksName;
            }

            //if (booksFilterRequest.minPrice is not null)
            //{
            //    bookss = bookss.Where(e => e.Price > booksFilterRequest.minPrice);
            //    booksFilterResponse.MinPrice = booksFilterRequest.minPrice;
            //}

            //if (booksFilterRequest.maxPrice is not null)
            //{
            //    bookss = bookss.Where(e => e.Price < booksFilterRequest.maxPrice);
            //    booksFilterResponse.MaxPrice = booksFilterRequest.maxPrice;
            //}

            //if (booksFilterRequest.lessQuantity)
            //{
            //    bookss = bookss.OrderBy(e => e.Quantity);
            //    booksFilterResponse.LessQuantity = booksFilterRequest.lessQuantity;
            //}

            if (booksFilterRequest.status)
            {
                bookss = bookss.Where(e => e.Status);
                booksFilterResponse.Status = booksFilterRequest.status;
            }

            if (booksFilterRequest.CreatedAt != null)
            {
                bookss = bookss.Where(e => e.CreatedAt.Date == booksFilterRequest.CreatedAt.Value.Date);
            }


            if (booksFilterRequest.categoryId is not null)
            {
                bookss = bookss.Where(e => e.CategoryId == booksFilterRequest.categoryId);
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

        [HttpPost("")]
        public async Task<IActionResult> Create([FromForm] BookCreateRequest booksCreateRequest)
        {
            var books = booksCreateRequest.Adapt<Book>();

            if (booksCreateRequest.Img is not null && booksCreateRequest.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(booksCreateRequest.Img.FileName);

                // Save Img in wwwroot
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\books_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    booksCreateRequest.Img.CopyTo(stream);
                }

                // Save Img in Db
                books.MainImg = fileName;
            }

            await _booksRepository.CreateAsync(books);
            await _booksRepository.CommitAsync();

            //if (booksCreateRequest.SubImgs is not null && booksCreateRequest.SubImgs.Count > 0)
            //{
            //    foreach (var item in booksCreateRequest.SubImgs)
            //    {
            //        // Save Img in wwwroot
            //        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);

            //        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\books_images\\books_sub_images", fileName);

            //        using (var stream = System.IO.File.Create(filePath))
            //        {
            //            item.CopyTo(stream);
            //        }

            //        // Save Img in Db
            //        await _booksSubImageRepository.CreateAsync(new()
            //        {
            //            Img = fileName,
            //            BookId = books.Id
            //        });
            //    }

            //    await _booksSubImageRepository.CommitAsync();
            //}

            return CreatedAtAction(nameof(GetOne), new { id = books.Id }, new SuccessModel
            {
                Message = "Add Book Successfully"
            });
        }
        [HttpGet("MyRating")]
        public async Task<IActionResult> GetMyRating(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);

            var rating = await _ratingRepository
                .GetOneAsync(e => e.BookId == bookId && e.ApplicationUserId == user.Id);

            if (rating is null)
                return Ok(new { Value = 0 });


            return Ok(new { Value = rating.Value });
        }


        [HttpPost("Rate")]
        public async Task<IActionResult> RateBook(int bookId, byte value)
        {
            if (value < 1 || value > 5)
                return BadRequest("Rating value must be between 1 and 5");

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return Unauthorized();

            var book = await _booksRepository.GetOneAsync(e => e.Id == bookId);
            if (book is null)
                return NotFound("Book not found");

            var existingRating = await _ratingRepository
                .GetOneAsync(e => e.BookId == bookId && e.ApplicationUserId == user.Id);

            if (existingRating is null)
            {
                // Add new
                await _ratingRepository.CreateAsync(new BookRating
                {
                    BookId = bookId,
                    ApplicationUserId = user.Id,
                    Value = value
                });
            }
            else
            {
                // Update rating
                existingRating.Value = value;
            }

            await _ratingRepository.CommitAsync();

            return Ok(new { Message = "Rating saved successfully" });
        }

        [Authorize(Roles = $"{SD.Super_Admin_Role},{SD.Admin_Role}")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var books = await _booksRepository.GetOneAsync(e => e.Id == id, tracked: false);

            var booksSubImages = await _booksSubImageRepository.GetAsync(e => e.BookId == id, tracked: false);

            return Ok(new
            {
                Book = books,
                BookSubImages = booksSubImages
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role},{SD.Admin_Role}")]
        public async Task<IActionResult> Edit(int id, [FromForm] BooksUpdateRequest booksUpdateRequest)
        {
            var booksInDb = await _booksRepository.GetOneAsync(e => e.Id == id);

            if (booksInDb is null) return NotFound();

            if (booksUpdateRequest.Img is not null && booksUpdateRequest.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(booksUpdateRequest.Img.FileName);

                // Save Img in wwwroot
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\books_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    booksUpdateRequest.Img.CopyTo(stream);
                }

                // Delete Old Img from wwwroot
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", booksInDb.MainImg);

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Save Img in Db
                booksInDb.MainImg = fileName;
            }

            booksInDb.Name = booksUpdateRequest.Name;
            booksInDb.Description = booksUpdateRequest.Description;
            booksInDb.Price = booksUpdateRequest.Price;
            booksInDb.Quantity = booksUpdateRequest.Quantity;
            booksInDb.Discount = booksUpdateRequest.Discount;
            booksInDb.Status = booksUpdateRequest.Status;
            booksInDb.CategoryId = booksUpdateRequest.CategoryId;

            await _booksRepository.CommitAsync();

            if (booksUpdateRequest.SubImgs is not null && booksUpdateRequest.SubImgs.Count > 0)
            {
                // Delete Old sub imgs from wwwroot & Db
                var booksSubImages = await _booksSubImageRepository.GetAsync(e => e.BookId == id);

                List<BookSubImages> listOfBookSubImages = [];
                foreach (var item in booksSubImages)
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", item.Img);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    listOfBookSubImages.Add(item);
                }

                _booksSubImageRepository.RemoveRange(listOfBookSubImages);
                await _booksSubImageRepository.CommitAsync();

                // Create & Save New sub imgs
                List<BookSubImages> listOfNewBookSubImages = [];
                foreach (var item in booksUpdateRequest.SubImgs)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\books_images\\books_sub_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }

                    // Save Img in Db
                    listOfNewBookSubImages.Add(new()
                    {
                        Img = fileName,
                        BookId = id
                    });
                }

                await _booksSubImageRepository.AddRangeAsync(listOfNewBookSubImages);
                await _booksSubImageRepository.CommitAsync();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role},{SD.Admin_Role}")]
        public async Task<IActionResult> Delete(int id)
        {
            var books = await _booksRepository.GetOneAsync(e => e.Id == id);

            if (books is null) return NotFound();

            // Delete Old Img from wwwroot
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\books_images", books.MainImg);

            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            // Delete Old sub imgs from wwwroot & Db
            var booksSubImages = await _booksSubImageRepository.GetAsync(e => e.BookId == books.Id);

            List<BookSubImages> listOfBookSubImages = [];
            foreach (var item in booksSubImages)
            {
                var oldSubImgFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\books_images\\books_sub_images", item.Img);

                if (System.IO.File.Exists(oldSubImgFilePath))
                {
                    System.IO.File.Delete(oldSubImgFilePath);
                }

                listOfBookSubImages.Add(item);
            }

            _booksSubImageRepository.RemoveRange(listOfBookSubImages);
            await _booksSubImageRepository.CommitAsync();

            _booksRepository.Delete(books);
            await _booksRepository.CommitAsync();

            return NoContent();
        }
    }
}
