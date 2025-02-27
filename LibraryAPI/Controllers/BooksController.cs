using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Data;
using LibraryAPI.Models;
using LibraryAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/books
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var books = await _context.Books.ToListAsync();

            var bookDtos = books.Select(book => new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Genre = book.Genre,
                Categorization = book.Categorization,
                ISBN = book.ISBN,
                Cover = book.Cover,
                Description = book.Description,
                Summary = book.Summary,
                PublicationYear = book.PublicationYear,
                Publisher = book.Publisher,
                PageCount = book.PageCount,
                Language = book.Language,
                AvailableCopies = book.AvailableCopies,
                DateAdded = book.DateAdded,
                DateModified = book.DateModified
            }).ToList();

            return Ok(bookDtos);
        }

        // GET: api/books/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound(new { Message = "Книга не найдена" });

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Genre = book.Genre,
                Categorization = book.Categorization,
                ISBN = book.ISBN,
                Cover = book.Cover,
                Description = book.Description,
                Summary = book.Summary,
                PublicationYear = book.PublicationYear,
                Publisher = book.Publisher,
                PageCount = book.PageCount,
                Language = book.Language,
                AvailableCopies = book.AvailableCopies,
                DateAdded = book.DateAdded,
                DateModified = book.DateModified
            };

            return Ok(bookDto);
        }

        // POST: api/books
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookCreateDto bookDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = bookDto.Title,
                Authors = bookDto.Authors,
                Genre = bookDto.Genre,
                Categorization = bookDto.Categorization,
                ISBN = bookDto.ISBN,
                Cover = bookDto.Cover,
                Description = bookDto.Description,
                Summary = bookDto.Summary,
                PublicationYear = bookDto.PublicationYear,
                Publisher = bookDto.Publisher,
                PageCount = bookDto.PageCount,
                Language = bookDto.Language,
                AvailableCopies = bookDto.AvailableCopies,
                DateAdded = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var result = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Genre = book.Genre,
                Categorization = book.Categorization,
                ISBN = book.ISBN,
                Cover = book.Cover,
                Description = book.Description,
                Summary = book.Summary,
                PublicationYear = book.PublicationYear,
                Publisher = book.Publisher,
                PageCount = book.PageCount,
                Language = book.Language,
                AvailableCopies = book.AvailableCopies,
                DateAdded = book.DateAdded,
                DateModified = book.DateModified
            };

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, result);
        }

        // PUT: api/books/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BookUpdateDto bookDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { Message = "Книга не найдена" });

            book.Title = bookDto.Title;
            book.Authors = bookDto.Authors;
            book.Genre = bookDto.Genre;
            book.Categorization = bookDto.Categorization;
            book.ISBN = bookDto.ISBN;
            book.Cover = bookDto.Cover;
            book.Description = bookDto.Description;
            book.Summary = bookDto.Summary;
            book.PublicationYear = bookDto.PublicationYear;
            book.Publisher = bookDto.Publisher;
            book.PageCount = bookDto.PageCount;
            book.Language = bookDto.Language;
            book.AvailableCopies = bookDto.AvailableCopies;
            book.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Genre = book.Genre,
                Categorization = book.Categorization,
                ISBN = book.ISBN,
                Cover = book.Cover,
                Description = book.Description,
                Summary = book.Summary,
                PublicationYear = book.PublicationYear,
                Publisher = book.Publisher,
                PageCount = book.PageCount,
                Language = book.Language,
                AvailableCopies = book.AvailableCopies,
                DateAdded = book.DateAdded,
                DateModified = book.DateModified
            };

            return Ok(result);
        }

        // DELETE: api/books/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { Message = "Книга не найдена" });

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
