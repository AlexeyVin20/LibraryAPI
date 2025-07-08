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
using System.Dynamic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LibraryAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(LoginRequest request);
        Task<AuthResponse> Register(RegisterRequest request);
        Task<AuthResponse> RefreshToken(string refreshToken);
        Task<User> GetUserFromToken(ClaimsPrincipal user);
        Task<bool> ForgotPassword(string identifier);
        Task<bool> ResetPassword(ResetPasswordWithTokenRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly LibraryDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            LibraryDbContext context,
            IJwtService jwtService,
            IOptions<JwtSettings> jwtSettings,
            IEmailService emailService,
            ITemplateRenderer templateRenderer,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
            _templateRenderer = templateRenderer;
            _configuration = configuration;
            _logger = logger;
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

            return await GenerateAuthResponse(user, user.PasswordResetRequired);
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

        public async Task<bool> ForgotPassword(string identifier)
        {
            // Определяем, что передано: email или логин
            User user;
            if (!string.IsNullOrWhiteSpace(identifier) && identifier.Contains("@"))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == identifier);
            }
            else
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Username == identifier);
            }

            if (user == null)
            {
                // Не раскрываем, существует ли пользователь, для безопасности
                return true; // Всегда true, чтобы не раскрывать наличие пользователя
            }

            // Генерируем токен
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            token = token.Replace('+', '-').Replace('/', '_'); // Делаем токен URL-safe

            user.PasswordResetToken = token;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1); // Токен действителен 1 час

            await _context.SaveChangesAsync();

            // Формируем ссылку для сброса, используя URL из конфигурации
            var clientAppUrl = _configuration["AppSettings:ClientAppUrl"];
            if (string.IsNullOrEmpty(clientAppUrl))
            {
                throw new InvalidOperationException("ClientAppUrl не настроен в конфигурации (AppSettings:ClientAppUrl).");
            }
            var resetLink = $"{clientAppUrl.TrimEnd('/')}/reset-password?token={token}&email={Uri.EscapeDataString(user.Email)}";

            // Готовим данные для шаблона
            var templateModel = new ExpandoObject() as IDictionary<string, object>;
            templateModel.Add("UserName", user.FullName);
            templateModel.Add("ResetLink", resetLink);
            templateModel.Add("Year", DateTime.Now.Year);

            // Рендерим шаблон
            var templatePath = Path.Combine("Templates", "PasswordResetEmail.html");
            string htmlBody = await _templateRenderer.RenderAsync(templatePath, (ExpandoObject)templateModel);

            // Отправляем email
            var emailSent = await _emailService.SendEmailAsync(user.Email, "Сброс пароля", htmlBody);

            if (!emailSent)
            {
                _logger.LogError("Ошибка отправки письма сброса пароля пользователю {Email}", user.Email);
            }

            return emailSent;
        }

        public async Task<bool> ResetPassword(ResetPasswordWithTokenRequest request)
        {
            // Ищем пользователя по токену сброса пароля
            var query = _context.Users.AsQueryable();

            // Если в запросе передан email, дополнительно проверим его соответствие
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                query = query.Where(u => u.Email == request.Email);
            }

            var user = await query.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == request.Token &&
                u.PasswordResetTokenExpires > DateTime.UtcNow);

            if (user == null)
            {
                return false;
            }

            user.PasswordHash = HashPassword(request.NewPassword);
            user.PasswordResetRequired = false;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<AuthResponse> GenerateAuthResponse(User user, bool passwordResetRequired = false)
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
                    DateRegistered = user.DateRegistered,
                    Username = user.Username,
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate,
                    BorrowedBooksCount = user.BorrowedBooksCount ?? 0,
                    MaxBooksAllowed = user.MaxBooksAllowed ?? 5,
                    LoanPeriodDays = user.LoanPeriodDays,
                    FineAmount = user.FineAmount,
                    PasswordResetRequired = passwordResetRequired,
                    Roles = roles.ToArray()
                },
                PasswordResetRequired = passwordResetRequired
            };
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Если это старый хеш SHA256, пытаемся его проверить
                using var sha256 = SHA256.Create();
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sha256Hash = Convert.ToBase64String(hashedBytes);
                return sha256Hash == passwordHash;
            }
        }
    }
}