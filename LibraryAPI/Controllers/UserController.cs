using LibraryAPI.Data;
using LibraryAPI.Models.DTOs;
using LibraryAPI.Models;
using LibraryAPI.Services;
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
        private readonly INotificationService _notificationService;

        public UserController(LibraryDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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

        [HttpPost("assign-roles")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto dto)
        {
            var role = await _context.Set<Role>().FindAsync(dto.RoleId);
            if (role == null)
                return NotFound(new { message = "Роль не найдена" });

            var users = await _context.Users.Where(u => dto.UserIds.Contains(u.Id)).ToListAsync();
            var notFoundIds = dto.UserIds.Except(users.Select(u => u.Id)).ToList();
            if (notFoundIds.Any())
                return NotFound(new { message = $"Пользователь(и) не найдены: {string.Join(", ", notFoundIds)}" });

            var existingUserRoles = await _context.Set<UserRole>()
                .Where(ur => dto.UserIds.Contains(ur.UserId) && ur.RoleId == dto.RoleId)
                .ToListAsync();

            var alreadyAssignedUserIds = existingUserRoles.Select(ur => ur.UserId).ToHashSet();

            var newUserRoles = dto.UserIds
                .Where(id => !alreadyAssignedUserIds.Contains(id))
                .Select(id => new UserRole { UserId = id, RoleId = dto.RoleId })
                .ToList();

            _context.Set<UserRole>().AddRange(newUserRoles);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Роль успешно назначена выбранным пользователям" });
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
        public async Task<ActionResult<IEnumerable<object>>> GetRoles()
        {
            var rolesWithCounts = await _context.Set<Role>()
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    // Если у модели Role есть другие свойства, которые должны быть возвращены,
                    // их также следует включить здесь.
                    UsersCount = _context.Set<UserRole>().Count(ur => ur.RoleId == r.Id)
                })
                .ToListAsync();

            return Ok(rolesWithCounts);
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

        [HttpPut("roles/{id:int}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role roleUpdateData)
        {
            if (roleUpdateData == null || string.IsNullOrWhiteSpace(roleUpdateData.Name))
            {
                return BadRequest(new { message = "Имя роли не может быть пустым." });
            }

            var role = await _context.Set<Role>().FindAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Роль не найдена" });
            }

            // Проверка, существует ли другая роль с таким же именем
            var existingRoleWithNewName = await _context.Set<Role>()
                .FirstOrDefaultAsync(r => r.Name == roleUpdateData.Name && r.Id != id);

            if (existingRoleWithNewName != null)
            {
                return Conflict(new { message = "Роль с таким именем уже существует" });
            }

            role.Name = roleUpdateData.Name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Эта ошибка может возникнуть, если данные были изменены другим процессом
                // между загрузкой и попыткой сохранения.
                return Conflict(new { message = "Данные роли были изменены другим процессом. Попробуйте обновить и повторить попытку." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = $"Ошибка при обновлении роли: {ex.InnerException?.Message ?? ex.Message}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Непредвиденная ошибка при обновлении роли: {ex.Message}" });
            }

            return NoContent();
        }

        [HttpDelete("roles/{id:int}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Set<Role>().FindAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Роль не найдена" });
            }

            // Проверка, используется ли роль какими-либо пользователями
            var isRoleInUse = await _context.Set<UserRole>().AnyAsync(ur => ur.RoleId == id);
            if (isRoleInUse)
            {
                return BadRequest(new { message = "Эта роль назначена одному или нескольким пользователям и не может быть удалена." });
            }

            _context.Set<Role>().Remove(role);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Обработка специфических ошибок базы данных, если это необходимо
                return BadRequest(new { message = $"Ошибка при удалении роли: {ex.InnerException?.Message ?? ex.Message}" });
            }
            catch (Exception ex)
            {
                // Общая обработка ошибок
                return BadRequest(new { message = $"Непредвиденная ошибка при удалении роли: {ex.Message}" });
            }

            return NoContent();
        }

        [HttpPost("{id}/update-borrowed-count")]
        public async Task<IActionResult> UpdateBorrowedBooksCount(Guid id, [FromBody] UserUpdateBorrowedCountDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            user.BorrowedBooksCount = dto.BorrowedBooksCount;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Счетчик книг успешно обновлен", borrowedBooksCount = user.BorrowedBooksCount });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Данные были изменены другим процессом. Попробуйте обновить страницу." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ошибка при обновлении счетчика книг: {ex.Message}" });
            }
        }

        [HttpGet("{id}/favorites")]
        public async Task<ActionResult<IEnumerable<FavoriteBookDto>>> GetFavoriteBooks(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var favorites = await _context.FavoriteBooks
                .Where(fb => fb.UserId == id)
                .Include(fb => fb.Book)
                .ToListAsync();

            return Ok(favorites.Select(fb => new FavoriteBookDto
            {
                UserId = fb.UserId,
                BookId = fb.BookId,
                BookTitle = fb.Book.Title,
                BookAuthors = fb.Book.Authors,
                BookCover = fb.Book.Cover,
                DateAdded = fb.DateAdded
            }));
        }

        [HttpPost("{id}/favorites")]
        public async Task<IActionResult> AddFavoriteBook(Guid id, [FromBody] FavoriteBookCreateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null)
                return NotFound(new { message = "Книга не найдена" });

            var existingFavorite = await _context.FavoriteBooks
                .FirstOrDefaultAsync(fb => fb.UserId == id && fb.BookId == dto.BookId);

            if (existingFavorite != null)
                return BadRequest(new { message = "Эта книга уже в избранном" });

            var favoriteBook = new FavoriteBook
            {
                UserId = id,
                BookId = dto.BookId,
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

        [HttpDelete("{id}/favorites/{bookId}")]
        public async Task<IActionResult> RemoveFavoriteBook(Guid id, Guid bookId)
        {
            var favoriteBook = await _context.FavoriteBooks
                .FirstOrDefaultAsync(fb => fb.UserId == id && fb.BookId == bookId);

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

        [HttpGet("users-with-roles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.Email,
                    Roles = u.UserRoles.Select(ur => new { ur.Role.Id, ur.Role.Name })
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("remove-roles")]
        public async Task<IActionResult> RemoveRoles([FromBody] RemoveRolesDto dto)
        {
            var userRoles = await _context.Set<UserRole>()
                .Where(ur => dto.UserIds.Contains(ur.UserId) && ur.RoleId == dto.RoleId)
                .ToListAsync();

            if (!userRoles.Any())
                return NotFound(new { message = "Указанные пользователи не имеют данной роли" });

            _context.Set<UserRole>().RemoveRange(userRoles);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Роль успешно удалена у выбранных пользователей" });
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

        [HttpGet("with-books")]
        public async Task<IActionResult> GetUsersWithBooks()
        {
            // Оптимизированный запрос: сначала получаем пользователей с активными резервированиями
            var userIds = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана)
                .Select(r => r.UserId)
                .Distinct()
                .ToListAsync();

            if (!userIds.Any())
            {
                return Ok(new { TotalUsers = 0, TotalBorrowedBooks = 0, Users = new List<object>() });
            }

            // Получаем пользователей
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            // Получаем все активные резервирования для этих пользователей
            var activeReservations = await _context.Reservations
                .Where(r => userIds.Contains(r.UserId) && 
                    (r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана))
                .Include(r => r.Book)
                .ToListAsync();

            // Группируем резервирования по пользователям
            var reservationsByUser = activeReservations.GroupBy(r => r.UserId).ToDictionary(g => g.Key, g => g.ToList());

            var usersWithBooks = users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Username,
                u.Email,
                u.Phone,
                BorrowedBooksCount = reservationsByUser.ContainsKey(u.Id) ? reservationsByUser[u.Id].Count : 0,
                MaxBooksAllowed = u.MaxBooksAllowed ?? 5,
                LoanPeriodDays = u.LoanPeriodDays,
                ActiveReservations = reservationsByUser.ContainsKey(u.Id) 
                    ? reservationsByUser[u.Id].Select(r => (object)new
                    {
                        r.Id,
                        r.BookId,
                        r.Book.Cover,
                        BookTitle = r.Book.Title,
                        BookAuthors = r.Book.Authors,
                        BookISBN = r.Book.ISBN,
                        BorrowDate = r.ReservationDate,
                        DueDate = r.ExpirationDate,
                        ReturnDate = r.ActualReturnDate,
                        DaysOverdue = r.ExpirationDate < DateTime.UtcNow 
                            ? (int)(DateTime.UtcNow - r.ExpirationDate).TotalDays 
                            : 0,
                        IsOverdue = r.ExpirationDate < DateTime.UtcNow,
                        Status = r.Status.ToString(),
                        ReservationStatus = r.Status.ToString(),
                        r.Notes
                    }).ToList()
                    : new List<object>()
            }).ToList();

            // Автоматическая отправка напоминаний пользователям с книгами, которые скоро нужно вернуть
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendDueRemindersToUsersWithBooksAsync();
                }
                catch (Exception ex)
                {
                    // Логирование ошибки без прерывания основного запроса
                }
            });

            var result = new
            {
                TotalUsers = usersWithBooks.Count,
                TotalBorrowedBooks = usersWithBooks.Sum(u => u.BorrowedBooksCount),
                Users = usersWithBooks
            };

            return Ok(result);
        }

        [HttpGet("with-fines")]
        public async Task<IActionResult> GetUsersWithFines()
        {
            // Получаем пользователей со штрафами
            var usersWithFineAmount = await _context.Users
                .Where(u => u.FineAmount > 0)
                .ToListAsync();

            // Получаем пользователей с просроченными резервированиями
            var userIdsWithOverdueReservations = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Просрочена || 
                    ((r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана) && 
                     r.ExpirationDate < DateTime.UtcNow))
                .Select(r => r.UserId)
                .Distinct()
                .ToListAsync();

            // Объединяем списки пользователей
            var allUserIds = usersWithFineAmount.Select(u => u.Id)
                .Union(userIdsWithOverdueReservations)
                .Distinct()
                .ToList();

            if (!allUserIds.Any())
            {
                return Ok(new { TotalUsersWithFines = 0, TotalFineAmount = 0, TotalOverdueBooks = 0, Users = new List<object>() });
            }

            // Получаем всех пользователей
            var allUsers = await _context.Users
                .Where(u => allUserIds.Contains(u.Id))
                .ToListAsync();

            // Получаем все резервирования для этих пользователей
            var allReservations = await _context.Reservations
                .Where(r => allUserIds.Contains(r.UserId))
                .Include(r => r.Book)
                .ToListAsync();

            // Группируем резервирования по пользователям
            var reservationsByUser = allReservations.GroupBy(r => r.UserId).ToDictionary(g => g.Key, g => g.ToList());

            var usersWithFines = allUsers.Select(u => 
            {
                var userReservations = reservationsByUser.ContainsKey(u.Id) ? reservationsByUser[u.Id] : new List<Reservation>();
                
                var overdueReservations = userReservations
                    .Where(r => r.Status == ReservationStatus.Просрочена || 
                        ((r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана) && 
                         r.ExpirationDate < DateTime.UtcNow))
                    .ToList();

                var activeReservations = userReservations
                    .Where(r => r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана || 
                               r.Status == ReservationStatus.Просрочена)
                    .ToList();

                return new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.Email,
                    u.Phone,
                    FineAmount = u.FineAmount,
                    BorrowedBooksCount = activeReservations.Count,
                    OverdueBooks = overdueReservations.Select(r => new
                    {
                        ReservationId = r.Id,
                        r.BookId,
                        r.Book.Cover,
                        BookTitle = r.Book.Title,
                        BookAuthors = r.Book.Authors,
                        BookISBN = r.Book.ISBN,
                        BorrowDate = r.ReservationDate,
                        DueDate = r.ExpirationDate,
                        DaysOverdue = (int)(DateTime.UtcNow - r.ExpirationDate).TotalDays,
                        EstimatedFine = Math.Max(0, (int)(DateTime.UtcNow - r.ExpirationDate).TotalDays * 10), // 10 рублей за день
                        ReservationStatus = r.Status.ToString(),
                        r.Notes
                    }).ToList(),
                    AllReservations = userReservations.Select(r => new
                    {
                        ReservationId = r.Id,
                        BookTitle = r.Book.Title,
                        BorrowDate = r.ReservationDate,
                        DueDate = r.ExpirationDate,
                        ReturnDate = r.ActualReturnDate,
                        IsOverdue = (r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана) && 
                                    r.ExpirationDate < DateTime.UtcNow,
                        Status = r.Status.ToString(),
                        r.Notes
                    }).ToList()
                };
            }).ToList();

            // Автоматическая отправка уведомлений о штрафах и просроченных книгах
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendFineNotificationsToUsersWithFinesAsync();
                    await _notificationService.SendOverdueNotificationsToUsersWithBooksAsync();
                }
                catch (Exception ex)
                {
                    // Логирование ошибки без прерывания основного запроса
                }
            });

            var result = new
            {
                TotalUsersWithFines = usersWithFines.Count,
                TotalFineAmount = usersWithFines.Sum(u => u.FineAmount),
                TotalOverdueBooks = usersWithFines.Sum(u => u.OverdueBooks.Count),
                Users = usersWithFines
            };

            return Ok(result);
        }

        [HttpGet("{id}/reservations")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetUserReservations(Guid id, [FromQuery] string? status = null)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Пользователь не найден");

            var query = _context.Reservations
                .Where(r => r.UserId == id)
                .Include(r => r.Book)
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<ReservationStatus>(status, out var reservationStatus))
                {
                    query = query.Where(r => r.Status == reservationStatus);
                }
            }

            var reservations = await query
                .OrderByDescending(r => r.ReservationDate)
                .Select(r => new ReservationDto
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
                    Book = r.Book != null ? new BookDto { 
                        Id = r.Book.Id,
                        Title = r.Book.Title,
                        Authors = r.Book.Authors,
                        ISBN = r.Book.ISBN,
                        Cover = r.Book.Cover
                    } : null
                })
                .ToListAsync();

            return Ok(reservations);
        }

        [HttpGet("{id}/active-reservations")]
        public async Task<ActionResult<object>> GetUserActiveReservations(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Пользователь не найден");

            var activeReservations = await _context.Reservations
                .Where(r => r.UserId == id && 
                    (r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана))
                .Include(r => r.Book)
                .Select(r => new
                {
                    r.Id,
                    r.BookId,
                    BookTitle = r.Book.Title,
                    BookAuthors = r.Book.Authors,
                    BookISBN = r.Book.ISBN,
                    BookCover = r.Book.Cover,
                    ReservationDate = r.ReservationDate,
                    ExpirationDate = r.ExpirationDate,
                    Status = r.Status.ToString(),
                    DaysRemaining = (int)(r.ExpirationDate - DateTime.UtcNow).TotalDays,
                    IsOverdue = r.ExpirationDate < DateTime.UtcNow,
                    DaysOverdue = r.ExpirationDate < DateTime.UtcNow ? 
                        (int)(DateTime.UtcNow - r.ExpirationDate).TotalDays : 0,
                    r.Notes
                })
                .ToListAsync();

            var overdueCount = activeReservations.Count(r => r.IsOverdue);
            var totalFine = overdueCount * 10; // Примерный расчет штрафа

            return Ok(new
            {
                UserId = id,
                UserName = user.FullName,
                TotalActiveReservations = activeReservations.Count,
                OverdueReservations = overdueCount,
                EstimatedTotalFine = totalFine,
                MaxBooksAllowed = user.MaxBooksAllowed ?? 5,
                CanBorrowMore = activeReservations.Count < (user.MaxBooksAllowed ?? 5),
                Reservations = activeReservations
            });
        }

        [HttpGet("{id}/overdue-reservations")]
        public async Task<ActionResult<object>> GetUserOverdueReservations(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Пользователь не найден");

            var overdueReservations = await _context.Reservations
                .Where(r => r.UserId == id && 
                    ((r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана) && 
                     r.ExpirationDate < DateTime.UtcNow) || r.Status == ReservationStatus.Просрочена)
                .Include(r => r.Book)
                .Select(r => new
                {
                    r.Id,
                    r.BookId,
                    BookTitle = r.Book.Title,
                    BookAuthors = r.Book.Authors,
                    BookISBN = r.Book.ISBN,
                    BookCover = r.Book.Cover,
                    ReservationDate = r.ReservationDate,
                    ExpirationDate = r.ExpirationDate,
                    Status = r.Status.ToString(),
                    DaysOverdue = (int)(DateTime.UtcNow - r.ExpirationDate).TotalDays,
                    EstimatedFine = Math.Max(0, (int)(DateTime.UtcNow - r.ExpirationDate).TotalDays * 10),
                    r.Notes
                })
                .ToListAsync();

            var totalFine = overdueReservations.Sum(r => r.EstimatedFine);

            return Ok(new
            {
                UserId = id,
                UserName = user.FullName,
                TotalOverdueReservations = overdueReservations.Count,
                TotalEstimatedFine = totalFine,
                CurrentFineAmount = user.FineAmount,
                OverdueReservations = overdueReservations
            });
        }

        [HttpPost("{id}/extend-reservation/{reservationId}")]
        public async Task<IActionResult> ExtendReservation(Guid id, Guid reservationId, [FromBody] ExtendReservationDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Пользователь не найден");

            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == id);

            if (reservation == null)
                return NotFound("Резервирование не найдено");

            if (reservation.Status != ReservationStatus.Одобрена && reservation.Status != ReservationStatus.Выдана)
                return BadRequest("Можно продлить только активные резервирования");

            if (reservation.ExpirationDate < DateTime.UtcNow)
                return BadRequest("Нельзя продлить просроченное резервирование");

            // Проверяем, есть ли другие резервирования на эту книгу в очереди
            var hasQueuedReservations = await _context.Reservations
                .AnyAsync(r => r.BookId == reservation.BookId && 
                    r.Status == ReservationStatus.Обрабатывается && 
                    r.Notes.Contains("В очереди"));

            if (hasQueuedReservations)
                return BadRequest("Нельзя продлить резервирование, так как есть другие пользователи в очереди на эту книгу");

            // Продлеваем на указанное количество дней (по умолчанию на срок займа пользователя)
            var extensionDays = dto.ExtensionDays > 0 ? dto.ExtensionDays : user.LoanPeriodDays;
            reservation.ExpirationDate = reservation.ExpirationDate.AddDays(extensionDays);
            
            if (!string.IsNullOrEmpty(dto.Notes))
            {
                reservation.Notes += $" Продлено на {extensionDays} дней: {dto.Notes}";
            }
            else
            {
                reservation.Notes += $" Продлено на {extensionDays} дней";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Резервирование успешно продлено на {extensionDays} дней",
                newExpirationDate = reservation.ExpirationDate,
                extensionDays = extensionDays
            });
        }

        [HttpPost("{id}/fine")]
        public async Task<IActionResult> AddFineToUser(Guid id, [FromBody] UserFineCreateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            // Проверяем валидность данных
            if (dto.Amount <= 0)
                return BadRequest(new { message = "Сумма штрафа должна быть больше нуля" });

            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { message = "Причина штрафа обязательна" });

            Reservation? reservation = null;
            if (dto.ReservationId.HasValue)
            {
                reservation = await _context.Reservations
                    .Include(r => r.Book)
                    .FirstOrDefaultAsync(r => r.Id == dto.ReservationId.Value);

                if (reservation == null)
                    return NotFound(new { message = "Резервирование не найдено" });

                if (reservation.UserId != id)
                    return BadRequest(new { message = "Резервирование не принадлежит указанному пользователю" });
            }

            // Проверяем, не был ли уже начислен штраф за эти дни для данного резервирования
            var calculatedForDate = DateTime.UtcNow.Date;
            if (dto.ReservationId.HasValue && dto.OverdueDays.HasValue)
            {
                var existingFine = await _context.FineRecords
                    .FirstOrDefaultAsync(fr => 
                        fr.ReservationId == dto.ReservationId.Value &&
                        fr.CalculatedForDate == calculatedForDate &&
                        fr.FineType == (dto.FineType ?? "Overdue"));

                if (existingFine != null)
                    return BadRequest(new { 
                        message = "Штраф за эти дни уже был начислен для данного резервирования",
                        existingFineId = existingFine.Id,
                        existingFineAmount = existingFine.Amount,
                        calculatedDate = existingFine.CalculatedForDate
                    });
            }

            // Создаем запись о штрафе
            var fineRecord = new FineRecord
            {
                UserId = id,
                ReservationId = dto.ReservationId,
                Amount = dto.Amount,
                Reason = dto.Reason,
                OverdueDays = dto.OverdueDays,
                Notes = dto.Notes,
                FineType = dto.FineType ?? "Overdue",
                CalculatedForDate = calculatedForDate
            };

            _context.FineRecords.Add(fineRecord);

            // Обновляем общую сумму штрафа пользователя
            user.FineAmount += dto.Amount;

            try
            {
                await _context.SaveChangesAsync();

                // Отправляем уведомление о штрафе
                var fineNotificationDto = new FineNotificationDto
                {
                    UserId = id,
                    FineAmount = user.FineAmount,
                    PreviousFineAmount = user.FineAmount - dto.Amount,
                    Reason = dto.Reason,
                    OverdueBooks = reservation != null ? new List<OverdueBookDto>
                    {
                        new OverdueBookDto
                        {
                            BookId = reservation.BookId,
                            BookTitle = reservation.Book.Title,
                            BookAuthors = reservation.Book.Authors,
                            DueDate = reservation.ExpirationDate,
                            DaysOverdue = dto.OverdueDays ?? 0
                        }
                    } : new List<OverdueBookDto>()
                };

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _notificationService.SendFineNotificationAsync(fineNotificationDto);
                    }
                    catch (Exception ex)
                    {
                        // Логирование ошибки без прерывания основного процесса
                    }
                });

                return Ok(new
                {
                    message = "Штраф успешно начислен",
                    fineId = fineRecord.Id,
                    amount = dto.Amount,
                    totalFineAmount = user.FineAmount,
                    reason = dto.Reason,
                    overdueDays = dto.OverdueDays,
                    calculatedForDate = calculatedForDate,
                    reservationId = dto.ReservationId,
                    bookTitle = reservation?.Book?.Title
                });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = $"Ошибка при начислении штрафа: {ex.InnerException?.Message ?? ex.Message}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Непредвиденная ошибка: {ex.Message}" });
            }
        }

        [HttpGet("{id}/fines")]
        public async Task<ActionResult<UserFineHistoryDto>> GetUserFines(Guid id, [FromQuery] bool? isPaid = null)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var query = _context.FineRecords
                .Where(fr => fr.UserId == id)
                .Include(fr => fr.Reservation)
                .ThenInclude(r => r.Book)
                .AsQueryable();

            if (isPaid.HasValue)
            {
                query = query.Where(fr => fr.IsPaid == isPaid.Value);
            }

            var fineRecords = await query
                .OrderByDescending(fr => fr.CreatedAt)
                .Select(fr => new FineRecordDto
                {
                    Id = fr.Id,
                    UserId = fr.UserId,
                    ReservationId = fr.ReservationId,
                    Amount = fr.Amount,
                    Reason = fr.Reason,
                    OverdueDays = fr.OverdueDays,
                    CreatedAt = fr.CreatedAt,
                    PaidAt = fr.PaidAt,
                    IsPaid = fr.IsPaid,
                    Notes = fr.Notes,
                    CalculatedForDate = fr.CalculatedForDate,
                    FineType = fr.FineType,
                    UserName = user.FullName,
                    BookTitle = fr.Reservation != null ? fr.Reservation.Book.Title : null,
                    ReservationStatus = fr.Reservation != null ? fr.Reservation.Status.ToString() : null
                })
                .ToListAsync();

            var totalAmount = fineRecords.Sum(fr => fr.Amount);
            var paidAmount = fineRecords.Where(fr => fr.IsPaid).Sum(fr => fr.Amount);
            var unpaidAmount = totalAmount - paidAmount;

            var result = new UserFineHistoryDto
            {
                UserId = id,
                UserName = user.FullName,
                TotalFineAmount = totalAmount,
                PaidAmount = paidAmount,
                UnpaidAmount = unpaidAmount,
                TotalFines = fineRecords.Count,
                PaidFines = fineRecords.Count(fr => fr.IsPaid),
                UnpaidFines = fineRecords.Count(fr => !fr.IsPaid),
                FineRecords = fineRecords
            };

            return Ok(result);
        }

        [HttpPost("{id}/pay-fine")]
        public async Task<IActionResult> PayFine(Guid id, [FromBody] UserFinePaymentDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var fineRecord = await _context.FineRecords
                .FirstOrDefaultAsync(fr => fr.Id == dto.FineId && fr.UserId == id);

            if (fineRecord == null)
                return NotFound(new { message = "Штраф не найден" });

            if (fineRecord.IsPaid)
                return BadRequest(new { message = "Штраф уже оплачен" });

            // Отмечаем штраф как оплаченный
            fineRecord.IsPaid = true;
            fineRecord.PaidAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(dto.PaymentNotes))
            {
                fineRecord.Notes += $" Оплачено: {dto.PaymentNotes}";
            }

            // Уменьшаем общую сумму штрафа пользователя
            user.FineAmount = Math.Max(0, user.FineAmount - fineRecord.Amount);

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Штраф успешно оплачен",
                    fineId = fineRecord.Id,
                    paidAmount = fineRecord.Amount,
                    remainingFineAmount = user.FineAmount,
                    paidAt = fineRecord.PaidAt
                });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = $"Ошибка при оплате штрафа: {ex.InnerException?.Message ?? ex.Message}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Непредвиденная ошибка: {ex.Message}" });
            }
        }
    }
}
