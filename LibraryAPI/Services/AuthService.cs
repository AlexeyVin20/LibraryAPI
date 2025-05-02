using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LibraryAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(LoginRequest request);
        Task<AuthResponse> Register(RegisterRequest request);
        Task<AuthResponse> RefreshToken(string refreshToken);
        Task<User> GetUserFromToken(ClaimsPrincipal user);
    }

    public class AuthService : IAuthService
    {
        private readonly LibraryDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            LibraryDbContext context,
            IJwtService jwtService,
            IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new Exception("Неверный пароль");
            }

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> Register(RegisterRequest request)
        {
            // Проверка, что пользователь с таким именем не существует
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new Exception("Пользователь с таким именем уже существует");
            }

            // Проверка, что пользователь с таким email не существует
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new Exception("Пользователь с таким email уже существует");
            }

            // Создаем нового пользователя
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                PassportNumber = request.PassportNumber,
                PassportIssuedBy = request.PassportIssuedBy,
                PassportIssuedDate = request.PassportIssuedDate,
                Address = request.Address,
                Username = request.Username,
                PasswordHash = HashPassword(request.Password),
                DateRegistered = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);

            // Проверяем существование роли "User"
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                userRole = new Role { Name = "User", Description = "Обычный пользователь" };
                _context.Roles.Add(userRole);
                await _context.SaveChangesAsync();
            }

            // Назначаем пользователю роль "User"
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = userRole.Id
            });

            await _context.SaveChangesAsync();

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> RefreshToken(string refreshToken)
        {
            var savedRefreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (savedRefreshToken == null || savedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                throw new Exception("Недействительный или просроченный refresh token");
            }

            var user = savedRefreshToken.User;
            if (user == null || !user.IsActive)
            {
                throw new Exception("Пользователь не существует или деактивирован");
            }

            // Удаляем старый refresh token
            _context.RefreshTokens.Remove(savedRefreshToken);
            await _context.SaveChangesAsync();

            // Генерируем новый ответ с токенами
            return await GenerateAuthResponse(user);
        }

        public async Task<User> GetUserFromToken(ClaimsPrincipal claimsPrincipal)
        {
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }

            return user;
        }

        private async Task<AuthResponse> GenerateAuthResponse(User user)
        {
            // Получаем роли пользователя
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            // Генерируем JWT-токен
            var token = _jwtService.GenerateJwtToken(user, roles);
            
            // Генерируем refresh token
            var refreshToken = _jwtService.GenerateRefreshToken();
            
            // Сохраняем refresh token в базе данных
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            // Обновляем дату последнего входа
            user.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Формируем ответ
            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                User = new AuthUserDto
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
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate,
                    BorrowedBooksCount = user.BorrowedBooksCount ?? 0,
                    MaxBooksAllowed = user.MaxBooksAllowed ?? 5,
                    LoanPeriodDays = user.LoanPeriodDays,
                    FineAmount = user.FineAmount,
                    Roles = roles.ToArray()
                }
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == passwordHash;
        }
    }
}