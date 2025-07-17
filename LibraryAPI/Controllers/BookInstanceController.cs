using LibraryAPI.Data;
using LibraryAPI.Models;
using LibraryAPI.Models.DTOs;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookInstanceController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly IBookInstanceAllocationService _allocationService;

        public BookInstanceController(LibraryDbContext context, IBookInstanceAllocationService allocationService)
        {
            _context = context;
            _allocationService = allocationService;
        }

        // GET: api/BookInstance
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookInstanceDto>>> GetBookInstances(
            [FromQuery] Guid? bookId = null,
            [FromQuery] string? status = null,
            [FromQuery] int? shelfId = null,
            [FromQuery] bool? isActive = null)
        {
            var query = _context.BookInstances
                .Include(bi => bi.Book)
                .Include(bi => bi.Shelf)
                .AsQueryable();

            if (bookId.HasValue)
                query = query.Where(bi => bi.BookId == bookId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(bi => bi.Status == status);

            if (shelfId.HasValue)
                query = query.Where(bi => bi.ShelfId == shelfId.Value);

            if (isActive.HasValue)
                query = query.Where(bi => bi.IsActive == isActive.Value);

            var instances = await query.ToListAsync();

            var instanceDtos = instances.Select(bi => new BookInstanceDto
            {
                Id = bi.Id,
                BookId = bi.BookId,
                Book = bi.Book != null ? new BookDto
                {
                    Id = bi.Book.Id,
                    Title = bi.Book.Title,
                    Authors = bi.Book.Authors,
                    ISBN = bi.Book.ISBN,
                    Genre = bi.Book.Genre,
                    Publisher = bi.Book.Publisher,
                    PublicationYear = bi.Book.PublicationYear
                } : null,
                InstanceCode = bi.InstanceCode,
                Status = bi.Status,
                Condition = bi.Condition,
                PurchasePrice = bi.PurchasePrice,
                DateAcquired = bi.DateAcquired,
                DateLastChecked = bi.DateLastChecked,
                Notes = bi.Notes,
                ShelfId = bi.ShelfId,
                Shelf = bi.Shelf != null ? new ShelfDto
                {
                    Id = bi.Shelf.Id,
                    Category = bi.Shelf.Category,
                    ShelfNumber = bi.Shelf.ShelfNumber
                } : null,
                Position = bi.Position,
                Location = bi.Location,
                IsActive = bi.IsActive,
                DateCreated = bi.DateCreated,
                DateModified = bi.DateModified
            }).ToList();

            return Ok(instanceDtos);
        }

        // GET: api/BookInstance/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BookInstanceDto>> GetBookInstance(Guid id)
        {
            var instance = await _context.BookInstances
                .Include(bi => bi.Book)
                .Include(bi => bi.Shelf)
                .FirstOrDefaultAsync(bi => bi.Id == id);

            if (instance == null)
            {
                return NotFound($"Экземпляр книги с ID {id} не найден");
            }

            var instanceDto = new BookInstanceDto
            {
                Id = instance.Id,
                BookId = instance.BookId,
                Book = instance.Book != null ? new BookDto
                {
                    Id = instance.Book.Id,
                    Title = instance.Book.Title,
                    Authors = instance.Book.Authors,
                    ISBN = instance.Book.ISBN,
                    Genre = instance.Book.Genre,
                    Publisher = instance.Book.Publisher,
                    PublicationYear = instance.Book.PublicationYear,
                    Description = instance.Book.Description,
                    Cover = instance.Book.Cover
                } : null,
                InstanceCode = instance.InstanceCode,
                Status = instance.Status,
                Condition = instance.Condition,
                PurchasePrice = instance.PurchasePrice,
                DateAcquired = instance.DateAcquired,
                DateLastChecked = instance.DateLastChecked,
                Notes = instance.Notes,
                ShelfId = instance.ShelfId,
                Shelf = instance.Shelf != null ? new ShelfDto
                {
                    Id = instance.Shelf.Id,
                    Category = instance.Shelf.Category,
                    ShelfNumber = instance.Shelf.ShelfNumber
                } : null,
                Position = instance.Position,
                Location = instance.Location,
                IsActive = instance.IsActive,
                DateCreated = instance.DateCreated,
                DateModified = instance.DateModified
            };

            return Ok(instanceDto);
        }

        // GET: api/BookInstance/book/{bookId}
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<BookInstanceSimpleDto>>> GetBookInstancesByBookId(Guid bookId)
        {
            var instances = await _context.BookInstances
                .Where(bi => bi.BookId == bookId && bi.IsActive)
                .Select(bi => new BookInstanceSimpleDto
                {
                    Id = bi.Id,
                    BookId = bi.BookId,
                    InstanceCode = bi.InstanceCode,
                    Status = bi.Status,
                    Condition = bi.Condition,
                    Location = bi.Location,
                    IsActive = bi.IsActive
                })
                .ToListAsync();

            return Ok(instances);
        }

        // POST: api/BookInstance
        [HttpPost]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<ActionResult<BookInstanceDto>> CreateBookInstance(BookInstanceCreateDto createDto)
        {
            // Проверяем, существует ли книга
            var book = await _context.Books.FindAsync(createDto.BookId);
            if (book == null)
            {
                return BadRequest($"Книга с ID {createDto.BookId} не найдена");
            }

            // Генерируем код экземпляра, если он не предоставлен
            string instanceCode = createDto.InstanceCode;
            if (string.IsNullOrEmpty(instanceCode))
            {
                if (string.IsNullOrEmpty(book.ISBN))
                {
                    return BadRequest("У книги отсутствует ISBN, и код экземпляра не предоставлен");
                }

                var lastNumber = await GetLastInstanceNumber(createDto.BookId, book.ISBN);
                instanceCode = $"{book.ISBN}#{(lastNumber + 1):D3}";
            }

            // Проверяем уникальность кода экземпляра
            var existingInstance = await _context.BookInstances
                .FirstOrDefaultAsync(bi => bi.InstanceCode == instanceCode);
            if (existingInstance != null)
            {
                return BadRequest($"Экземпляр с кодом {instanceCode} уже существует");
            }

            var instance = new BookInstance
            {
                Id = Guid.NewGuid(),
                BookId = createDto.BookId,
                InstanceCode = instanceCode,
                Status = createDto.Status,
                Condition = createDto.Condition,
                PurchasePrice = createDto.PurchasePrice ?? book.Price,
                DateAcquired = createDto.DateAcquired.ToUniversalTime(),
                Notes = createDto.Notes ?? "Экземпляр книги",
                ShelfId = createDto.ShelfId ?? book.ShelfId,
                Position = createDto.Position,
                Location = createDto.Location ?? "Основной фонд",
                IsActive = createDto.IsActive,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            _context.BookInstances.Add(instance);

            // Пересчитываем количество доступных копий книги
            await UpdateBookAvailableCopies(book.Id);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBookInstance), 
                new { id = instance.Id }, 
                await GetBookInstanceDto(instance.Id));
        }

        // PUT: api/BookInstance/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<IActionResult> UpdateBookInstance(Guid id, BookInstanceUpdateDto updateDto)
        {
            var instance = await _context.BookInstances
                .Include(bi => bi.Book)
                .FirstOrDefaultAsync(bi => bi.Id == id);

            if (instance == null)
            {
                return NotFound($"Экземпляр книги с ID {id} не найден");
            }

            // Проверяем уникальность кода экземпляра (если он изменился)
            if (instance.InstanceCode != updateDto.InstanceCode)
            {
                var existingInstance = await _context.BookInstances
                    .FirstOrDefaultAsync(bi => bi.InstanceCode == updateDto.InstanceCode && bi.Id != id);
                if (existingInstance != null)
                {
                    return BadRequest($"Экземпляр с кодом {updateDto.InstanceCode} уже существует");
                }
            }

            var bookId = instance.BookId;

            instance.InstanceCode = updateDto.InstanceCode;
            instance.Status = updateDto.Status;
            instance.Condition = updateDto.Condition;
            instance.PurchasePrice = updateDto.PurchasePrice;
            instance.DateAcquired = updateDto.DateAcquired.ToUniversalTime();
            instance.DateLastChecked = updateDto.DateLastChecked?.ToUniversalTime();
            instance.Notes = updateDto.Notes;
            instance.ShelfId = updateDto.ShelfId;
            instance.Position = updateDto.Position;
            instance.Location = updateDto.Location;
            instance.IsActive = updateDto.IsActive;
            instance.DateModified = DateTime.UtcNow;

            // Пересчитываем количество доступных копий книги
            await UpdateBookAvailableCopies(bookId);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/BookInstance/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> DeleteBookInstance(Guid id)
        {
            var instance = await _context.BookInstances
                .Include(bi => bi.Book)
                .FirstOrDefaultAsync(bi => bi.Id == id);

            if (instance == null)
            {
                return NotFound($"Экземпляр книги с ID {id} не найден");
            }

            var bookId = instance.BookId;
            _context.BookInstances.Remove(instance);

            // Пересчитываем количество доступных копий книги
            await RecalculateBookAvailableCopies(bookId);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/BookInstance/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<IActionResult> UpdateBookInstanceStatus(Guid id, [FromBody] string newStatus)
        {
            var instance = await _context.BookInstances
                .Include(bi => bi.Book)
                .FirstOrDefaultAsync(bi => bi.Id == id);

            if (instance == null)
            {
                return NotFound($"Экземпляр книги с ID {id} не найден");
            }

            var bookId = instance.BookId;
            instance.Status = newStatus;
            instance.DateModified = DateTime.UtcNow;

            if (newStatus == "Доступна")
            {
                instance.DateLastChecked = DateTime.UtcNow;
            }

            // Пересчитываем количество доступных копий книги
            await UpdateBookAvailableCopies(bookId);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/BookInstance/auto-create/{bookId}
        [HttpPost("auto-create/{bookId}")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<IActionResult> AutoCreateBookInstances(Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound($"Книга с ID {bookId} не найдена");
            }

            if (string.IsNullOrEmpty(book.ISBN))
            {
                return BadRequest("У книги отсутствует ISBN, невозможно создать автоматические экземпляры");
            }

            // Получаем существующие экземпляры
            var existingInstances = await _context.BookInstances
                .Where(bi => bi.BookId == bookId && bi.IsActive)
                .CountAsync();

            // Используем AvailableCopies как общее количество копий для автосоздания
            // В реальном проекте лучше добавить отдельное поле TotalCopies
            var totalCopiesNeeded = book.AvailableCopies;
            var instancesToCreate = totalCopiesNeeded - existingInstances;

            if (instancesToCreate <= 0)
            {
                return BadRequest($"Экземпляры уже созданы. Существует {existingInstances} экземпляров, требуется {totalCopiesNeeded}");
            }

            var createdInstances = new List<BookInstance>();

            // Находим последний номер экземпляра для данной книги
            var lastInstanceNumber = await GetLastInstanceNumber(bookId, book.ISBN);

            for (int i = 1; i <= instancesToCreate; i++)
            {
                var instanceNumber = lastInstanceNumber + i;
                var instanceCode = $"{book.ISBN}#{instanceNumber:D3}";

                var instance = new BookInstance
                {
                    Id = Guid.NewGuid(),
                    BookId = bookId,
                    InstanceCode = instanceCode,
                    Status = "Доступна",
                    Condition = "Хорошее",
                    PurchasePrice = book.Price,
                    DateAcquired = DateTime.UtcNow,
                    Notes = "Автоматически созданный экземпляр",
                    ShelfId = book.ShelfId,
                    Position = null,
                    Location = "Основной фонд",
                    IsActive = true,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow
                };

                createdInstances.Add(instance);
            }

            _context.BookInstances.AddRange(createdInstances);
            
            // Пересчитываем количество доступных копий книги
            await UpdateBookAvailableCopies(bookId);
            
            await _context.SaveChangesAsync();

            return Ok(new { 
                Message = $"Создано {instancesToCreate} экземпляров книги", 
                CreatedCount = instancesToCreate,
                InstanceCodes = createdInstances.Select(i => i.InstanceCode).ToList()
            });
        }

        // POST: api/BookInstance/create-multiple/{bookId}
        [HttpPost("create-multiple/{bookId}")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<IActionResult> CreateMultipleBookInstances(Guid bookId, [FromBody] int count)
        {
            if (count <= 0)
            {
                return BadRequest("Количество экземпляров должно быть больше 0");
            }

            if (count > 100)
            {
                return BadRequest("Нельзя создать более 100 экземпляров за раз");
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound($"Книга с ID {bookId} не найдена");
            }

            if (string.IsNullOrEmpty(book.ISBN))
            {
                return BadRequest("У книги отсутствует ISBN, невозможно создать автоматические экземпляры");
            }

            var createdInstances = new List<BookInstance>();
            var lastInstanceNumber = await GetLastInstanceNumber(bookId, book.ISBN);

            for (int i = 1; i <= count; i++)
            {
                var instanceNumber = lastInstanceNumber + i;
                var instanceCode = $"{book.ISBN}#{instanceNumber:D3}";

                var instance = new BookInstance
                {
                    Id = Guid.NewGuid(),
                    BookId = bookId,
                    InstanceCode = instanceCode,
                    Status = "Доступна",
                    Condition = "Хорошее",
                    PurchasePrice = book.Price,
                    DateAcquired = DateTime.UtcNow,
                    Notes = "Автоматически созданный экземпляр",
                    ShelfId = book.ShelfId,
                    Position = null,
                    Location = "Основной фонд",
                    IsActive = true,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow
                };

                createdInstances.Add(instance);
            }

                        _context.BookInstances.AddRange(createdInstances);
            
            // Добавляем к полю book.AvailableCopies количество созданных экземпляров
            book.AvailableCopies += count;
            _context.Books.Update(book);
            
            await _context.SaveChangesAsync();

            return Ok(new { 
                Message = $"Создано {count} экземпляров книги", 
                CreatedCount = count,
                InstanceCodes = createdInstances.Select(i => i.InstanceCode).ToList()
            });
        }

        [HttpPost("bulk-create")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<IActionResult> BulkCreateInstances([FromBody] BulkCreateInstancesDto dto)
        {
            if (dto.Count <= 0)
            {
                return BadRequest("Количество должно быть больше нуля.");
            }

            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null)
            {
                return NotFound($"Книга с ID {dto.BookId} не найдена.");
            }
            if (string.IsNullOrEmpty(book.ISBN))
            {
                return BadRequest("У книги отсутствует ISBN, необходимый для генерации кодов экземпляров.");
            }

            var instances = new List<BookInstance>();
            var lastNumber = await GetLastInstanceNumber(dto.BookId, book.ISBN);

            for (int i = 1; i <= dto.Count; i++)
            {
                var newInstance = new BookInstance
                {
                    Id = Guid.NewGuid(),
                    BookId = dto.BookId,
                    InstanceCode = $"{book.ISBN}#{(lastNumber + i):D3}",
                    Status = dto.DefaultStatus ?? "Доступна",
                    Condition = dto.DefaultCondition ?? "Новая",
                    PurchasePrice = book.Price,
                    DateAcquired = DateTime.UtcNow,
                    IsActive = true,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                    ShelfId = book.ShelfId,
                    Location = "Основной фонд"
                };
                instances.Add(newInstance);
            }

            await _context.BookInstances.AddRangeAsync(instances);
            await UpdateBookAvailableCopies(dto.BookId);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = $"Успешно создано {instances.Count} экземпляров." });
        }

        [HttpPut("bulk-update-status")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<IActionResult> BulkUpdateInstanceStatuses([FromBody] List<BulkInstanceStatusUpdateDto> updates)
        {
            if (updates == null || !updates.Any())
            {
                return BadRequest("Тело запроса не может быть пустым.");
            }

            var errors = new List<object>();
            var bookIdsToRecalculate = new HashSet<Guid>();

            var instanceIds = updates.Select(u => u.Id).ToList();
            var instancesToUpdate = await _context.BookInstances
                .Where(i => instanceIds.Contains(i.Id))
                .ToListAsync();

            var instanceDict = instancesToUpdate.ToDictionary(i => i.Id);

            foreach (var update in updates)
            {
                if (instanceDict.TryGetValue(update.Id, out var instance))
                {
                    instance.Status = update.NewStatus;
                    instance.DateModified = DateTime.UtcNow;
                    bookIdsToRecalculate.Add(instance.BookId);
                }
                else
                {
                    errors.Add(new { id = update.Id, error = "Экземпляр не найден." });
                }
            }

            foreach (var bookId in bookIdsToRecalculate)
            {
                await UpdateBookAvailableCopies(bookId);
            }

            await _context.SaveChangesAsync();

            if (errors.Any())
            {
                return StatusCode(207, new { message = "Операция завершена с ошибками.", errors });
            }

            return Ok(new { message = "Статусы экземпляров успешно обновлены." });
        }

        private async Task<int> GetLastInstanceNumber(Guid bookId, string isbn)
        {
            var instanceCodes = await _context.BookInstances
                .Where(bi => bi.BookId == bookId && bi.InstanceCode.StartsWith(isbn + "#"))
                .Select(bi => bi.InstanceCode)
                .ToListAsync();

            var lastNumber = 0;
            foreach (var code in instanceCodes)
            {
                var parts = code.Split('#');
                if (parts.Length == 2 && int.TryParse(parts[1], out var number))
                {
                    if (number > lastNumber)
                        lastNumber = number;
                }
            }

            return lastNumber;
        }

        private async Task UpdateBookAvailableCopies(Guid bookId)
        {
            await _allocationService.RecalculateBookAvailableCopies(bookId);
        }

        private async Task<BookInstanceDto> GetBookInstanceDto(Guid id)
        {
            var instance = await _context.BookInstances
                .Include(bi => bi.Book)
                .Include(bi => bi.Shelf)
                .FirstOrDefaultAsync(bi => bi.Id == id);

            return new BookInstanceDto
            {
                Id = instance.Id,
                BookId = instance.BookId,
                Book = instance.Book != null ? new BookDto
                {
                    Id = instance.Book.Id,
                    Title = instance.Book.Title,
                    Authors = instance.Book.Authors,
                    ISBN = instance.Book.ISBN
                } : null,
                InstanceCode = instance.InstanceCode,
                Status = instance.Status,
                Condition = instance.Condition,
                PurchasePrice = instance.PurchasePrice,
                DateAcquired = instance.DateAcquired,
                DateLastChecked = instance.DateLastChecked,
                Notes = instance.Notes,
                ShelfId = instance.ShelfId,
                Shelf = instance.Shelf != null ? new ShelfDto
                {
                    Id = instance.Shelf.Id,
                    Category = instance.Shelf.Category,
                    ShelfNumber = instance.Shelf.ShelfNumber
                } : null,
                Position = instance.Position,
                Location = instance.Location,
                IsActive = instance.IsActive,
                DateCreated = instance.DateCreated,
                DateModified = instance.DateModified
            };
        }

        // GET: api/BookInstance/best-available/{bookId}
        [HttpGet("best-available/{bookId}")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<ActionResult<BookInstanceDto>> GetBestAvailableInstance(Guid bookId)
        {
            var bestInstance = await FindBestAvailableInstance(bookId);
            
            if (bestInstance == null)
            {
                return NotFound("Нет доступных экземпляров для данной книги");
            }

            return Ok(await GetBookInstanceDto(bestInstance.Id));
        }

        // Метод для поиска лучшего доступного экземпляра книги используя сервис
        private async Task<BookInstance?> FindBestAvailableInstance(Guid bookId)
        {
            return await _allocationService.FindBestAvailableInstance(bookId);
        }

        // GET: api/BookInstance/stats/{bookId}
        [HttpGet("stats/{bookId}")]
        public async Task<ActionResult> GetBookInstanceStats(Guid bookId)
        {
            var totalInstances = await _context.BookInstances
                .Where(bi => bi.BookId == bookId && bi.IsActive)
                .CountAsync();

            var availableInstances = await _context.BookInstances
                .Where(bi => bi.BookId == bookId && bi.Status == "Доступна" && bi.IsActive)
                .CountAsync();

            var issuedInstances = await _context.BookInstances
                .Where(bi => bi.BookId == bookId && bi.Status == "Выдана" && bi.IsActive)
                .CountAsync();

            var reservedInstances = await _context.BookInstances
                .Where(bi => bi.BookId == bookId && bi.Status == "Зарезервирована" && bi.IsActive)
                .CountAsync();

            return Ok(new
            {
                BookId = bookId,
                TotalInstances = totalInstances,
                AvailableInstances = availableInstances,
                IssuedInstances = issuedInstances,
                ReservedInstances = reservedInstances,
                OtherInstances = totalInstances - availableInstances - issuedInstances - reservedInstances
            });
        }

        // POST: api/BookInstance/recalculate-all-books
        [HttpPost("recalculate-all-books")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> RecalculateAllBooksAvailableCopies()
        {
            var allBooks = await _context.Books.Select(b => b.Id).ToListAsync();
            int updatedBooks = 0;

            foreach (var bookId in allBooks)
            {
                await _allocationService.RecalculateBookAvailableCopies(bookId);
                updatedBooks++;
            }

            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                Message = $"Пересчитано количество доступных копий для {updatedBooks} книг",
                UpdatedBooksCount = updatedBooks
            });
        }

        // POST: api/BookInstance/recalculate/{bookId}
        [HttpPost("recalculate/{bookId}")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<IActionResult> RecalculateBookAvailableCopies(Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound($"Книга с ID {bookId} не найдена");
            }

            await _allocationService.RecalculateBookAvailableCopies(bookId);
            await _context.SaveChangesAsync();

            // Получаем обновленные данные
            var updatedBook = await _context.Books.FindAsync(bookId);

            return Ok(new 
            { 
                Message = "Количество доступных копий пересчитано",
                BookId = bookId,
                AvailableCopies = updatedBook.AvailableCopies
            });
        }

        // GET: api/BookInstance/{id}/reservation
        [HttpGet("{id}/reservation")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<ActionResult> GetBookInstanceReservation(Guid id)
        {
            var instance = await _context.BookInstances
                .Include(bi => bi.Book)
                .FirstOrDefaultAsync(bi => bi.Id == id);

            if (instance == null)
            {
                return NotFound($"Экземпляр книги с ID {id} не найден");
            }

            // Ищем активную резервацию для этого экземпляра
            var activeReservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.BookInstanceId == id && 
                           (r.Status == ReservationStatus.Одобрена || 
                            r.Status == ReservationStatus.Выдана ||
                            r.Status == ReservationStatus.Просрочена))
                .OrderByDescending(r => r.ReservationDate)
                .FirstOrDefaultAsync();

            var result = new
            {
                InstanceId = id,
                InstanceCode = instance.InstanceCode,
                InstanceStatus = instance.Status,
                BookTitle = instance.Book?.Title,
                HasActiveReservation = activeReservation != null,
                ActiveReservation = activeReservation != null ? new
                {
                    Id = activeReservation.Id,
                    Status = activeReservation.Status.ToString(),
                    ReservationDate = activeReservation.ReservationDate,
                    ExpirationDate = activeReservation.ExpirationDate,
                    UserName = activeReservation.User?.FullName,
                    Notes = activeReservation.Notes
                } : null
            };

            return Ok(result);
        }

        // PUT: api/BookInstance/{id}/force-status
        [HttpPut("{id}/force-status")]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> ForceUpdateBookInstanceStatus(Guid id, [FromBody] ForceStatusUpdateDto dto)
        {
            var instance = await _context.BookInstances
                .Include(bi => bi.Book)
                .FirstOrDefaultAsync(bi => bi.Id == id);

            if (instance == null)
            {
                return NotFound($"Экземпляр книги с ID {id} не найден");
            }

            // Проверяем, есть ли активная резервация
            var activeReservation = await _context.Reservations
                .Where(r => r.BookInstanceId == id && 
                           (r.Status == ReservationStatus.Одобрена || 
                            r.Status == ReservationStatus.Выдана ||
                            r.Status == ReservationStatus.Просрочена))
                .FirstOrDefaultAsync();

            if (activeReservation != null && !dto.IgnoreReservation)
            {
                return BadRequest($"Экземпляр связан с активной резервацией (ID: {activeReservation.Id}). " +
                                 "Для принудительного изменения статуса установите IgnoreReservation = true");
            }

            var oldStatus = instance.Status;
            var bookId = instance.BookId;

            instance.Status = dto.NewStatus;
            instance.DateModified = DateTime.UtcNow;
            instance.Notes = string.IsNullOrEmpty(dto.Reason) ? instance.Notes : 
                           $"{instance.Notes} | Принудительное изменение статуса: {dto.Reason}";

            if (dto.NewStatus == "Доступна")
            {
                instance.DateLastChecked = DateTime.UtcNow;
            }

            // Пересчитываем количество доступных копий книги
            await UpdateBookAvailableCopies(bookId);

            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                Message = "Статус экземпляра принудительно изменен",
                InstanceId = id,
                OldStatus = oldStatus,
                NewStatus = dto.NewStatus,
                Reason = dto.Reason,
                HasActiveReservation = activeReservation != null,
                ReservationId = activeReservation?.Id
            });
        }

        // GET: api/BookInstance/status-summary
        [HttpGet("status-summary")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        public async Task<ActionResult> GetInstanceStatusSummary()
        {
            var statusStats = await _context.BookInstances
                .Where(bi => bi.IsActive)
                .GroupBy(bi => bi.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalInstances = await _context.BookInstances.CountAsync(bi => bi.IsActive);

            return Ok(new
            {
                TotalActiveInstances = totalInstances,
                StatusBreakdown = statusStats,
                Summary = new
                {
                    Available = statusStats.FirstOrDefault(s => s.Status == "Доступна")?.Count ?? 0,
                    Issued = statusStats.FirstOrDefault(s => s.Status == "Выдана")?.Count ?? 0,
                    Reserved = statusStats.FirstOrDefault(s => s.Status == "Зарезервирована")?.Count ?? 0,
                    Other = statusStats.Where(s => s.Status != "Доступна" && 
                                                  s.Status != "Выдана" && 
                                                  s.Status != "Зарезервирована")
                                      .Sum(s => s.Count)
                }
            });
        }
    }
} 