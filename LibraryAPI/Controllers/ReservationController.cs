using LibraryAPI.Data;
using LibraryAPI.Models.DTOs;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public ReservationController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations([FromQuery] string status = null)
        {
            var query = _context.Reservations.Include(r => r.User).Include(r => r.Book).AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(status, out ReservationStatus reservationStatus))
                {
                    query = query.Where(r => r.Status == reservationStatus);
                }
            }
            var reservations = await query.ToListAsync();
            return Ok(reservations.Select(r => new ReservationDto
            {
                Id = r.Id,
                UserId = r.UserId,
                BookId = r.BookId,
                ReservationDate = r.ReservationDate,
                ExpirationDate = r.ExpirationDate,
                ActualReturnDate = r.ActualReturnDate,
                Status = r.Status.ToString(),
                Notes = r.Notes,
                User = r.User != null ? new UserDto { FullName = r.User.FullName } : null,
                Book = r.Book != null ? new BookDto { Title = r.Book.Title } : null
            }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDto>> GetReservation(Guid id)
        {
            var reservation = await _context.Reservations.Include(r => r.User).Include(r => r.Book).FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null)
                return NotFound();

            return Ok(new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                BookId = reservation.BookId,
                ReservationDate = reservation.ReservationDate,
                ExpirationDate = reservation.ExpirationDate,
                ActualReturnDate = reservation.ActualReturnDate,
                Status = reservation.Status.ToString(),
                Notes = reservation.Notes,
                User = reservation.User != null ? new UserDto { FullName = reservation.User.FullName } : null,
                Book = reservation.Book != null ? new BookDto { Title = reservation.Book.Title } : null
            });
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

            // Если книг 0 и статус "Выдана", устанавливаем очередь
            if (book.AvailableCopies <= 0 && status == ReservationStatus.Выдана)
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
                    reservation.Notes = (reservation.Notes ?? "") + " (В очереди, доступна после: " + 
                                      earliestReturn.ExpirationDate.ToString("dd.MM.yyyy") + ")";
                }
                else
                {
                    return BadRequest("Нет доступных копий книги для резервации и нет информации о возвратах");
                }
            }
            // Уменьшаем количество доступных копий только при статусе "Выдана"
            else if (status == ReservationStatus.Выдана)
            {
                if (book.AvailableCopies <= 0)
                {
                    return BadRequest("Нет доступных копий книги для резервации");
                }
                
                book.AvailableCopies--;
            }
            
            _context.Reservations.Add(reservation);
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
                        reservation.Notes = (reservation.Notes ?? "") + " (В очереди, доступна после: " + 
                                          earliestReturn.ExpirationDate.ToString("dd.MM.yyyy") + ")";
                        
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
                            reservation.Notes = (reservation.Notes ?? "") + " (В очереди, доступна после: " + 
                                              earliestReturn.ExpirationDate.ToString("dd.MM.yyyy") + ")";
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