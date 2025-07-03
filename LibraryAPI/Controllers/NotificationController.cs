using LibraryAPI.Models.DTOs;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using LibraryAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IAuthService _authService;
        private readonly ILogger<NotificationController> _logger;
        private readonly LibraryDbContext _context;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly IEmailService _emailService;

        public NotificationController(
            INotificationService notificationService,
            IAuthService authService,
            ILogger<NotificationController> logger,
            LibraryDbContext context,
            ITemplateRenderer templateRenderer,
            IEmailService emailService)
        {
            _notificationService = notificationService;
            _authService = authService;
            _logger = logger;
            _context = context;
            _templateRenderer = templateRenderer;
            _emailService = emailService;
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
        /// Получение всех уведомлений всех пользователей (только для Администратор/Библиотекарь)
        /// </summary>
        /// <param name="isRead">Фильтр по статусу прочтения (null - все, true - прочитанные, false - непрочитанные)</param>
        /// <param name="page">Номер страницы (по умолчанию 1)</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 20)</param>
        /// <param name="userId">Фильтр по пользователю (необязательно)</param>
        /// <param name="type">Фильтр по типу уведомления (необязательно)</param>
        /// <param name="priority">Фильтр по приоритету (необязательно)</param>
        /// <returns>Список всех уведомлений с информацией о пользователях</returns>
        /// <response code="200">Список уведомлений успешно получен</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(List<AdminNotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Получение всех уведомлений всех пользователей",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор или Библиотекарь. Возвращает список всех уведомлений с информацией о пользователях, с возможностью фильтрации и пагинации."
        )]
        public async Task<IActionResult> GetAllNotifications(
            [FromQuery] bool? isRead = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? userId = null,
            [FromQuery] Models.NotificationType? type = null,
            [FromQuery] Models.NotificationPriority? priority = null)
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var notifications = await _notificationService.GetAllNotificationsAsync(isRead, page, pageSize, userId, type, priority);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения всех уведомлений");
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
        /// Получение административной статистики по всем уведомлениям (только для Администратор/Библиотекарь)
        /// </summary>
        /// <returns>Детальная статистика по всем уведомлениям в системе</returns>
        /// <response code="200">Статистика успешно получена</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpGet("admin/stats")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(AdminNotificationStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Получение административной статистики по всем уведомлениям",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор или Библиотекарь. Возвращает детальную статистику по всем уведомлениям в системе, включая информацию о пользователях и активности за последние дни."
        )]
        public async Task<IActionResult> GetAdminNotificationStats()
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var stats = await _notificationService.GetAllNotificationStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения административной статистики уведомлений");
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
        /// Удаление уведомления
        /// </summary>
        /// <param name="notificationId">ID уведомления для удаления</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Уведомление успешно удалено</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Уведомление не найдено</response>
        [HttpDelete("{notificationId}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Удаление уведомления",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Удаляет указанное уведомление пользователя."
        )]
        public async Task<IActionResult> DeleteNotification(Guid notificationId)
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var result = await _notificationService.DeleteNotificationAsync(notificationId, user.Id);
                if (!result)
                    return NotFound(new { message = "Уведомление не найдено" });

                return Ok(new { message = "Уведомление успешно удалено" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления уведомления {NotificationId}", notificationId);
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

        /// <summary>
        /// Отправка тестового email уведомления (только для Администратор)
        /// </summary>
        /// <param name="userId">ID пользователя для отправки тестового email</param>
        /// <param name="dto">Данные тестового сообщения</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Тестовое email уведомление успешно отправлено</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpPost("test-email/{userId}")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Отправка тестового email уведомления",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор или Библиотекарь. Отправляет тестовое email уведомление указанному пользователю."
        )]
        public async Task<IActionResult> TestEmailNotification(Guid userId, [FromBody] TestEmailDto dto)
        {
            try
            {
                var currentUser = await _authService.GetUserFromToken(User);
                var templateData = new Dictionary<string, object>
                {
                    { "Message", dto.Message }
                };
                var result = await _notificationService.SendEmailNotificationAsync(userId, dto.Title, Models.NotificationType.GeneralInfo, templateData);
                
                if (result)
                {
                    return Ok(new { message = "Тестовое email уведомление отправлено" });
                }
                else
                {
                    return BadRequest(new { message = "Не удалось отправить тестовое email уведомление" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки тестового email уведомления");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка массовых email уведомлений (только для Администратор/Библиотекарь)
        /// </summary>
        /// <param name="dto">Данные для массовой отправки email уведомлений</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Массовые email уведомления успешно отправлены</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpPost("send-bulk-email")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Отправка массовых email уведомлений",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор или Библиотекарь. Отправляет email уведомления нескольким пользователям."
        )]
        public async Task<IActionResult> SendBulkEmailNotification([FromBody] NotificationPushDto dto)
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var result = await _notificationService.SendBulkEmailNotificationAsync(dto.UserIds, dto.Title, dto.Message, dto.Type);
                
                if (result)
                {
                    return Ok(new { message = "Массовые email уведомления отправлены" });
                }
                else
                {
                    return BadRequest(new { message = "Не удалось отправить массовые email уведомления" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки массовых email уведомлений");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получение статистики email уведомлений (только для Администратор/Библиотекарь)
        /// </summary>
        /// <returns>Статистика email уведомлений</returns>
        /// <response code="200">Статистика email успешно получена</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав доступа</response>
        [HttpGet("email-stats")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Получение статистики email уведомлений",
            Description = "Требуется JWT Bearer токен в заголовке Authorization и роль Администратор или Библиотекарь. Возвращает детальную статистику по email уведомлениям."
        )]
        public async Task<IActionResult> GetEmailStats()
        {
            try
            {
                var user = await _authService.GetUserFromToken(User);
                var stats = await _notificationService.GetAllNotificationStatsAsync();
                
                return Ok(new
                {
                    EmailsSent = stats.EmailsSent,
                    EmailsDelivered = stats.EmailsDelivered,
                    EmailsFailed = stats.EmailsFailed,
                    EmailSuccessRate = stats.EmailsSent > 0 ? (double)stats.EmailsDelivered / stats.EmailsSent * 100 : 0,
                    TotalNotifications = stats.TotalNotifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения статистики email уведомлений");
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка напоминания о скором возврате книг конкретному пользователю
        /// </summary>
        /// <param name="userId">ID пользователя для отправки напоминания</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Напоминание успешно отправлено</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Пользователь не найден или нет книг для напоминания</response>
        [HttpPost("send-due-reminder/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Отправка напоминания о скором возврате книг конкретному пользователю",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Отправляет напоминание конкретному пользователю о необходимости вернуть книги."
        )]
        public async Task<IActionResult> SendDueReminderToUser(Guid userId)
        {
            try
            {
                var currentUser = await _authService.GetUserFromToken(User);
                var result = await _notificationService.SendDueReminderToUserAsync(userId);
                
                if (!result)
                    return NotFound(new { message = "Пользователь не найден или нет книг для напоминания" });

                return Ok(new { message = "Напоминание о возврате книг отправлено пользователю" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки напоминания о возврате книг пользователю {UserId}", userId);
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка уведомления о просроченных книгах конкретному пользователю
        /// </summary>
        /// <param name="userId">ID пользователя для отправки уведомления</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Уведомление о просроченных книгах успешно отправлено</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Пользователь не найден или нет просроченных книг</response>
        [HttpPost("send-overdue-notification/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Отправка уведомления о просроченных книгах конкретному пользователю",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Отправляет уведомление конкретному пользователю о просроченных книгах."
        )]
        public async Task<IActionResult> SendOverdueNotificationToUser(Guid userId)
        {
            try
            {
                var currentUser = await _authService.GetUserFromToken(User);
                var result = await _notificationService.SendOverdueNotificationToUserAsync(userId);
                
                if (!result)
                    return NotFound(new { message = "Пользователь не найден или нет просроченных книг" });

                return Ok(new { message = "Уведомление о просроченных книгах отправлено пользователю" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки уведомления о просроченных книгах пользователю {UserId}", userId);
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка уведомления о штрафах конкретному пользователю
        /// </summary>
        /// <param name="userId">ID пользователя для отправки уведомления</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Уведомление о штрафах успешно отправлено</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Пользователь не найден или нет неоплаченных штрафов</response>
        [HttpPost("send-fine-notification/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Отправка уведомления о штрафах конкретному пользователю",
            Description = "Требуется JWT Bearer токен в заголовке Authorization. Отправляет уведомление конкретному пользователю о неоплаченных штрафах."
        )]
        public async Task<IActionResult> SendFineNotificationToUser(Guid userId)
        {
            try
            {
                var currentUser = await _authService.GetUserFromToken(User);
                var result = await _notificationService.SendFineNotificationToUserAsync(userId);
                
                if (!result)
                    return NotFound(new { message = "Пользователь не найден или нет неоплаченных штрафов" });

                return Ok(new { message = "Уведомление о штрафах отправлено пользователю" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки уведомления о штрафах пользователю {UserId}", userId);
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправка кастомизированного email уведомления
        /// </summary>
        /// <param name="request">Данные для отправки</param>
        /// <returns>Результат операции</returns>
        /// <response code="200">Email успешно отправлен</response>
        /// <response code="400">Пользователь не найден или некорректные данные</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="500">Ошибка при отправке email</response>
        [HttpPost("send-custom-email")]
        [Authorize(Roles = "Администратор,Библиотекарь")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Отправка кастомизированного email уведомления",
            Description = "Требуется JWT Bearer токен и роль Администратор или Библиотекарь. Отправляет email с использованием HTML-шаблона."
        )]
        public async Task<IActionResult> SendCustomEmail([FromBody] SendEmailRequestDto request)
        {
            try
            {
                // -- START DEBUG LOGGING --
                _logger.LogInformation("--- Получен запрос на кастомный email ---");
                _logger.LogInformation("UserID: {UserId}, Type: {Type}, Title: {Title}", request.UserId, request.Type, request.Title);
                _logger.LogInformation("Данные для шаблона (TemplateData):");
                if (request.TemplateData.Count == 0)
                {
                    _logger.LogInformation("  -> TemplateData пуст.");
                }
                else
                {
                    foreach (var kvp in request.TemplateData)
                    {
                        _logger.LogInformation("  -> Ключ: '{Key}', Значение: '{Value}'", kvp.Key, kvp.Value);
                    }
                }
                _logger.LogInformation("--- Конец лога запроса ---");
                // -- END DEBUG LOGGING --

                // 1. Найти пользователя
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    return BadRequest(new { message = "Пользователь не найден или у него нет email." });
                }

                // 2. Определить, какой HTML-шаблон использовать
                string templateName = GetTemplateNameForType(request.Type);

                // 3. Создать модель с данными для шаблона
                var templateModel = new Dictionary<string, object>(request.TemplateData);
                templateModel["UserName"] = user.FullName;
                templateModel["Title"] = request.Title;

                // Преобразуем словарь в ExpandoObject
                var expandoModel = new ExpandoObject();
                var modelAsDictionary = (IDictionary<string, object>)expandoModel;
                foreach (var kvp in templateModel)
                {
                    modelAsDictionary[kvp.Key] = kvp.Value;
                }

                // 4. Сгенерировать HTML из шаблона и данных
                string htmlBody = await _templateRenderer.RenderAsync(templateName, expandoModel);

                // 5. Отправить email
                var emailSent = await _emailService.SendBulkEmailAsync(
                    new List<string> { user.Email },
                    request.Title,
                    htmlBody,
                    true // isHtml
                );
                
                if (emailSent)
                {
                    _logger.LogInformation($"Кастомный Email успешно отправлен пользователю {user.Email}.");
                    return Ok(new { message = "Email успешно отправлен." });
                }
                else
                {
                    _logger.LogError($"Ошибка при отправке кастомного email пользователю {user.Email}.");
                    return StatusCode(500, new { message = "Ошибка при отправке email." });
                }
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Ошибка: шаблон email не найден.");
                return StatusCode(500, new { message = "Ошибка на сервере: шаблон не найден." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка при отправке кастомного email.");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера." });
            }
        }
        
        // Вспомогательные методы
        private string GetTemplateNameForType(string type)
        {
            return type switch
            {
                "BookOverdue" => "Templates/OverdueEmail.html",
                "FineAdded" => "Templates/FineEmail.html",
                "ReturnSoon" => "Templates/ReturnSoonEmail.html",
                "BookReturned" => "Templates/BookReturnedEmail.html",
                "ReservationReady" => "Templates/ReservationEmail.html",
                _ => "Templates/GeneralEmail.html",
            };
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