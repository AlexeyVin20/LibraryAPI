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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
        {
            var books = await _context.Books.Include(b => b.Shelf).ToListAsync();
            var bookDtos = books.Select(book => new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Genre = book.Genre,
                Categorization = book.Categorization,
                UDK = book.UDK,
                BBK = book.BBK,
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
                DateModified = book.DateModified,
                Edition = book.Edition,
                Price = book.Price,
                Format = book.Format,
                OriginalTitle = book.OriginalTitle,
                OriginalLanguage = book.OriginalLanguage,
                IsEbook = book.IsEbook,
                Condition = book.Condition,
                ShelfId = book.ShelfId
            }).ToList();
            return Ok(bookDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetById(Guid id)
        {
            var book = await _context.Books.Include(b => b.Shelf).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return NotFound(new { Message = "Книга не найдена" });

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Genre = book.Genre,
                Categorization = book.Categorization,
                UDK = book.UDK,
                BBK = book.BBK,
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
                DateModified = book.DateModified,
                Edition = book.Edition,
                Price = book.Price,
                Format = book.Format,
                OriginalTitle = book.OriginalTitle,
                OriginalLanguage = book.OriginalLanguage,
                IsEbook = book.IsEbook,
                Condition = book.Condition,
                ShelfId = book.ShelfId
            };
            return Ok(bookDto);
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> Create([FromBody] BookCreateDto bookDto)
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
                UDK = bookDto.UDK,
                BBK = bookDto.BBK,
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
                DateModified = DateTime.UtcNow,
                Edition = bookDto.Edition,
                Price = bookDto.Price,
                Format = bookDto.Format,
                OriginalTitle = bookDto.OriginalTitle,
                OriginalLanguage = bookDto.OriginalLanguage,
                IsEbook = bookDto.IsEbook,
                Condition = bookDto.Condition,
                ShelfId = bookDto.ShelfId
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
                UDK = book.UDK,
                BBK = book.BBK,
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
                DateModified = book.DateModified,
                Edition = book.Edition,
                Price = book.Price,
                Format = book.Format,
                OriginalTitle = book.OriginalTitle,
                OriginalLanguage = book.OriginalLanguage,
                IsEbook = book.IsEbook,
                Condition = book.Condition,
                ShelfId = book.ShelfId
            };

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BookDto>> Update(Guid id, [FromBody] BookUpdateDto bookDto)
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
            book.UDK = bookDto.UDK;
            book.BBK = bookDto.BBK;
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
            book.Edition = bookDto.Edition;
            book.Price = bookDto.Price;
            book.Format = bookDto.Format;
            book.OriginalTitle = bookDto.OriginalTitle;
            book.OriginalLanguage = bookDto.OriginalLanguage;
            book.IsEbook = bookDto.IsEbook;
            book.Condition = bookDto.Condition;
            book.ShelfId = bookDto.ShelfId;

            await _context.SaveChangesAsync();

            var result = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Genre = book.Genre,
                Categorization = book.Categorization,
                UDK = book.UDK,
                BBK = book.BBK,
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
                DateModified = book.DateModified,
                Edition = book.Edition,
                Price = book.Price,
                Format = book.Format,
                OriginalTitle = book.OriginalTitle,
                OriginalLanguage = book.OriginalLanguage,
                IsEbook = book.IsEbook,
                Condition = book.Condition,
                ShelfId = book.ShelfId
            };

            return Ok(result);
        }

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

        [HttpPut("{id}/position")]
        public async Task<ActionResult<BookDto>> UpdateBookPosition(Guid id, [FromBody] BookPositionDto positionDto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { Message = "Книга не найдена" });

            // Проверяем, что полка существует
            var shelf = await _context.Shelves.FindAsync(positionDto.ShelfId);
            if (shelf == null)
                return NotFound(new { Message = "Полка не найдена" });

            // Проверяем, что позиция в пределах емкости полки
            if (positionDto.Position >= shelf.Capacity)
                return BadRequest(new { Message = "Позиция превышает емкость полки" });

            // Проверяем, что позиция не занята другой книгой
            var existingBook = await _context.Books
                .FirstOrDefaultAsync(b => b.ShelfId == positionDto.ShelfId && b.Position == positionDto.Position);

            if (existingBook != null && existingBook.Id != id)
                return BadRequest(new { Message = "Эта позиция уже занята" });

            book.ShelfId = positionDto.ShelfId;
            book.Position = positionDto.Position;
            book.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Genre = book.Genre,
                ShelfId = book.ShelfId,
                Position = book.Position,
                // Другие поля...
            });
        }

    }
}
