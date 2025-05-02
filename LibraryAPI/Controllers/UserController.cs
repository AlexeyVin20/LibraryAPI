using LibraryAPI.Data;
using LibraryAPI.Models.DTOs;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserCreateDto userDto)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
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
                PasswordHash = passwordHash,
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
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto userDto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);
            
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

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
            user.IsActive = userDto.IsActive;
            user.BorrowedBooksCount = userDto.BorrowedBooksCount;
            user.MaxBooksAllowed = userDto.MaxBooksAllowed;
            user.LoanPeriodDays = userDto.LoanPeriodDays;
            user.FineAmount = userDto.FineAmount;
            user.BorrowedBooks = userDto.BorrowedBooks;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Данные были изменены другим процессом. Попробуйте обновить страницу." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ошибка при обновлении: {ex.Message}" });
            }

            return NoContent();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(dto.Id);
            if (user == null)
                return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return BadRequest(new { message = "Старый пароль неверен" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Пароль успешно изменён" });
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

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleCreateDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var role = await _context.Set<Role>().FindAsync(dto.RoleId);
            if (role == null)
                return NotFound(new { message = "Роль не найдена" });

            var existingUserRole = await _context.Set<UserRole>()
                .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);
            
            if (existingUserRole != null)
                return BadRequest(new { message = "Роль уже назначена пользователю" });

            var userRole = new UserRole
            {
                UserId = dto.UserId,
                RoleId = dto.RoleId
            };

            _context.Set<UserRole>().Add(userRole);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Роль успешно добавлена пользователю" });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = $"Ошибка при назначении роли: {ex.InnerException?.Message ?? ex.Message}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Непредвиденная ошибка: {ex.Message}" });
            }
        }

        [HttpPut("update-role")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UserRoleUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var userRole = await _context.Set<UserRole>()
                .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.OldRoleId);
            
            if (userRole == null)
                return NotFound(new { message = "У пользователя нет указанной роли" });

            var newRole = await _context.Set<Role>().FindAsync(dto.NewRoleId);
            if (newRole == null)
                return NotFound(new { message = "Новая роль не найдена" });

            var existingNewRole = await _context.Set<UserRole>()
                .AnyAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.NewRoleId);
            
            if (existingNewRole)
                return BadRequest(new { message = "Эта роль уже назначена пользователю" });

            _context.Set<UserRole>().Remove(userRole);

            var newUserRole = new UserRole
            {
                UserId = dto.UserId,
                RoleId = dto.NewRoleId
            };

            _context.Set<UserRole>().Add(newUserRole);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Роль пользователя успешно обновлена" });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = $"Ошибка при обновлении роли: {ex.InnerException?.Message ?? ex.Message}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Непредвиденная ошибка: {ex.Message}" });
            }
        }

        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveUserRole([FromBody] UserRoleDeleteDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var userRole = await _context.Set<UserRole>()
                .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);
            
            if (userRole == null)
                return NotFound(new { message = "У пользователя нет указанной роли" });

            _context.Set<UserRole>().Remove(userRole);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Роль пользователя успешно удалена" });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = $"Ошибка при удалении роли: {ex.InnerException?.Message ?? ex.Message}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Непредвиденная ошибка: {ex.Message}" });
            }
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            var roles = await _context.Set<Role>().ToListAsync();
            return Ok(roles);
        }

        [HttpGet("{id}/roles")]
        public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUserRoles(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var userRoles = user.UserRoles?.Select(ur => new UserRoleDto
            {
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name
            }).ToList() ?? new List<UserRoleDto>();

            return Ok(userRoles);
        }

        [HttpPost("roles")]
        public async Task<ActionResult<Role>> CreateRole([FromBody] Role role)
        {
            if (await _context.Set<Role>().AnyAsync(r => r.Name == role.Name))
                return Conflict(new { message = "Роль с таким именем уже существует" });

            _context.Set<Role>().Add(role);
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ошибка при создании роли: {ex.Message}" });
            }

            return CreatedAtAction(nameof(GetRoles), new { id = role.Id }, role);
        }
    }
}
