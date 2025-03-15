using LibraryAPI.Data;
using LibraryAPI.Models.DTOs;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public UserController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Username = u.Username,
                IsActive = u.IsActive,
                BorrowedBooksCount = u.BorrowedBooksCount ?? 0,
                MaxBooksAllowed = u.MaxBooksAllowed ?? 5,
                FineAmount = u.FineAmount
            }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return Ok(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Username = user.Username,
                IsActive = user.IsActive,
                BorrowedBooksCount = user.BorrowedBooksCount ?? 0,
                MaxBooksAllowed = user.MaxBooksAllowed ?? 5,
                FineAmount = user.FineAmount
            });
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(UserCreateDto userDto)
        {
            var user = new User
            {
                Id = userDto.Id,
                FullName = userDto.FullName,
                Email = userDto.Email,
                Username = userDto.Username,
                IsActive = userDto.IsActive,
                BorrowedBooksCount = userDto.BorrowedBooksCount,
                MaxBooksAllowed = userDto.MaxBooksAllowed,
                FineAmount = userDto.FineAmount,
                // Следующие поля отсутствуют в DTO, но должны быть заполнены в модели
                DateRegistered = DateTime.UtcNow,
                // Эти поля требуются в модели User, но отсутствуют в DTO
                // В реальном приложении они должны быть добавлены в DTO или получены другим способом
                Phone = "", // Необходимо получить из запроса или установить значение по умолчанию
                DateOfBirth = DateTime.UtcNow, // Значение по умолчанию
                PassportNumber = "", // Необходимо получить из запроса или установить значение по умолчанию
                PassportIssuedBy = "", // Необходимо получить из запроса или установить значение по умолчанию
                PasswordHash = "" // В реальном приложении должен быть хэш пароля
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Username = user.Username,
                IsActive = user.IsActive,
                BorrowedBooksCount = user.BorrowedBooksCount ?? 0,
                MaxBooksAllowed = user.MaxBooksAllowed ?? 5,
                FineAmount = user.FineAmount
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserUpdateDto userDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.FullName = userDto.FullName;
            user.Email = userDto.Email;
            user.Username = userDto.Username;
            user.IsActive = userDto.IsActive;
            user.BorrowedBooksCount = userDto.BorrowedBooksCount;
            user.MaxBooksAllowed = userDto.MaxBooksAllowed;
            user.FineAmount = userDto.FineAmount;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}