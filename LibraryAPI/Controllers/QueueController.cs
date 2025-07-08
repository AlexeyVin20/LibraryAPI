using LibraryAPI.Data;
using LibraryAPI.Models;
using LibraryAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public QueueController(LibraryDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает список резерваций, находящихся в очереди на указанную книгу
        /// </summary>
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetQueueByBook(Guid bookId)
        {
            var queuedReservations = await _context.Reservations
                .Where(r => r.BookId == bookId &&
                       r.Status == ReservationStatus.Обрабатывается &&
                       r.Notes.Contains("В очереди"))
                .Include(r => r.User)
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();

            var result = queuedReservations.Select(r => new ReservationDto
            {
                Id = r.Id,
                UserId = r.UserId,
                BookId = r.BookId,
                ReservationDate = r.ReservationDate,
                ExpirationDate = r.ExpirationDate,
                Status = r.Status.ToString(),
                Notes = r.Notes,
                User = r.User != null ? new UserDto { FullName = r.User.FullName } : null
            });

            return Ok(result);
        }

        /// <summary>
        /// Возвращает список резерваций пользователя, которые находятся в очереди
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetQueueByUser(Guid userId)
        {
            var queuedReservations = await _context.Reservations
                .Where(r => r.UserId == userId &&
                       r.Status == ReservationStatus.Обрабатывается &&
                       r.Notes.Contains("В очереди"))
                .Include(r => r.Book)
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();

            var result = queuedReservations.Select(r => new ReservationDto
            {
                Id = r.Id,
                UserId = r.UserId,
                BookId = r.BookId,
                ReservationDate = r.ReservationDate,
                ExpirationDate = r.ExpirationDate,
                Status = r.Status.ToString(),
                Notes = r.Notes,
                Book = r.Book != null ? new BookDto { Title = r.Book.Title } : null
            });

            return Ok(result);
        }

        /// <summary>
        /// Отменяет резервацию из очереди по её идентификатору
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelQueueReservation(Guid id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            if (reservation.Status != ReservationStatus.Обрабатывается ||
                string.IsNullOrEmpty(reservation.Notes) ||
                !reservation.Notes.Contains("В очереди"))
            {
                return BadRequest("Указанная резервация не находится в очереди");
            }

            reservation.Status = ReservationStatus.Отменена_пользователем;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 