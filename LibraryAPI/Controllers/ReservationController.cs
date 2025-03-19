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

            ReservationStatus status;
            if (!Enum.TryParse(reservationDto.Status, out status))
            {
                return BadRequest("Недопустимый статус резервации");
            }

            reservation.UserId = reservationDto.UserId;
            reservation.BookId = reservationDto.BookId;
            reservation.ReservationDate = reservationDto.ReservationDate;
            reservation.ExpirationDate = reservationDto.ExpirationDate;
            reservation.Status = status;
            reservation.Notes = reservationDto.Notes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(Guid id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}