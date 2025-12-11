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
    public class AuthorsController : ControllerBase
    {
        private readonly IRepository<Book> _booksRepository;
        private readonly IBookSubImagesRepository _booksSubImageRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<BookRating> _ratingRepository;
        private readonly IRepository<Author> _authorRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthorsController(IRepository<Book> booksRepository,
           IRepository<Category> categoryRepository
           , UserManager<ApplicationUser> userManager, IRepository<BookRating> ratingRepository
            , IRepository<Book> bookRepository , IRepository<Author> authorRepository)
        {
            _booksRepository = booksRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _ratingRepository = ratingRepository;
            _authorRepository = authorRepository;
        }

        [HttpPost("Get")]
        public async Task<IActionResult> Get(AuthorFilterRequest AuthorFilterRequest)
        {
            var authors = await _authorRepository.GetAsync(tracked: false);

            AuthorCreateRequest authorFilterResponse = new();

            // Add Filter
            if (AuthorFilterRequest.Name is not null)
            {
                var AuthorsNameTrimmed = AuthorFilterRequest.Name.Trim();

                authors = authors.Where(e => e.Name.Contains(AuthorsNameTrimmed));
                authorFilterResponse.Name = AuthorFilterRequest.Name;
            }

            // Fix: Filter by Age (assume AuthorFilterRequest.Age is int? and Author.Age is int)
            if (AuthorFilterRequest.Age != null)
            {
                authors = authors.Where(e => e.Age == AuthorFilterRequest.Age.Value);
                authorFilterResponse.Age = AuthorFilterRequest.Age.Value;
            }

            if (AuthorFilterRequest.Skills != null)
            {
                authors = authors.Where(e => e.Skills == AuthorFilterRequest.Skills);
            }


            if (AuthorFilterRequest.AuthorCategories is not null)
            {
                authors = authors.Where(e => e.AuthorCategories == AuthorFilterRequest.AuthorCategories);
                authorFilterResponse.AuthorCategories = AuthorFilterRequest.AuthorCategories;
            }
            if (AuthorFilterRequest.Authorbooks is not null)
            {
                authors = authors.Where(e => e.Authorbooks == AuthorFilterRequest.Authorbooks);
                authorFilterResponse.Authorbooks = AuthorFilterRequest.Authorbooks;
            }


            // Add Pagination
            var totalNumberOfPages = Math.Ceiling(authors.Count() / 8.0);
            authorFilterResponse.TotalNumberOfPages = totalNumberOfPages;
            authorFilterResponse.CurrentPage = AuthorFilterRequest.page;

            authors = authors.Skip((AuthorFilterRequest.page - 1) * 8).Take(8);

            return Ok(new
            {
                Books = authors,
                BookFilter = authorFilterResponse
            });
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromForm] AuthorCreateRequest authorCreateRequest)
        {
            var Authors = authorCreateRequest.Adapt<Book>();

            if (authorCreateRequest.Img is not null && authorCreateRequest.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(authorCreateRequest.Img.FileName);

                // Save Img in wwwroot
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Author_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    authorCreateRequest.Img.CopyTo(stream);
                }

                // Save Img in Db
                Authors.MainImg = fileName;
            }

            await _booksRepository.CreateAsync(Authors);
            await _booksRepository.CommitAsync();

            #region  forSubIMG if required
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

            #endregion

            return CreatedAtAction(nameof(GetOne), new { id = Authors.Id }, new SuccessModel
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

            //var booksSubImages = await _booksSubImageRepository.GetAsync(e => e.BookId == id, tracked: false);

            return Ok(new
            {
                Book = books,
                //BookSubImages = booksSubImages
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role},{SD.Admin_Role}")]
        public async Task<IActionResult> Edit(int id, [FromForm] AuthorUpdateRequest authorUpdateRequest)
        {
            var AuthorsInDb = await _authorRepository.GetOneAsync(e => e.Id == id);

            if (AuthorsInDb is null) return NotFound();

            if (authorUpdateRequest.Img is not null && authorUpdateRequest.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(authorUpdateRequest.Img.FileName);

                // Save Img in wwwroot
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Author_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    authorUpdateRequest.Img.CopyTo(stream);
                }

                // Delete Old Img from wwwroot
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Author_images", AuthorsInDb.Img);

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Save Img in Db
                AuthorsInDb.Img = fileName;
            }

            AuthorsInDb.Name = authorUpdateRequest.Name;
            AuthorsInDb.Age = authorUpdateRequest.Age;
            AuthorsInDb.AuthorCategories = authorUpdateRequest.AuthorCategories;
            AuthorsInDb.Authorbooks = authorUpdateRequest.Authorbooks;
            AuthorsInDb.Skills = authorUpdateRequest.Skills;

            #region forSubIMG if required

            //if (authorUpdateRequest.SubImgs is not null && authorUpdateRequest.SubImgs.Count > 0)
            //{
            //    // Delete Old sub imgs from wwwroot & Db
            //    var booksSubImages = await _booksSubImageRepository.GetAsync(e => e.BookId == id);

            //    List<BookSubImages> listOfBookSubImages = [];
            //    foreach (var item in booksSubImages)
            //    {
            //        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", item.Img);

            //        if (System.IO.File.Exists(oldFilePath))
            //        {
            //            System.IO.File.Delete(oldFilePath);
            //        }

            //        listOfBookSubImages.Add(item);
            //    }

            //    _booksSubImageRepository.RemoveRange(listOfBookSubImages);
            //    await _booksSubImageRepository.CommitAsync();

            //    // Create & Save New sub imgs
            //    List<BookSubImages> listOfNewBookSubImages = [];
            //    foreach (var item in authorUpdateRequest.SubImgs)
            //    {
            //        // Save Img in wwwroot
            //        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);

            //        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\books_images\\books_sub_images", fileName);

            //        using (var stream = System.IO.File.Create(filePath))
            //        {
            //            item.CopyTo(stream);
            //        }

            //        // Save Img in Db
            //        listOfNewBookSubImages.Add(new()
            //        {
            //            Img = fileName,
            //            BookId = id
            //        });
            //    }

            //    await _booksSubImageRepository.AddRangeAsync(listOfNewBookSubImages);
            //    await _booksSubImageRepository.CommitAsync();
            //}
            #endregion


            await _authorRepository.CommitAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role},{SD.Admin_Role}")]
        public async Task<IActionResult> Delete(int id)
        {
            var Authors = await _authorRepository.GetOneAsync(e => e.Id == id);

            if (Authors is null) return NotFound();

            // Delete Old Img from wwwroot
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Author_images", Authors.Img);

            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
            #region forSubIMG if required
            // Delete Old sub imgs from wwwroot & Db
            //var authorSubImages = await _authorSubImageRepository.GetAsync(e => e.AuthorId == Authors.Id);

            //List<BookSubImages> listOfBookSubImages = [];
            //foreach (var item in booksSubImages)
            //{
            //    var oldSubImgFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\books_images\\books_sub_images", item.Img);

            //    if (System.IO.File.Exists(oldSubImgFilePath))
            //    {
            //        System.IO.File.Delete(oldSubImgFilePath);
            //    }

            //    listOfBookSubImages.Add(item);
            //}

            //_booksSubImageRepository.RemoveRange(listOfBookSubImages);
            //await _booksSubImageRepository.CommitAsync();

            #endregion

            _authorRepository.Delete(Authors);
            await _authorRepository.CommitAsync();

            return NoContent();
        }
    }
}
