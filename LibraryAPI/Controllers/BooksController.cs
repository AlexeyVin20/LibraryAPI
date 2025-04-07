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
                Genre = book.Genre ?? string.Empty,
                Categorization = book.Categorization ?? string.Empty,
                UDK = book.UDK ?? string.Empty,
                BBK = book.BBK ?? string.Empty,
                ISBN = book.ISBN,
                Cover = book.Cover ?? string.Empty,
                Description = book.Description ?? string.Empty,
                Summary = book.Summary ?? string.Empty,
                PublicationYear = book.PublicationYear,
                Publisher = book.Publisher ?? string.Empty,
                PageCount = book.PageCount,
                Language = book.Language ?? string.Empty,
                AvailableCopies = book.AvailableCopies,
                DateAdded = book.DateAdded,
                DateModified = book.DateModified,
                Edition = book.Edition ?? string.Empty,
                Price = book.Price,
                Format = book.Format ?? string.Empty,
                OriginalTitle = book.OriginalTitle ?? string.Empty,
                OriginalLanguage = book.OriginalLanguage ?? string.Empty,
                IsEbook = book.IsEbook,
                Condition = book.Condition ?? string.Empty,
                ShelfId = book.ShelfId,
                Position = book.Position
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
                Genre = book.Genre ?? string.Empty,
                Categorization = book.Categorization ?? string.Empty,
                UDK = book.UDK ?? string.Empty,
                BBK = book.BBK ?? string.Empty,
                ISBN = book.ISBN,
                Cover = book.Cover ?? string.Empty,
                Description = book.Description ?? string.Empty,
                Summary = book.Summary ?? string.Empty,
                PublicationYear = book.PublicationYear,
                Publisher = book.Publisher ?? string.Empty,
                PageCount = book.PageCount,
                Language = book.Language ?? string.Empty,
                AvailableCopies = book.AvailableCopies,
                DateAdded = book.DateAdded,
                DateModified = book.DateModified,
                Edition = book.Edition ?? string.Empty,
                Price = book.Price,
                Format = book.Format ?? string.Empty,
                OriginalTitle = book.OriginalTitle ?? string.Empty,
                OriginalLanguage = book.OriginalLanguage ?? string.Empty,
                IsEbook = book.IsEbook,
                Condition = book.Condition ?? string.Empty,
                ShelfId = book.ShelfId,
                Position = book.Position
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
                ShelfId = bookDto.ShelfId,
                Position = bookDto.Position
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var result = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Genre = book.Genre ?? string.Empty,
                Categorization = book.Categorization ?? string.Empty,
                UDK = book.UDK ?? string.Empty,
                BBK = book.BBK ?? string.Empty,
                ISBN = book.ISBN,
                Cover = book.Cover ?? string.Empty,
                Description = book.Description ?? string.Empty,
                Summary = book.Summary ?? string.Empty,
                PublicationYear = book.PublicationYear,
                Publisher = book.Publisher ?? string.Empty,
                PageCount = book.PageCount,
                Language = book.Language ?? string.Empty,
                AvailableCopies = book.AvailableCopies,
                DateAdded = book.DateAdded,
                DateModified = book.DateModified,
                Edition = book.Edition ?? string.Empty,
                Price = book.Price,
                Format = book.Format ?? string.Empty,
                OriginalTitle = book.OriginalTitle ?? string.Empty,
                OriginalLanguage = book.OriginalLanguage ?? string.Empty,
                IsEbook = book.IsEbook,
                Condition = book.Condition ?? string.Empty,
                ShelfId = book.ShelfId,
                Position = book.Position
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
            book.Position = bookDto.Position;

            await _context.SaveChangesAsync();

            var result = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Genre = book.Genre ?? string.Empty,
                Categorization = book.Categorization ?? string.Empty,
                UDK = book.UDK ?? string.Empty,
                BBK = book.BBK ?? string.Empty,
                ISBN = book.ISBN,
                Cover = book.Cover ?? string.Empty,
                Description = book.Description ?? string.Empty,
                Summary = book.Summary ?? string.Empty,
                PublicationYear = book.PublicationYear,
                Publisher = book.Publisher ?? string.Empty,
                PageCount = book.PageCount,
                Language = book.Language ?? string.Empty,
                AvailableCopies = book.AvailableCopies,
                DateAdded = book.DateAdded,
                DateModified = book.DateModified,
                Edition = book.Edition ?? string.Empty,
                Price = book.Price,
                Format = book.Format ?? string.Empty,
                OriginalTitle = book.OriginalTitle ?? string.Empty,
                OriginalLanguage = book.OriginalLanguage ?? string.Empty,
                IsEbook = book.IsEbook,
                Condition = book.Condition ?? string.Empty,
                ShelfId = book.ShelfId,
                Position = book.Position
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
                Genre = book.Genre ?? string.Empty,
                ShelfId = book.ShelfId,
                Position = book.Position,
                // Другие поля...
            });
        }

        [HttpPost("{id}/auto-position")]
        public async Task<IActionResult> AutoPositionBook(Guid id)
        {
            try
            {
                // Получаем книгу по ID
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return NotFound("Книга не найдена");
                }

                // Получаем категоризацию книги
                var categorization = book.Categorization;
                if (string.IsNullOrEmpty(categorization))
                {
                    return BadRequest("У книги отсутствует категоризация");
                }

                // Получаем все полки
                var shelves = await _context.Shelves.ToListAsync();

                // Находим полки с подходящей категорией
                var matchingShelves = shelves
                    .Where(s => string.Equals(s.Category, categorization, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!matchingShelves.Any())
                {
                    // Если нет точного совпадения, ищем полки, содержащие категорию в начале строки
                    matchingShelves = shelves
                        .Where(s => categorization.StartsWith(s.Category, StringComparison.OrdinalIgnoreCase) ||
                                   s.Category.StartsWith(categorization, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    // Если до сих пор нет совпадений, используем общую полку
                    if (!matchingShelves.Any())
                    {
                        matchingShelves = shelves
                            .Where(s => string.Equals(s.Category, "Общая", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        // Если нет общей полки, используем любую доступную
                        if (!matchingShelves.Any())
                        {
                            matchingShelves = shelves.ToList();
                        }
                    }
                }

                if (!matchingShelves.Any())
                {
                    return BadRequest("Нет доступных полок для размещения книги");
                }

                // Получаем все книги с расположением на полках
                var allBooks = await _context.Books.ToListAsync();
                var booksWithPositions = allBooks
                    .Where(b => b.ShelfId != null && b.Position != null)
                    .ToList();

                // Ищем свободное место на подходящих полках
                foreach (var shelf in matchingShelves)
                {
                    // Проверяем занятые позиции на этой полке
                    var occupiedPositions = booksWithPositions
                        .Where(b => b.ShelfId == shelf.Id)
                        .Select(b => b.Position)
                        .ToHashSet();

                    // Ищем первую свободную позицию
                    for (int position = 0; position < shelf.Capacity; position++)
                    {
                        if (!occupiedPositions.Contains(position))
                        {
                            // Найдена свободная позиция, размещаем книгу
                            book.ShelfId = shelf.Id;
                            book.Position = position;

                            // Обновляем книгу в базе данных
                            await _context.SaveChangesAsync();

                            // Возвращаем успешный результат с информацией о размещении
                            return Ok(new
                            {
                                message = "Книга успешно размещена на полке",
                                shelfId = shelf.Id,
                                position = position,
                                category = shelf.Category
                            });
                        }
                    }
                }

                // Если не нашли свободной позиции
                return BadRequest("На подходящих полках нет свободных мест");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при автоматическом размещении книги: {ex.Message}");
            }
        }
    }
}
