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
                Phone = u.Phone,
                DateOfBirth = u.DateOfBirth,
                PassportNumber = u.PassportNumber,
                PassportIssuedBy = u.PassportIssuedBy,
                PassportIssuedDate = u.PassportIssuedDate,
                Address = u.Address,
                DateRegistered = u.DateRegistered,
                Username = u.Username,
                PasswordHash = u.PasswordHash,
                IsActive = u.IsActive,
                LastLoginDate = u.LastLoginDate,
                BorrowedBooksCount = u.BorrowedBooksCount ?? 0,
                MaxBooksAllowed = u.MaxBooksAllowed ?? 5,
                LoanPeriodDays = u.LoanPeriodDays,
                FineAmount = u.FineAmount,
                UserRoles = u.UserRoles,
                BorrowedBooks = u.BorrowedBooks
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
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth,
                PassportNumber = user.PassportNumber,
                PassportIssuedBy = user.PassportIssuedBy,
                PassportIssuedDate = user.PassportIssuedDate,
                Address = user.Address,
                DateRegistered = user.DateRegistered,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                BorrowedBooksCount = user.BorrowedBooksCount ?? 0,
                MaxBooksAllowed = user.MaxBooksAllowed ?? 5,
                LoanPeriodDays = user.LoanPeriodDays,
                FineAmount = user.FineAmount,
                UserRoles = user.UserRoles,
                BorrowedBooks = user.BorrowedBooks
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
                Phone = userDto.Phone,
                DateOfBirth = userDto.DateOfBirth,
                PassportNumber = userDto.PassportNumber,
                PassportIssuedBy = userDto.PassportIssuedBy,
                PassportIssuedDate = userDto.PassportIssuedDate,
                Address = userDto.Address,
                DateRegistered = userDto.DateRegistered,
                Username = userDto.Username,
                PasswordHash = userDto.PasswordHash,
                IsActive = userDto.IsActive,
                BorrowedBooksCount = userDto.BorrowedBooksCount,
                MaxBooksAllowed = userDto.MaxBooksAllowed,
                LoanPeriodDays = userDto.LoanPeriodDays,
                FineAmount = userDto.FineAmount,
                UserRoles = userDto.UserRoles,
                BorrowedBooks = userDto.BorrowedBooks
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth,
                PassportNumber = user.PassportNumber,
                PassportIssuedBy = user.PassportIssuedBy,
                PassportIssuedDate = user.PassportIssuedDate,
                Address = user.Address,
                DateRegistered = user.DateRegistered,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                BorrowedBooksCount = user.BorrowedBooksCount ?? 0,
                MaxBooksAllowed = user.MaxBooksAllowed ?? 5,
                LoanPeriodDays = user.LoanPeriodDays,
                FineAmount = user.FineAmount,
                UserRoles = user.UserRoles,
                BorrowedBooks = user.BorrowedBooks
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
            user.Phone = userDto.Phone;
            user.DateOfBirth = userDto.DateOfBirth;
            user.PassportNumber = userDto.PassportNumber;
            user.PassportIssuedBy = userDto.PassportIssuedBy;
            user.PassportIssuedDate = userDto.PassportIssuedDate;
            user.Address = userDto.Address;
            user.DateRegistered = userDto.DateRegistered;
            user.Username = userDto.Username;
            user.PasswordHash = userDto.PasswordHash;
            user.IsActive = userDto.IsActive;
            user.BorrowedBooksCount = userDto.BorrowedBooksCount;
            user.MaxBooksAllowed = userDto.MaxBooksAllowed;
            user.LoanPeriodDays = userDto.LoanPeriodDays;
            user.FineAmount = userDto.FineAmount;
            user.UserRoles = userDto.UserRoles;
            user.BorrowedBooks = userDto.BorrowedBooks;

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
