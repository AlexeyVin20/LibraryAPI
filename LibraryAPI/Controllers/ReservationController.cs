using LibraryAPI.Data;
using LibraryAPI.Models.DTOs;
using LibraryAPI.Models;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly IBookInstanceAllocationService _allocationService;

        public ReservationController(LibraryDbContext context, IBookInstanceAllocationService allocationService)
        {
            _context = context;
            _allocationService = allocationService;
        }

        #region Приватные методы для работы с экземплярами книг

        /// <summary>
        /// Обрабатывает выдачу книги с автоматическим назначением экземпляра
        /// </summary>
        private async Task<(bool Success, string? ErrorMessage, BookInstance? AllocatedInstance)> 
            HandleBookIssuing(Reservation reservation)
        {
            if (reservation.Status != ReservationStatus.Выдана)
            {
                return (true, null, null);
            }

            // Проверяем, есть ли уже назначенный экземпляр (из предыдущего резервирования)
            if (reservation.BookInstanceId.HasValue)
            {
                // Если экземпляр уже назначен, просто обновляем его статус на "Выдана"
                await _allocationService.UpdateBookInstanceStatus(reservation.BookInstanceId, "Выдана");
                var existingInstance = await _context.BookInstances.FindAsync(reservation.BookInstanceId.Value);
                return (true, null, existingInstance);
            }

            // Пытаемся выделить лучший доступный экземпляр (только если не был назначен ранее)
            var allocatedInstance = await _allocationService.AllocateBestAvailableInstance(reservation.BookId);
            
            if (allocatedInstance == null)
            {
                return (false, "Нет доступных экземпляров для выдачи", null);
            }

            // Назначаем экземпляр резервации
            reservation.BookInstanceId = allocatedInstance.Id;
            
            return (true, null, allocatedInstance);
        }

        /// <summary>
        /// Обрабатывает возврат книги с обновлением статуса экземпляра (НЕ удаляет связь)
        /// </summary>
        private async Task<bool> HandleBookReturn(Reservation reservation, ReservationStatus newStatus)
        {
            // Если статус меняется на "Возвращена" и есть назначенный экземпляр
            if (newStatus == ReservationStatus.Возвращена && reservation.BookInstanceId.HasValue)
            {
                // Освобождаем экземпляр, но НЕ удаляем связь для истории
                var released = await _allocationService.ReleaseBookInstance(reservation.BookInstanceId);
                
                // Связь с экземпляром НЕ очищаем для сохранения истории
                return released;
            }

            return false;
        }

        /// <summary>
        /// Обрабатывает резервирование книги с назначением экземпляра
        /// </summary>
        private async Task<(bool Success, string? ErrorMessage, BookInstance? ReservedInstance)>
            HandleBookReservation(Reservation reservation)
        {
            if (reservation.Status != ReservationStatus.Одобрена)
            {
                return (true, null, null);
            }

            // Проверяем, есть ли уже назначенный экземпляр
            if (reservation.BookInstanceId.HasValue)
            {
                // Если экземпляр уже назначен, обновляем его статус на "Зарезервирована"
                await _allocationService.UpdateBookInstanceStatus(reservation.BookInstanceId, "Зарезервирована");
                var existingInstance = await _context.BookInstances.FindAsync(reservation.BookInstanceId.Value);
                return (true, null, existingInstance);
            }

            // Находим лучший доступный экземпляр для резервирования
            var bestAvailable = await _allocationService.FindBestAvailableInstance(reservation.BookId);
            
            if (bestAvailable != null)
            {
                // Назначаем экземпляр резервации и резервируем его
                reservation.BookInstanceId = bestAvailable.Id;
                await _allocationService.ReserveBookInstance(bestAvailable.Id);
                
                return (true, null, bestAvailable);
            }

            return (true, null, null); // Нет доступных экземпляров, но это не ошибка
        }

        /// <summary>
        /// Обрабатывает отмену резервирования с освобождением экземпляра (НЕ удаляет связь)
        /// </summary>
        private async Task<bool> HandleBookReservationCancellation(Reservation reservation, ReservationStatus newStatus)
        {
            // Если статус меняется на отменённый/истёкший и есть назначенный экземпляр
            if ((newStatus == ReservationStatus.Отменена || 
                 newStatus == ReservationStatus.Отменена_пользователем ||
                 newStatus == ReservationStatus.Истекла) && 
                reservation.BookInstanceId.HasValue)
            {
                // Освобождаем экземпляр, но НЕ удаляем связь для сохранения истории
                var released = await _allocationService.UpdateBookInstanceStatus(reservation.BookInstanceId, "Доступна");
                
                // Связь с экземпляром НЕ очищаем для сохранения истории
                return released;
            }

            return false;
        }

        /// <summary>
        /// Обрабатывает просроченные резервации с обновлением статуса экземпляра
        /// </summary>
        private Task<bool> HandleOverdueReservation(Reservation reservation, ReservationStatus newStatus)
        {
            // Если статус меняется на "Просрочена" и есть назначенный экземпляр
            if (newStatus == ReservationStatus.Просрочена && reservation.BookInstanceId.HasValue)
            {
                // У просроченных книг экземпляр остается в статусе "Выдана" до фактического возврата
                // Экземпляр не освобождается автоматически при просрочке
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// Обрабатывает переход резервации в статус "Обрабатывается"
        /// </summary>
        private async Task<bool> HandleReservationProcessing(Reservation reservation, ReservationStatus oldStatus)
        {
            // Если резервация переходит в статус "Обрабатывается" с других статусов
            if (reservation.Status == ReservationStatus.Обрабатывается && reservation.BookInstanceId.HasValue)
            {
                // Если ранее был назначен экземпляр, освобождаем его
                if (oldStatus == ReservationStatus.Одобрена || oldStatus == ReservationStatus.Выдана)
                {
                    await _allocationService.UpdateBookInstanceStatus(reservation.BookInstanceId, "Доступна");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Универсальный метод для обработки изменения статуса резервации
        /// </summary>
        private async Task<(bool Success, string? ErrorMessage)> HandleReservationStatusChange(
            Reservation reservation, ReservationStatus oldStatus, ReservationStatus newStatus)
        {
            // Если статус не изменился, ничего не делаем
            if (oldStatus == newStatus)
            {
                return (true, null);
            }

            // Обрабатываем переходы в зависимости от нового статуса
            switch (newStatus)
            {
                case ReservationStatus.Обрабатывается:
                    await HandleReservationProcessing(reservation, oldStatus);
                    break;

                case ReservationStatus.Одобрена:
                    // Если переходим в "Одобрена" из "Выдана", меняем статус экземпляра на "Зарезервирована"
                    if (oldStatus == ReservationStatus.Выдана && reservation.BookInstanceId.HasValue)
                    {
                        await _allocationService.UpdateBookInstanceStatus(reservation.BookInstanceId, "Зарезервирована");
                    }
                    // Если переходим из других статусов, назначаем и резервируем экземпляр
                    else if (oldStatus != ReservationStatus.Одобрена)
                    {
                        var reservationResult = await HandleBookReservation(reservation);
                        if (!reservationResult.Success)
                        {
                            return (false, reservationResult.ErrorMessage);
                        }
                    }
                    break;

                case ReservationStatus.Выдана:
                    // Если переходим в "Выдана" из "Одобрена", просто меняем статус экземпляра
                    if (oldStatus == ReservationStatus.Одобрена && reservation.BookInstanceId.HasValue)
                    {
                        await _allocationService.UpdateBookInstanceStatus(reservation.BookInstanceId, "Выдана");
                    }
                    // Если переходим из других статусов, назначаем и выдаем экземпляр
                    else if (oldStatus != ReservationStatus.Выдана)
                    {
                        var issuingResult = await HandleBookIssuing(reservation);
                        if (!issuingResult.Success)
                        {
                            return (false, issuingResult.ErrorMessage);
                        }
                    }
                    break;

                case ReservationStatus.Возвращена:
                    await HandleBookReturn(reservation, newStatus);
                    break;

                case ReservationStatus.Просрочена:
                    await HandleOverdueReservation(reservation, newStatus);
                    break;

                case ReservationStatus.Отменена:
                case ReservationStatus.Отменена_пользователем:
                case ReservationStatus.Истекла:
                    await HandleBookReservationCancellation(reservation, newStatus);
                    break;

                default:
                    // Неизвестный статус - не обрабатываем
                    break;
            }

            return (true, null);
        }

        /// <summary>
        /// Создает DTO резервации с информацией об экземпляре
        /// </summary>
        private async Task<ReservationDto> ToReservationDtoWithInstance(Reservation reservation)
        {
            var reservationDto = new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                BookId = reservation.BookId,
                BookInstanceId = reservation.BookInstanceId,
                ReservationDate = reservation.ReservationDate,
                ExpirationDate = reservation.ExpirationDate,
                ActualReturnDate = reservation.ActualReturnDate,
                Status = reservation.Status.ToString(),
                Notes = reservation.Notes,
                User = reservation.User != null ? new UserDto { FullName = reservation.User.FullName } : null,
                Book = reservation.Book != null ? new BookDto { Title = reservation.Book.Title } : null
            };

            // Добавляем информацию об экземпляре, если он назначен
            if (reservation.BookInstanceId.HasValue)
            {
                var bookInstance = await _context.BookInstances
                    .Include(bi => bi.Book)
                    .Include(bi => bi.Shelf)
                    .FirstOrDefaultAsync(bi => bi.Id == reservation.BookInstanceId.Value);

                if (bookInstance != null)
                {
                    reservationDto.BookInstance = new BookInstanceDto
                    {
                        Id = bookInstance.Id,
                        BookId = bookInstance.BookId,
                        InstanceCode = bookInstance.InstanceCode,
                        Status = bookInstance.Status,
                        Condition = bookInstance.Condition,
                        Location = bookInstance.Location,
                        ShelfId = bookInstance.ShelfId,
                        Position = bookInstance.Position,
                        Notes = bookInstance.Notes,
                        Shelf = bookInstance.Shelf != null ? new ShelfDto
                        {
                            Id = bookInstance.Shelf.Id,
                            Category = bookInstance.Shelf.Category,
                            ShelfNumber = bookInstance.Shelf.ShelfNumber
                        } : null
                    };
                }
            }

            return reservationDto;
        }

        #endregion

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations([FromQuery] string status = null)
        {
            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Include(r => r.BookInstance)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(status, out ReservationStatus reservationStatus))
                {
                    query = query.Where(r => r.Status == reservationStatus);
                }
            }
            
            var reservations = await query.ToListAsync();
            var reservationDtos = new List<ReservationDto>();

            foreach (var reservation in reservations)
            {
                var dto = await ToReservationDtoWithInstance(reservation);
                reservationDtos.Add(dto);
            }

            return Ok(reservationDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDto>> GetReservation(Guid id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Include(r => r.BookInstance)
                    .ThenInclude(bi => bi.Shelf)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (reservation == null)
                return NotFound();

            var dto = await ToReservationDtoWithInstance(reservation);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation(ReservationCreateDto reservationDto)
        {
            var user = await _context.Users.FindAsync(reservationDto.UserId);
            var book = await _context.Books.FindAsync(reservationDto.BookId);

            if (user == null || book == null)
                return BadRequest("Пользователь или книга не найдены");

            ReservationStatus status;
            if (!Enum.TryParse(reservationDto.Status, out status))
            {
                status = ReservationStatus.Обрабатывается;
            }

            var reservation = new Reservation
            {
                Id = reservationDto.Id,
                UserId = reservationDto.UserId,
                BookId = reservationDto.BookId,
                ReservationDate = reservationDto.ReservationDate,
                ExpirationDate = reservationDto.ExpirationDate,
                Status = status,
                Notes = reservationDto.Notes
            };

            // Проверяем доступность книг и обрабатываем логику очереди
            if (status == ReservationStatus.Выдана)
            {
                if (book.AvailableCopies <= 0)
                {
                    // Находим самую раннюю дату возврата для данной книги
                    var earliestReturn = await _context.Reservations
                        .Where(r => r.BookId == book.Id && 
                               (r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана))
                        .OrderBy(r => r.ExpirationDate)
                        .FirstOrDefaultAsync();

                    if (earliestReturn != null)
                    {
                        // Устанавливаем дату выдачи на дату возврата самой ранней резервации
                        reservation.ReservationDate = earliestReturn.ExpirationDate;
                        
                        // Обновляем статус на "В очереди"
                        reservation.Status = ReservationStatus.Обрабатывается;
                        string queueNote = " (В очереди, доступна после: " + 
                                          earliestReturn.ExpirationDate.ToString("dd.MM.yyyy") + ")";
                        reservation.Notes = string.IsNullOrEmpty(reservation.Notes) ? 
                                          queueNote.TrimStart() : 
                                          reservation.Notes + queueNote;
                        
                        // Обновляем статус для дальнейшей обработки
                        status = ReservationStatus.Обрабатывается;
                    }
                    else
                    {
                        return BadRequest("Нет доступных копий книги для резервации и нет информации о возвратах");
                    }
                }
                else
                {
                    // Уменьшаем количество доступных копий только если резервация будет выдана
                    book.AvailableCopies--;
                }
            }
            
            _context.Reservations.Add(reservation);

            // Обрабатываем изменения статуса экземпляров с помощью универсального метода
            var statusChangeResult = await HandleReservationStatusChange(reservation, ReservationStatus.Обрабатывается, status);
            if (!statusChangeResult.Success)
            {
                return BadRequest(statusChangeResult.ErrorMessage);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                BookId = reservation.BookId,
                ReservationDate = reservation.ReservationDate,
                ExpirationDate = reservation.ExpirationDate,
                Status = reservation.Status.ToString(),
                Notes = reservation.Notes,
                User = new UserDto { FullName = user.FullName },
                Book = new BookDto { Title = book.Title }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(Guid id, ReservationUpdateDto reservationDto)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            ReservationStatus newStatus;
            if (!Enum.TryParse(reservationDto.Status, out newStatus))
            {
                return BadRequest("Недопустимый статус резервации");
            }
            
            // Получаем старый статус
            var oldStatus = reservation.Status;
            var book = await _context.Books.FindAsync(reservation.BookId);
            
            if (book == null)
                return BadRequest("Книга не найдена");

            // Сразу применяем новый статус к сущности, чтобы универсальный обработчик видел актуальное состояние
            reservation.Status = newStatus;

            // Обрабатываем изменения статуса экземпляров с помощью универсального метода
            var statusChangeResult = await HandleReservationStatusChange(reservation, oldStatus, newStatus);
            if (!statusChangeResult.Success)
            {
                return BadRequest(statusChangeResult.ErrorMessage);
            }

            // Проверяем, изменился ли статус на "Выдана"
            bool wasIssued = (oldStatus == ReservationStatus.Выдана);
            bool isIssued = (newStatus == ReservationStatus.Выдана);
            
            // Если статус изменился с не "Выдана" на "Выдана"
            if (!wasIssued && isIssued)
            {
                if (book.AvailableCopies <= 0)
                {
                    // Если книг нет в наличии, обрабатываем очередь
                    var earliestReturn = await _context.Reservations
                        .Where(r => r.BookId == book.Id && 
                               (r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана))
                        .OrderBy(r => r.ExpirationDate)
                        .FirstOrDefaultAsync();

                    if (earliestReturn != null)
                    {
                        // Устанавливаем дату выдачи на дату возврата самой ранней резервации
                        reservation.ReservationDate = earliestReturn.ExpirationDate;
                        
                        // Обновляем статус и добавляем пометку
                        reservation.Status = ReservationStatus.Обрабатывается;
                        string queueNote = " (В очереди, доступна после: " + 
                                          earliestReturn.ExpirationDate.ToString("dd.MM.yyyy") + ")";
                        reservation.Notes = string.IsNullOrEmpty(reservation.Notes) ? 
                                          queueNote.TrimStart() : 
                                          reservation.Notes + queueNote;
                        
                        // Завершаем обработку без уменьшения количества копий
                        await _context.SaveChangesAsync();
                        return NoContent();
                    }
                    
                    return BadRequest("Нет доступных копий книги для резервации");
                }
                
                // Уменьшаем количество доступных копий
                book.AvailableCopies--;
            }
            // Если статус изменился с "Выдана" на не "Выдана"
            else if (wasIssued && !isIssued)
            {
                // Увеличиваем количество доступных копий (возврат)
                book.AvailableCopies++;
                
                // Проверяем, есть ли ожидающие в очереди на эту книгу
                var nextInQueue = await _context.Reservations
                    .Where(r => r.BookId == book.Id && 
                           r.Status == ReservationStatus.Обрабатывается && 
                           r.Notes.Contains("В очереди"))
                    .OrderBy(r => r.ReservationDate)
                    .FirstOrDefaultAsync();
                
                if (nextInQueue != null)
                {
                    // Изменяем статус следующей резервации в очереди на "Одобрена"
                    nextInQueue.Status = ReservationStatus.Одобрена;
                    
                    // Удаляем пометку о нахождении в очереди
                    if (nextInQueue.Notes.Contains("(В очереди, доступна после:"))
                    {
                        int startIndex = nextInQueue.Notes.IndexOf("(В очереди, доступна после:");
                        nextInQueue.Notes = nextInQueue.Notes.Substring(0, startIndex).Trim();
                    }
                    
                    // Уменьшаем количество доступных копий снова, так как книга идёт следующему в очереди
                    book.AvailableCopies--;
                }
            }

            // Если меняется книга для резервации и старый статус был "Выдана"
            if (reservationDto.BookId != reservation.BookId && wasIssued)
            {
                // Возвращаем копию старой книги
                book.AvailableCopies++;
                
                // Получаем новую книгу
                var newBook = await _context.Books.FindAsync(reservationDto.BookId);
                if (newBook == null)
                    return BadRequest("Новая книга не найдена");
                
                if (isIssued)
                {
                    // Проверяем наличие новой книги
                    if (newBook.AvailableCopies <= 0)
                    {
                        // Если нет доступных копий, обрабатываем очередь
                        var earliestReturn = await _context.Reservations
                            .Where(r => r.BookId == newBook.Id && 
                                  (r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана))
                            .OrderBy(r => r.ExpirationDate)
                            .FirstOrDefaultAsync();

                        if (earliestReturn != null)
                        {
                            // Устанавливаем дату выдачи на дату возврата самой ранней резервации
                            reservation.ReservationDate = earliestReturn.ExpirationDate;
                            
                            // Обновляем статус на "В очереди"
                            reservation.Status = ReservationStatus.Обрабатывается;
                            string queueNote = " (В очереди, доступна после: " + 
                                              earliestReturn.ExpirationDate.ToString("dd.MM.yyyy") + ")";
                            reservation.Notes = string.IsNullOrEmpty(reservation.Notes) ? 
                                              queueNote.TrimStart() : 
                                              reservation.Notes + queueNote;
                        }
                        else
                        {
                            return BadRequest("Нет доступных копий новой книги для резервации");
                        }
                    }
                    else
                    {
                        // Уменьшаем количество копий новой книги
                        newBook.AvailableCopies--;
                    }
                }
            }
            
            reservation.UserId = reservationDto.UserId;
            reservation.BookId = reservationDto.BookId;
            reservation.ReservationDate = reservationDto.ReservationDate;
            reservation.ExpirationDate = reservationDto.ExpirationDate;
            
            // Сохраняем новый статус только если мы его не меняли в процессе обработки очереди
            if (reservation.Status != ReservationStatus.Обрабатывается || newStatus != ReservationStatus.Выдана)
            {
                reservation.Status = newStatus;
                
                // Устанавливаем дату фактического возврата при изменении статуса на "Возвращена"
                if (newStatus == ReservationStatus.Возвращена && oldStatus != ReservationStatus.Возвращена)
                {
                    reservation.ActualReturnDate = DateTime.UtcNow;
                }
                // Сбрасываем дату возврата если статус изменился с "Возвращена" на другой
                else if (oldStatus == ReservationStatus.Возвращена && newStatus != ReservationStatus.Возвращена)
                {
                    reservation.ActualReturnDate = null;
                }
            }
            
            reservation.Notes = reservationDto.Notes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(Guid id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (reservation == null)
                return NotFound();

            // Освобождаем экземпляр при удалении резервации (но сохраняем связь для истории)
            if (reservation.BookInstanceId.HasValue)
            {
                // Для всех статусов с назначенным экземпляром освобождаем его
                if (reservation.Status == ReservationStatus.Одобрена || 
                    reservation.Status == ReservationStatus.Выдана ||
                    reservation.Status == ReservationStatus.Просрочена)
                {
                    await _allocationService.UpdateBookInstanceStatus(reservation.BookInstanceId, "Доступна");
                }
            }

            // Если удаляемая резервация имела статус "Одобрена" или "Выдана", 
            // увеличиваем количество доступных копий
            if (reservation.Status == ReservationStatus.Одобрена || 
                reservation.Status == ReservationStatus.Выдана)
            {
                var book = reservation.Book;
                if (book != null)
                {
                    book.AvailableCopies++;
                    
                    // Проверяем, есть ли ожидающие в очереди на эту книгу
                    var nextInQueue = await _context.Reservations
                        .Where(r => r.BookId == book.Id && 
                               r.Status == ReservationStatus.Обрабатывается && 
                               r.Notes.Contains("В очереди"))
                        .OrderBy(r => r.ReservationDate)
                        .FirstOrDefaultAsync();
                    
                    if (nextInQueue != null)
                    {
                        // Изменяем статус следующей резервации в очереди на "Одобрена"
                        nextInQueue.Status = ReservationStatus.Одобрена;
                        
                        // Удаляем пометку о нахождении в очереди
                        if (nextInQueue.Notes.Contains("(В очереди, доступна после:"))
                        {
                            int startIndex = nextInQueue.Notes.IndexOf("(В очереди, доступна после:");
                            nextInQueue.Notes = nextInQueue.Notes.Substring(0, startIndex).Trim();
                        }
                        
                        // Уменьшаем количество доступных копий снова, так как книга идёт следующему в очереди
                        book.AvailableCopies--;
                    }
                }
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetReservationDatesByBookId(Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return NotFound("Книга не найдена");

            var reservationDates = await _context.Reservations
                .Where(r => r.BookId == bookId && 
                       (r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана))
                .Select(r => new { r.ReservationDate, r.ExpirationDate })
                .ToListAsync();

            var result = new
            {
                BookId = bookId,
                BookTitle = book.Title,
                ReservationPeriods = reservationDates.Select(d => new 
                {
                    StartDate = d.ReservationDate,
                    EndDate = d.ExpirationDate
                })
            };

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservationsByUserId(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Book)
                .Include(r => r.User)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    BookId = r.BookId,
                    ReservationDate = r.ReservationDate,
                    ExpirationDate = r.ExpirationDate,
                    Status = r.Status.ToString(),
                    Notes = r.Notes,
                    User = r.User != null ? new UserDto { FullName = r.User.FullName } : null,
                    Book = r.Book != null ? new BookDto { Title = r.Book.Title } : null
                })
                .ToListAsync();

            if (!reservations.Any())
            {
                return NotFound("Резервации для данного пользователя не найдены");
            }

            return Ok(reservations);
        }
    }
}