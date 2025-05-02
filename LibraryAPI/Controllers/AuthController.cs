using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Models;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Авторизация пользователя
        /// </summary>
        /// <param name="request">Логин и пароль</param>
        /// <returns>Токен авторизации и информация о пользователе</returns>
        /// <response code="200">Успешная авторизация</response>
        /// <response code="400">Неверные данные</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.Login(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="request">Данные нового пользователя</param>
        /// <returns>Токен авторизации и информация о пользователе</returns>
        /// <response code="200">Успешная регистрация</response>
        /// <response code="400">Неверные данные</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.Register(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Обновление токена авторизации
        /// </summary>
        /// <param name="request">Refresh-токен</param>
        /// <returns>Новые токены авторизации</returns>
        /// <response code="200">Токен успешно обновлен</response>
        /// <response code="400">Неверный refresh-токен</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _authService.RefreshToken(request.RefreshToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получение информации о текущем авторизованном пользователе
        /// </summary>
        /// <remarks>
        /// Требуется JWT-токен в заголовке Authorization в формате Bearer.
        /// </remarks>
        /// <returns>Информация о пользователе</returns>
        /// <response code="200">Информация о пользователе</response>
        /// <response code="401">Не авторизован</response>
        [HttpGet("session")]
        [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Получение информации о текущем пользователе",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Пример: `Authorization: Bearer {ваш_токен}`"
        )]
        public async Task<IActionResult> GetSession()
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();

                return Ok(new AuthUserDto
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
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
} 