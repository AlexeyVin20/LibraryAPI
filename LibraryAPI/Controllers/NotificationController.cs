using LibraryAPI.Models.DTOs;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IAuthService _authService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            IAuthService authService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Получение уведомлений пользователя
        /// </summary>
        /// <param name="isRead">Фильтр по статусу прочтения (null - все, true - прочитанные, false - непрочитанные)</param>
        /// <param name="page">Номер страницы (по умолчанию 1)</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 20)</param>
        /// <returns>Список уведомлений пользователя</returns>
        /// <response code="200">Список уведомлений успешно получен</response>
        /// <response code="401">Пользователь не авторизован</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Получение уведомлений пользователя",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Возвращает список уведомлений с пагинацией и фильтрацией."
        )]
        public async Task<IActionResult> GetUserNotifications(
            [FromQuery] bool? isRead = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var notifications = await _notificationService.GetUserNotificationsAsync(user.Id, isRead, page, pageSize);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения уведомлений для пользователя");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получение статистики уведомлений пользователя
        /// </summary>
        /// <returns>Статистика уведомлений (общее количество, непрочитанные, по типам и приоритетам)</returns>
        /// <response code="200">Статистика успешно получена</response>
        /// <response code="401">Пользователь не авторизован</response>
        [HttpGet("stats")]
        [Authorize]
        [ProducesResponseType(typeof(NotificationStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Получение статистики уведомлений",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Возвращает детальную статистику уведомлений пользователя."
        )]
        public async Task<IActionResult> GetNotificationStats()
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var stats = await _notificationService.GetUserNotificationStatsAsync(user.Id);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения статистики уведомлений для пользователя");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получение количества непрочитанных уведомлений
        /// </summary>
        /// <returns>Количество непрочитанных уведомлений пользователя</returns>
        /// <response code="200">Количество успешно получено</response>
        /// <response code="401">Пользователь не авторизован</response>
        [HttpGet("unread-count")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Получение количества непрочитанных уведомлений",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Возвращает количество непрочитанных уведомлений."
        )]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var count = await _notificationService.GetUnreadCountAsync(user.Id);
                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения количества непрочитанных уведомлений для пользователя");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отметка уведомления как прочитанного
        /// </summary>
        /// <param name="notificationId">ID уведомления для отметки</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Уведомление успешно отмечено как прочитанное</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Уведомление не найдено</response>
        [HttpPut("{notificationId}/mark-read")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Отметка уведомления как прочитанного",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Отмечает указанное уведомление как прочитанное."
        )]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var result = await _notificationService.MarkAsReadAsync(notificationId, user.Id);
                if (!result)
                    return NotFound(new { message = "Уведомление не найдено или уже прочитано" });

                return Ok(new { message = "Уведомление отмечено как прочитанное" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отметки уведомления {NotificationId} как прочитанного", notificationId);
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отметка нескольких уведомлений как прочитанных
        /// </summary>
        /// <param name="dto">Список ID уведомлений для отметки</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Уведомления успешно отмечены как прочитанные</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Уведомления не найдены</response>
        [HttpPut("mark-multiple-read")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Отметка нескольких уведомлений как прочитанных",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Отмечает указанные уведомления как прочитанные."
        )]
        public async Task<IActionResult> MarkMultipleAsRead([FromBody] NotificationBulkMarkReadDto dto)
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var result = await _notificationService.MarkMultipleAsReadAsync(dto.NotificationIds, user.Id);
                if (!result)
                    return NotFound(new { message = "Уведомления не найдены или уже прочитаны" });

                return Ok(new { message = "Уведомления отмечены как прочитанные" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отметки множественных уведомлений как прочитанных");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отметка всех уведомлений как прочитанных
        /// </summary>
        /// <returns>Результат операции</returns>
        /// <response code="200">Все уведомления успешно отмечены как прочитанные</response>
        /// <response code="401">Пользователь не авторизован</response>
        [HttpPut("mark-all-read")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Отметка всех уведомлений как прочитанных",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Отмечает все непрочитанные уведомления пользователя как прочитанные."
        )]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var result = await _notificationService.MarkAllAsReadAsync(user.Id);
                if (!result)
                    return Ok(new { message = "Нет непрочитанных уведомлений" });

                return Ok(new { message = "Все уведомления отмечены как прочитанные" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отметки всех уведомлений как прочитанных для пользователя");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка индивидуального уведомления (только для Администратор/Библиотекарь)
        /// </summary>
        /// <param name="dto">Данные уведомления для отправки</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Уведомление успешно отправлено</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpPost("send")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Отправка индивидуального уведомления",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор или Библиотекарь. Отправляет уведомление конкретному пользователю."
        )]
        public async Task<IActionResult> SendNotification([FromBody] NotificationCreateDto dto)
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var notification = await _notificationService.CreateNotificationAsync(dto);
                return Ok(new { message = "Уведомление отправлено", notificationId = notification.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки уведомления");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка массовых уведомлений (только для Администратор/Библиотекарь)
        /// </summary>
        /// <param name="dto">Данные для массовой отправки уведомлений</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Массовые уведомления успешно отправлены</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpPost("send-bulk")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Отправка массовых уведомлений",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор или Библиотекарь. Отправляет уведомления нескольким пользователям."
        )]
        public async Task<IActionResult> SendBulkNotification([FromBody] NotificationPushDto dto)
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var notifications = await _notificationService.CreateBulkNotificationsAsync(dto);
                return Ok(new { message = "Массовые уведомления отправлены", count = notifications.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки массовых уведомлений");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка напоминаний о скором возврате книг (только для Администратор/Библиотекарь)
        /// </summary>
        /// <returns>Результат операции</returns>
        /// <response code="200">Напоминания успешно отправлены</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpPost("send-due-reminders")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Отправка напоминаний о скором возврате книг",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор или Библиотекарь. Отправляет напоминания пользователям о необходимости вернуть книги."
        )]
        public async Task<IActionResult> SendDueReminders()
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                await _notificationService.SendDueRemindersToUsersWithBooksAsync();
                return Ok(new { message = "Напоминания о возврате книг отправлены" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки напоминаний о возврате книг");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка уведомлений о просроченных книгах (только для Администратор/Библиотекарь)
        /// </summary>
        /// <returns>Результат операции</returns>
        /// <response code="200">Уведомления о просроченных книгах успешно отправлены</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpPost("send-overdue-notifications")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Отправка уведомлений о просроченных книгах",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор или Библиотекарь. Отправляет уведомления пользователям о просроченных книгах."
        )]
        public async Task<IActionResult> SendOverdueNotifications()
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                await _notificationService.SendOverdueNotificationsToUsersWithBooksAsync();
                return Ok(new { message = "Уведомления о просроченных книгах отправлены" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки уведомлений о просроченных книгах");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка уведомлений о штрафах (только для Администратор/Библиотекарь)
        /// </summary>
        /// <returns>Результат операции</returns>
        /// <response code="200">Уведомления о штрафах успешно отправлены</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpPost("send-fine-notifications")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Отправка уведомлений о штрафах",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор или Библиотекарь. Отправляет уведомления пользователям о неоплаченных штрафах."
        )]
        public async Task<IActionResult> SendFineNotifications()
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                await _notificationService.SendFineNotificationsToUsersWithFinesAsync();
                return Ok(new { message = "Уведомления о штрафах отправлены" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки уведомлений о штрафах");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка тестового push уведомления (только для Администратор)
        /// </summary>
        /// <param name="userId">ID пользователя для отправки тестового уведомления</param>
        /// <param name="message">Текст тестового сообщения</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Тестовое уведомление успешно отправлено</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpPost("test-push/{userId}")]
        [Authorize(Roles = "Администратор")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Отправка тестового push уведомления",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор. Отправляет тестовое push уведомление указанному пользователю."
        )]
        public async Task<IActionResult> TestPushNotification(Guid userId, [FromBody] string message)
        {
            try
            {
                // Проверяем авторизацию через AuthService
                var currentUser = await _authService.GetUserFromToken(User);
                await _notificationService.SendPushNotificationAsync(userId, "Тестовое уведомление", message, Models.NotificationType.GeneralInfo);
                return Ok(new { message = "Тестовое push уведомление отправлено" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки тестового push уведомления");
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("auth-test")]
        public async Task<IActionResult> TestAuthentication()
        {
            try
            {
                var isAuthenticated = User.Identity.IsAuthenticated;
                var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();

                // Пытаемся получить пользователя через AuthService
                Guid userId = Guid.Empty;
                string userInfo = "Не удалось получить информацию о пользователе";
                
                try
                {
                    var user = await _authService.GetUserFromToken(User);
                    userId = user.Id;
                    userInfo = $"Пользователь найден: {user.FullName} ({user.Email})";
                }
                catch (Exception userEx)
                {
                    userInfo = $"Ошибка получения пользователя: {userEx.Message}";
                }

                return Ok(new
                {
                    IsAuthenticated = isAuthenticated,
                    UserId = userId,
                    UserInfo = userInfo,
                    Claims = claims,
                    AuthorizationHeader = authHeader?.Substring(0, Math.Min(50, authHeader.Length)) + "...",
                    UserName = User.Identity.Name,
                    AuthenticationType = User.Identity.AuthenticationType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка тестирования аутентификации");
                return BadRequest(new { message = "Ошибка тестирования аутентификации", error = ex.Message });
            }
        }


    }
} 