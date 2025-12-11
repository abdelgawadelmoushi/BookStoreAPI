using BookStoreAPI.DTOs.Requests;
using BookStoreAPI.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
            , IRepository<Book> bookRepository, IRepository<Author> authorRepository)
        {
            _booksRepository = booksRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _ratingRepository = ratingRepository;
            _authorRepository = authorRepository;
        }

        [HttpPost("Get")]
        public async Task<IActionResult> Get([FromBody] AuthorFilterRequest authorFilterRequest)
        {
            var authors = await _authorRepository.GetAsync(tracked: false);
            AuthorCreateRequest authorFilterResponse = new();

            if (!string.IsNullOrWhiteSpace(authorFilterRequest.Name))
            {
                var trimmedName = authorFilterRequest.Name.Trim();
                authors = authors.Where(a => a.Name.Contains(trimmedName));
                authorFilterResponse.Name = authorFilterRequest.Name;
            }

            if (authorFilterRequest.Age.HasValue)
            {
                authors = authors.Where(a => a.Age == authorFilterRequest.Age.Value);
                authorFilterResponse.Age = authorFilterRequest.Age.Value;
            }

            if (authorFilterRequest.Skills != null && authorFilterRequest.Skills.Any())
            {
                authors = authors.Where(a => a.Skills.Any(s => authorFilterRequest.Skills.Contains(s)));
                authorFilterResponse.Skills = authorFilterRequest.Skills;
            }


            int pageSize = 8;
            var totalNumberOfPages = Math.Ceiling(authors.Count() / (double)pageSize);
            authorFilterResponse.TotalNumberOfPages = totalNumberOfPages;
            authorFilterResponse.CurrentPage = authorFilterRequest.page;

            authors = authors.Skip((authorFilterRequest.page - 1) * pageSize)
                             .Take(pageSize);

            return Ok(new
            {
                Authors = authors,
                AuthorFilter = authorFilterResponse
            });
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromForm] AuthorCreateRequest authorCreateRequest)
        {
            var author = authorCreateRequest.Adapt<Author>();

            if (authorCreateRequest.Img is not null && authorCreateRequest.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(authorCreateRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Author_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    authorCreateRequest.Img.CopyTo(stream);
                }

                author.Img = fileName;
            }

           


            await _authorRepository.CreateAsync(author);
            await _authorRepository.CommitAsync();

            return CreatedAtAction(nameof(GetOne), new { id = author.Id }, new SuccessModel
            {
                Message = "Author Added Successfully"
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
                await _ratingRepository.CreateAsync(new BookRating
                {
                    BookId = bookId,
                    ApplicationUserId = user.Id,
                    Value = value
                });
            }
            else
            {
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

            return Ok(new
            {
                Book = books
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role},{SD.Admin_Role}")]
        public async Task<IActionResult> Edit(int id, [FromForm] AuthorUpdateRequest request)
        {
            var authorInDb = await _authorRepository
                .GetOneAsync(e => e.Id == id, includes: new System.Linq.Expressions.Expression<Func<Author, object>>[] { e => e.AuthorCategories, e => e.AuthorBooks });

            if (authorInDb is null)
                return NotFound();

            if (request.Img is not null && request.Img.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(request.Img.FileName);
                var filePath = Path.Combine("wwwroot/images/Author_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                    request.Img.CopyTo(stream);

                var oldFilePath = Path.Combine("wwwroot/images/Author_images", authorInDb.Img);
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                authorInDb.Img = fileName;
            }

            authorInDb.Name = request.Name;
            authorInDb.Age = request.Age;
            authorInDb.Skills = request.Skills;

            if (request.CategoryIds != null)
            {
                authorInDb.AuthorCategories.Clear();
                foreach (var catId in request.CategoryIds)
                {
                    authorInDb.AuthorCategories.Add(new AuthorCategory { CategoryId = catId });
                }
            }

            if (request.BookIds != null)
            {
                authorInDb.AuthorBooks.Clear();
                foreach (var bookId in request.BookIds)
                {
                    authorInDb.AuthorBooks.Add(new AuthorBook { BookId = bookId });
                }
            }

            await _authorRepository.CommitAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role},{SD.Admin_Role}")]
        public async Task<IActionResult> Delete(int id)
        {
            var Authors = await _authorRepository.GetOneAsync(e => e.Id == id);

            if (Authors is null) return NotFound();

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Author_images", Authors.Img);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            _authorRepository.Delete(Authors);
            await _authorRepository.CommitAsync();

            return NoContent();
        }
    }
}
