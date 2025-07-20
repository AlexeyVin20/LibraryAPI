using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Data;
using LibraryAPI.Models;
using LibraryAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;

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

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BookDto>>> SearchBooks(
            [FromQuery] string? title,
            [FromQuery] string? authors,
            [FromQuery] string? genre,
            [FromQuery] string? Categorization,
            [FromQuery] string? isbn,
            [FromQuery] bool? availableCopies,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            var query = _context.Books.Include(b => b.Shelf).AsQueryable();

            // Создаем список условий для текстовых полей
            var textConditions = new List<Expression<Func<Book, bool>>>();

            if (!string.IsNullOrEmpty(title))
            {
                textConditions.Add(b => b.Title.Contains(title));
            }

            if (!string.IsNullOrEmpty(authors))
            {
                textConditions.Add(b => b.Authors.Contains(authors));
            }

            if (!string.IsNullOrEmpty(genre))
            {
                textConditions.Add(b => b.Genre.Contains(genre));
            }

            if (!string.IsNullOrEmpty(Categorization))
            {
                textConditions.Add(b => b.Categorization.Contains(Categorization));
            }

            if (!string.IsNullOrEmpty(isbn))
            {
                textConditions.Add(b => b.ISBN.Contains(isbn));
            }

            // Применяем OR логику для текстовых полей
            if (textConditions.Any())
            {
                var parameter = Expression.Parameter(typeof(Book), "b");
                Expression combinedCondition = null;
                
                foreach (var condition in textConditions)
                {
                    var invokedCondition = Expression.Invoke(condition, parameter);
                    if (combinedCondition == null)
                    {
                        combinedCondition = invokedCondition;
                    }
                    else
                    {
                        combinedCondition = Expression.OrElse(combinedCondition, invokedCondition);
                    }
                }
                
                var lambda = Expression.Lambda<Func<Book, bool>>(combinedCondition, parameter);
                query = query.Where(lambda);
            }

            // Применяем фильтр по доступности (AND логика)
            if (availableCopies.HasValue && availableCopies.Value)
            {
                query = query.Where(b => b.AvailableCopies > 0);
            }

            var books = await query
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

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

        [HttpGet("statistics")]
        public async Task<ActionResult<BookStatisticsDto>> GetBookStatistics()
        {
            // Most popular books (top 5 by reservation count)
            var popularBookIds = await _context.Reservations
                .GroupBy(r => r.BookId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToListAsync();

            var popularBooks = await _context.Books
                .Where(b => popularBookIds.Contains(b.Id))
                .Select(book => new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Authors = book.Authors,
                    Genre = book.Genre ?? string.Empty,
                    ISBN = book.ISBN,
                    Cover = book.Cover ?? string.Empty,
                    AvailableCopies = book.AvailableCopies
                })
                .ToListAsync();
            
            var orderedPopularBooks = popularBookIds
                .Select(id => popularBooks.FirstOrDefault(b => b.Id == id))
                .Where(b => b != null)
                .ToList();

            // Genre distribution
            var genreDistribution = await _context.Books
                .Where(b => !string.IsNullOrEmpty(b.Genre))
                .GroupBy(b => b.Genre)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            // Availability statistics
            var totalBooks = await _context.Books.CountAsync();
            var availableBooks = await _context.Books.CountAsync(b => b.AvailableCopies > 0);
            var availability = new AvailabilityStats
            {
                Total = totalBooks,
                Available = availableBooks,
                NotAvailable = totalBooks - availableBooks
            };

            var statistics = new BookStatisticsDto
            {
                MostPopularBooks = orderedPopularBooks,
                GenreDistribution = genreDistribution,
                Availability = availability
            };

            return Ok(statistics);
        }

        [HttpGet("top-popular")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetTopPopularBooks([FromQuery] int limit = 10, [FromQuery] string period = "month")
        {
            DateTime startDate;
            switch (period.ToLower())
            {
                case "week":
                    startDate = DateTime.UtcNow.AddDays(-7);
                    break;
                case "year":
                    startDate = DateTime.UtcNow.AddYears(-1);
                    break;
                case "month":
                default:
                    startDate = DateTime.UtcNow.AddMonths(-1);
                    break;
            }

            var popularBookIds = await _context.Reservations
                .Where(r => r.ReservationDate >= startDate)
                .GroupBy(r => r.BookId)
                .OrderByDescending(g => g.Count())
                .Take(limit)
                .Select(g => g.Key)
                .ToListAsync();
            
            var popularBooks = await _context.Books
                .Where(b => popularBookIds.Contains(b.Id))
                .Select(book => new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Authors = book.Authors,
                    Genre = book.Genre ?? string.Empty,
                    ISBN = book.ISBN,
                    Cover = book.Cover ?? string.Empty,
                    AvailableCopies = book.AvailableCopies
                })
                .ToListAsync();

            var orderedPopularBooks = popularBookIds
                .Select(id => popularBooks.FirstOrDefault(b => b.Id == id))
                .Where(b => b != null)
                .ToList();

            return Ok(orderedPopularBooks);
        }

        [HttpPut("{id}/shelf/remove")]
        public async Task<ActionResult<BookDto>> RemoveBookFromShelfAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { Message = "Книга не найдена" });
            }

            book.ShelfId = null;
            book.Position = null;
            book.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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

        [HttpPost("{id}/favorite")]
        public async Task<IActionResult> AddToFavorites(Guid id, [FromBody] FavoriteBookUserDto dto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { message = "Книга не найдена" });

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var existingFavorite = await _context.FavoriteBooks
                .FirstOrDefaultAsync(fb => fb.UserId == dto.UserId && fb.BookId == id);

            if (existingFavorite != null)
                return BadRequest(new { message = "Эта книга уже в избранном" });

            var favoriteBook = new FavoriteBook
            {
                UserId = dto.UserId,
                BookId = id,
                DateAdded = DateTime.UtcNow
            };

            _context.FavoriteBooks.Add(favoriteBook);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Книга добавлена в избранное" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ошибка при добавлении книги в избранное: {ex.Message}" });
            }
        }

        [HttpDelete("{id}/favorite/{userId}")]
        public async Task<IActionResult> RemoveFromFavorites(Guid id, Guid userId)
        {
            var favoriteBook = await _context.FavoriteBooks
                .FirstOrDefaultAsync(fb => fb.UserId == userId && fb.BookId == id);

            if (favoriteBook == null)
                return NotFound(new { message = "Книга не найдена в избранном" });

            _context.FavoriteBooks.Remove(favoriteBook);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Книга удалена из избранного" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ошибка при удалении книги из избранного: {ex.Message}" });
            }
        }

        [HttpPost("{id}/genre")]
        public async Task<ActionResult<BookDto>> UpdateBookGenre(Guid id, [FromBody] UpdateGenreDto genreDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { Message = "Книга не найдена" });

            book.Genre = genreDto.Genre;
            book.DateModified = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();

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
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Ошибка при обновлении жанра книги: {ex.Message}" });
            }
        }

        [HttpPost("{id}/categorization")]
        public async Task<ActionResult<BookDto>> UpdateBookCategorization(Guid id, [FromBody] UpdateCategorizationDto categorizationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { Message = "Книга не найдена" });

            book.Categorization = categorizationDto.Categorization;
            book.DateModified = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();

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
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Ошибка при обновлении категоризации книги: {ex.Message}" });
            }
        }

        [HttpGet("{id}/availability")]
        public async Task<ActionResult<BookAvailabilityDto>> GetBookAvailability(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { Message = "Книга не найдена" });

            // Количество доступных экземпляров
            var availableCount = await _context.BookInstances.CountAsync(bi => bi.BookId == id && bi.Status == "Доступна" && bi.IsActive);

            // Ближайшая дата возврата (по резервированиям со статусом "Выдана")
            var nearestReturnDate = await _context.Reservations
                .Where(r => r.BookId == id && r.Status == ReservationStatus.Выдана && r.ExpirationDate > DateTime.UtcNow)
                .OrderBy(r => r.ExpirationDate)
                .Select(r => (DateTime?)r.ExpirationDate)
                .FirstOrDefaultAsync();

            // Очередь резервирований (ожидающие и одобренные)
            var queue = await _context.Reservations
                .Where(r => r.BookId == id && (r.Status == ReservationStatus.Обрабатывается || r.Status == ReservationStatus.Одобрена))
                .OrderBy(r => r.ReservationDate)
                .Join(_context.Users, r => r.UserId, u => u.Id, (r, u) => new ReservationQueueItemDto
                {
                    ReservationId = r.Id,
                    UserId = u.Id,
                    UserFullName = u.FullName,
                    ReservationDate = r.ReservationDate,
                    Status = r.Status.ToString()
                })
                .ToListAsync();

            var result = new BookAvailabilityDto
            {
                AvailableCount = availableCount,
                NearestReturnDate = nearestReturnDate,
                ReservationQueue = queue
            };

            return Ok(result);
        }
    }

    public class UpdateGenreDto
    {
        public string Genre { get; set; } = string.Empty;
    }

    public class UpdateCategorizationDto
    {
        public string Categorization { get; set; } = string.Empty;
    }
}
