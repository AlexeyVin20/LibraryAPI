using LibraryAPI.Data;
using LibraryAPI.Models;
using LibraryAPI.Models.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using LibraryAPI.Hubs;
using System.Dynamic;

namespace LibraryAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly LibraryDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;
        private readonly ITemplateRenderer _templateRenderer;

        public NotificationService(
            LibraryDbContext context, 
            IHubContext<NotificationHub> hubContext,
            IEmailService emailService,
            ILogger<NotificationService> logger,
            ITemplateRenderer templateRenderer)
        {
            _context = context;
            _hubContext = hubContext;
            _emailService = emailService;
            _logger = logger;
            _templateRenderer = templateRenderer;
        }

        /// Создание и отправка уведомления
        /// </summary>
        public async Task<Notification> CreateNotificationAsync(NotificationCreateDto dto, bool sendEmail = true, bool sendPush = true)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                _logger.LogWarning($"Пользователь с ID {dto.UserId} не найден при создании уведомления.");
                return null; // Или выбросить исключение, если это критично
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                Priority = dto.Priority,
                AdditionalData = dto.AdditionalData,
                BookId = dto.BookId,
                BorrowedBookId = null, // Не используем BorrowedBookId для резерваций
                CreatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Отправка кастомного Email, если есть TemplateData
            if (sendEmail && dto.TemplateData != null && dto.TemplateData.Count > 0)
            {
                await SendEmailNotificationAsync(notification.UserId, notification.Title, notification.Type, dto.TemplateData);
            }
            // Отправка простого Email, если нет TemplateData, но есть сообщение
            else if (sendEmail && !string.IsNullOrEmpty(dto.Message))
            {
                await SendEmailNotificationAsync(notification.UserId, notification.Title, Models.NotificationType.GeneralInfo, new Dictionary<string, object> { { "Message", dto.Message } });
            }

            // Отправка Push-уведомления через SignalR
            if(sendPush)
            {
                await SendPushNotificationAsync(notification.UserId, notification.Title, notification.Message, notification.Type);
            }

            return notification;
        }

        public async Task<List<Notification>> CreateBulkNotificationsAsync(NotificationPushDto dto)
        {
            var notifications = new List<Notification>();

            foreach (var userId in dto.UserIds)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = dto.Type,
                    Priority = dto.Priority,
                    AdditionalData = dto.AdditionalData,
                    CreatedAt = DateTime.UtcNow
                };
                notifications.Add(notification);
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Отправка push уведомлений всем пользователям
            await SendPushNotificationToMultipleUsersAsync(dto.UserIds, dto.Title, dto.Message, dto.Type);

            // Отправка email уведомлений всем пользователям с сохранением результата
            await SendBulkEmailNotificationWithSaveAsync(notifications, dto.Title, dto.Message, dto.Type);

            _logger.LogInformation($"Создано {notifications.Count} массовых уведомлений");
            return notifications;
        }

        public async Task SendBookDueReminderAsync(BookDueNotificationDto dto)
        {
            var title = "Напоминание о возврате книги";
            var message = $"Книга \"{dto.BookTitle}\" должна быть возвращена {dto.DueDate:dd.MM.yyyy}. " +
                         $"Осталось дней: {dto.DaysUntilDue}";

            var templateData = new Dictionary<string, object>
            {
                { "BookTitle", dto.BookTitle },
                { "DueDate", dto.DueDate.ToString("dd.MM.yyyy") },
                { "DaysUntilDue", dto.DaysUntilDue.ToString() }
            };

            var additionalData = JsonSerializer.Serialize(new
            {
                BookId = dto.BookId,
                BorrowedBookId = dto.BorrowedBookId,
                DueDate = dto.DueDate,
                DaysUntilDue = dto.DaysUntilDue
            });

            var notificationDto = new NotificationCreateDto
            {
                UserId = dto.UserId,
                Title = title,
                Message = message,
                Type = NotificationType.BookDueSoon,
                Priority = NotificationPriority.High,
                BookId = dto.BookId,
                BorrowedBookId = null, // Не используем BorrowedBookId для резерваций
                AdditionalData = JsonSerializer.Serialize(templateData),
                TemplateData = templateData
            };

            await CreateNotificationAsync(notificationDto);
        }

        public async Task SendOverdueNotificationAsync(OverdueNotificationDto dto)
        {
            var title = "Просроченная книга";
            
            var templateData = new Dictionary<string, object>
            {
                { "BookTitle", dto.BookTitle },
                { "DueDate", dto.DueDate.ToString("dd.MM.yyyy") },
                { "DaysOverdue", dto.DaysOverdue.ToString() },
                { "EstimatedFine", dto.EstimatedFine.ToString("C") }
            };

            var notificationDto = new NotificationCreateDto
            {
                UserId = dto.UserId,
                Title = title,
                Message = $"Книга \"{dto.BookTitle}\" просрочена. Срок возврата: {dto.DueDate:dd.MM.yyyy}", // Краткое сообщение для push
                Type = NotificationType.BookOverdue,
                Priority = NotificationPriority.Critical,
                BookId = dto.BookId,
                BorrowedBookId = null,
                AdditionalData = JsonSerializer.Serialize(templateData), // Сохраняем для истории
                TemplateData = templateData
            };

            await CreateNotificationAsync(notificationDto);
        }

        public async Task SendFineNotificationAsync(FineNotificationDto dto)
        {
            var title = "Уведомление о штрафе";
            var message = $"Ваш штраф составляет {dto.FineAmount:C}. " +
                         $"Причина: {dto.Reason}";

            if (dto.OverdueBooks?.Any() == true)
            {
                message += $"\nПросроченные книги: {string.Join(", ", dto.OverdueBooks.Select(b => b.BookTitle))}";
            }

            var additionalData = JsonSerializer.Serialize(new
            {
                FineAmount = dto.FineAmount,
                PreviousFineAmount = dto.PreviousFineAmount,
                Reason = dto.Reason,
                OverdueBooks = dto.OverdueBooks
            });

            var notificationDto = new NotificationCreateDto
            {
                UserId = dto.UserId,
                Title = title,
                Message = message,
                Type = NotificationType.FineAdded,
                Priority = NotificationPriority.High,
                BorrowedBookId = null, // Не используем BorrowedBookId для резерваций
                AdditionalData = additionalData
            };

            await CreateNotificationAsync(notificationDto);
        }

        public async Task SendBookReturnedNotificationAsync(Guid userId, string bookTitle, Guid bookId)
        {
            var title = "Книга возвращена";
            var message = $"Книга \"{bookTitle}\" успешно возвращена";

            var notificationDto = new NotificationCreateDto
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.BookReturned,
                Priority = NotificationPriority.Normal,
                BookId = bookId,
                BorrowedBookId = null // Не используем BorrowedBookId для резерваций
            };

            await CreateNotificationAsync(notificationDto);
        }

        public async Task SendDueRemindersToUsersWithBooksAsync()
        {
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var in3Days = DateTime.UtcNow.AddDays(3).Date;

            var reservationsToRemind = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.Status == ReservationStatus.Выдана && 
                           r.ActualReturnDate == null && 
                           r.ExpirationDate.Date >= tomorrow && 
                           r.ExpirationDate.Date <= in3Days)
                .ToListAsync();

            foreach (var reservation in reservationsToRemind)
            {
                var daysUntilDue = (reservation.ExpirationDate.Date - DateTime.UtcNow.Date).Days;
                
                var dto = new BookDueNotificationDto
                {
                    UserId = reservation.UserId,
                    BookId = reservation.BookId,
                    BorrowedBookId = null, // Не используем BorrowedBookId для резерваций
                    BookTitle = reservation.Book.Title,
                    BookAuthors = reservation.Book.Authors,
                    DueDate = reservation.ExpirationDate,
                    DaysUntilDue = daysUntilDue
                };

                await SendBookDueReminderAsync(dto);
            }

            _logger.LogInformation($"Отправлено {reservationsToRemind.Count} напоминаний о возврате книг");
        }

        public async Task SendOverdueNotificationsToUsersWithBooksAsync()
        {
            var overdueReservations = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => (r.Status == ReservationStatus.Просрочена || 
                           (r.Status == ReservationStatus.Выдана && r.ExpirationDate < DateTime.UtcNow)) &&
                           r.ActualReturnDate == null)
                .ToListAsync();

            foreach (var reservation in overdueReservations)
            {
                var daysOverdue = (DateTime.UtcNow - reservation.ExpirationDate).Days;
                var estimatedFine = daysOverdue * 10m; // 10 рублей за день

                var dto = new OverdueNotificationDto
                {
                    UserId = reservation.UserId,
                    BookId = reservation.BookId,
                    BorrowedBookId = null, // Не используем BorrowedBookId для резерваций
                    BookTitle = reservation.Book.Title,
                    BookAuthors = reservation.Book.Authors,
                    DueDate = reservation.ExpirationDate,
                    DaysOverdue = daysOverdue,
                    EstimatedFine = estimatedFine
                };

                await SendOverdueNotificationAsync(dto);
            }

            _logger.LogInformation($"Отправлено {overdueReservations.Count} уведомлений о просроченных книгах");
        }

        public async Task SendFineNotificationsToUsersWithFinesAsync()
        {
            var usersWithFines = await _context.Users
                .Where(u => u.FineAmount > 0)
                .ToListAsync();

            foreach (var user in usersWithFines)
            {
                // Получаем просроченные резервации для данного пользователя
                var overdueReservations = await _context.Reservations
                    .Include(r => r.Book)
                    .Where(r => r.UserId == user.Id && 
                               (r.Status == ReservationStatus.Просрочена || 
                               (r.Status == ReservationStatus.Выдана && r.ExpirationDate < DateTime.UtcNow)) &&
                               r.ActualReturnDate == null)
                    .ToListAsync();

                var overdueBooks = overdueReservations
                    .Select(r => new OverdueBookDto
                    {
                        BookId = r.BookId,
                        BookTitle = r.Book.Title,
                        BookAuthors = r.Book.Authors,
                        DueDate = r.ExpirationDate,
                        DaysOverdue = (DateTime.UtcNow - r.ExpirationDate).Days
                    })
                    .ToList();

                var dto = new FineNotificationDto
                {
                    UserId = user.Id,
                    FineAmount = user.FineAmount,
                    PreviousFineAmount = 0, // Можно добавить логику для отслеживания предыдущих штрафов
                    Reason = "Просроченные книги",
                    OverdueBooks = overdueBooks
                };

                await SendFineNotificationAsync(dto);
            }

            _logger.LogInformation($"Отправлено {usersWithFines.Count} уведомлений о штрафах");
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null || notification.IsRead)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkMultipleAsReadAsync(List<Guid> notificationIds, Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.Id) && n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!notifications.Any())
                return false;

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!unreadNotifications.Any())
                return false;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Удалено уведомление {notificationId} пользователя {userId}");
            return true;
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool? isRead = null, int page = 1, int pageSize = 20)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.Book)
                .AsQueryable();

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type.ToString(),
                    Priority = n.Priority.ToString(),
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    IsDelivered = n.IsDelivered,
                    DeliveredAt = n.DeliveredAt,
                    AdditionalData = n.AdditionalData,
                    BookId = n.BookId,
                    BorrowedBookId = n.BorrowedBookId,
                    IsEmailSent = n.IsEmailSent,
                    EmailSentAt = n.EmailSentAt,
                    EmailRecipient = n.EmailRecipient,
                    EmailDeliverySuccessful = n.EmailDeliverySuccessful,
                    EmailErrorMessage = n.EmailErrorMessage,
                    BookTitle = n.Book != null ? n.Book.Title : null,
                    BookAuthors = n.Book != null ? n.Book.Authors : null,
                    BookCover = n.Book != null ? n.Book.Cover : null
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<NotificationStatsDto> GetUserNotificationStatsAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            var stats = new NotificationStatsDto
            {
                TotalNotifications = notifications.Count,
                UnreadNotifications = notifications.Count(n => !n.IsRead),
                DeliveredNotifications = notifications.Count(n => n.IsDelivered),
                PendingNotifications = notifications.Count(n => !n.IsDelivered),
                EmailsSent = notifications.Count(n => n.IsEmailSent),
                EmailsDelivered = notifications.Count(n => n.IsEmailSent && n.EmailDeliverySuccessful),
                EmailsFailed = notifications.Count(n => n.IsEmailSent && !n.EmailDeliverySuccessful),
                NotificationsByType = notifications
                    .GroupBy(n => n.Type.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                NotificationsByPriority = notifications
                    .GroupBy(n => n.Priority.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return stats;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<List<AdminNotificationDto>> GetAllNotificationsAsync(bool? isRead = null, int page = 1, int pageSize = 20, Guid? userId = null, NotificationType? type = null, NotificationPriority? priority = null)
        {
            var query = _context.Notifications
                .Include(n => n.User)
                .Include(n => n.Book)
                .AsQueryable();

            // Применяем фильтры
            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);

            if (userId.HasValue)
                query = query.Where(n => n.UserId == userId.Value);

            if (type.HasValue)
                query = query.Where(n => n.Type == type.Value);

            if (priority.HasValue)
                query = query.Where(n => n.Priority == priority.Value);

            // Применяем пагинацию и сортировку
            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new AdminNotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    UserFullName = n.User.FullName,
                    UserEmail = n.User.Email,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type.ToString(),
                    Priority = n.Priority.ToString(),
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    IsDelivered = n.IsDelivered,
                    DeliveredAt = n.DeliveredAt,
                    AdditionalData = n.AdditionalData,
                    BookId = n.BookId,
                    BorrowedBookId = n.BorrowedBookId,
                    IsEmailSent = n.IsEmailSent,
                    EmailSentAt = n.EmailSentAt,
                    EmailRecipient = n.EmailRecipient,
                    EmailDeliverySuccessful = n.EmailDeliverySuccessful,
                    EmailErrorMessage = n.EmailErrorMessage,
                    BookTitle = n.Book != null ? n.Book.Title : null,
                    BookAuthors = n.Book != null ? n.Book.Authors : null,
                    BookCover = n.Book != null ? n.Book.Cover : null
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<AdminNotificationStatsDto> GetAllNotificationStatsAsync()
        {
            var allNotifications = await _context.Notifications
                .Include(n => n.User)
                .ToListAsync();

            var totalUsers = await _context.Users.CountAsync();
            var usersWithNotifications = await _context.Notifications
                .Select(n => n.UserId)
                .Distinct()
                .CountAsync();

            var usersWithUnreadNotifications = await _context.Notifications
                .Where(n => !n.IsRead)
                .Select(n => n.UserId)
                .Distinct()
                .CountAsync();

            // Статистика по дням за последние 7 дней
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                .ToList();

            var notificationsLastDays = new Dictionary<string, int>();
            foreach (var day in last7Days)
            {
                var count = allNotifications.Count(n => n.CreatedAt.Date == day);
                notificationsLastDays.Add(day.ToString("dd.MM"), count);
            }

            // Топ-5 пользователей с наибольшим количеством уведомлений
            var topUsers = allNotifications
                .GroupBy(n => n.User.FullName)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count());

            var stats = new AdminNotificationStatsDto
            {
                TotalNotifications = allNotifications.Count,
                UnreadNotifications = allNotifications.Count(n => !n.IsRead),
                DeliveredNotifications = allNotifications.Count(n => n.IsDelivered),
                PendingNotifications = allNotifications.Count(n => !n.IsDelivered),
                TotalUsers = totalUsers,
                UsersWithNotifications = usersWithNotifications,
                UsersWithUnreadNotifications = usersWithUnreadNotifications,
                EmailsSent = allNotifications.Count(n => n.IsEmailSent),
                EmailsDelivered = allNotifications.Count(n => n.IsEmailSent && n.EmailDeliverySuccessful),
                EmailsFailed = allNotifications.Count(n => n.IsEmailSent && !n.EmailDeliverySuccessful),
                NotificationsByType = allNotifications
                    .GroupBy(n => n.Type.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                NotificationsByPriority = allNotifications
                    .GroupBy(n => n.Priority.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                NotificationsLastDays = notificationsLastDays,
                TopUsersWithNotifications = topUsers
            };

            return stats;
        }

        public async Task SendPushNotificationAsync(Guid userId, string title, string message, NotificationType type)
        {
            try
            {
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        Title = title,
                        Message = message,
                        Type = type.ToString(),
                        Timestamp = DateTime.UtcNow
                    });

                // Обновление статуса доставки в БД
                var notification = await _context.Notifications
                    .Where(n => n.UserId == userId && n.Title == title && n.Message == message)
                    .OrderByDescending(n => n.CreatedAt)
                    .FirstOrDefaultAsync();

                if (notification != null)
                {
                    notification.IsDelivered = true;
                    notification.DeliveredAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка отправки push уведомления пользователю {userId}");
            }
        }

        public async Task SendPushNotificationToMultipleUsersAsync(List<Guid> userIds, string title, string message, NotificationType type)
        {
            try
            {
                var userIdsStrings = userIds.Select(id => id.ToString()).ToList();
                
                await _hubContext.Clients.Users(userIdsStrings)
                    .SendAsync("ReceiveNotification", new
                    {
                        Title = title,
                        Message = message,
                        Type = type.ToString(),
                        Timestamp = DateTime.UtcNow
                    });

                // Обновление статуса доставки для всех уведомлений
                var notifications = await _context.Notifications
                    .Where(n => userIds.Contains(n.UserId) && n.Title == title && n.Message == message)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsDelivered = true;
                    notification.DeliveredAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки массовых push уведомлений");
            }
        }

        public async Task CheckAndSendDueRemindersAsync()
        {
            await SendDueRemindersToUsersWithBooksAsync();
        }

        public async Task CheckAndSendOverdueNotificationsAsync()
        {
            await SendOverdueNotificationsToUsersWithBooksAsync();
        }

        public async Task CheckAndUpdateFinesAsync()
        {
            // Получаем просроченные резервирования
            var overdueReservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => (r.Status == ReservationStatus.Одобрена || r.Status == ReservationStatus.Выдана) && 
                           r.ExpirationDate < DateTime.UtcNow)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var fineRate = 10m; // 10 рублей за день просрочки

            foreach (var reservation in overdueReservations)
            {
                var daysOverdue = (int)(DateTime.UtcNow - reservation.ExpirationDate).TotalDays;
                if (daysOverdue <= 0) continue;

                // Проверяем, не был ли уже начислен штраф за сегодняшний день для этого резервирования
                var existingFineToday = await _context.FineRecords
                    .FirstOrDefaultAsync(fr => 
                        fr.ReservationId == reservation.Id &&
                        fr.CalculatedForDate == today &&
                        fr.FineType == "Overdue");

                if (existingFineToday != null)
                    continue; // Штраф за сегодня уже начислен

                // Получаем последний штраф для этого резервирования
                var lastFine = await _context.FineRecords
                    .Where(fr => fr.ReservationId == reservation.Id && fr.FineType == "Overdue")
                    .OrderByDescending(fr => fr.CalculatedForDate)
                    .FirstOrDefaultAsync();

                var lastCalculatedDays = 0;
                if (lastFine != null && lastFine.OverdueDays.HasValue)
                {
                    lastCalculatedDays = lastFine.OverdueDays.Value;
                }

                // Начисляем штраф только за новые дни просрочки
                var newOverdueDays = daysOverdue - lastCalculatedDays;
                if (newOverdueDays <= 0) continue;

                var newFineAmount = newOverdueDays * fineRate;

                // Создаем новую запись о штрафе
                var fineRecord = new FineRecord
                {
                    UserId = reservation.UserId,
                    ReservationId = reservation.Id,
                    Amount = newFineAmount,
                    Reason = $"Просрочка возврата книги \"{reservation.Book.Title}\" на {newOverdueDays} дн.",
                    OverdueDays = daysOverdue, // Общее количество дней просрочки
                    FineType = "Overdue",
                    CalculatedForDate = today,
                    Notes = $"Автоматически начислено за {newOverdueDays} новых дней просрочки (всего {daysOverdue} дн.)"
                };

                _context.FineRecords.Add(fineRecord);

                // Обновляем общую сумму штрафа пользователя
                reservation.User.FineAmount += newFineAmount;

                // Обновляем статус резервирования на "Просрочена"
                if (reservation.Status != ReservationStatus.Просрочена)
                {
                    reservation.Status = ReservationStatus.Просрочена;
                    reservation.Notes += $" Просрочено с {reservation.ExpirationDate:dd.MM.yyyy}";
                }

                // Отправляем уведомление о новом штрафе
                var fineNotificationDto = new FineNotificationDto
                {
                    UserId = reservation.UserId,
                    FineAmount = reservation.User.FineAmount,
                    PreviousFineAmount = reservation.User.FineAmount - newFineAmount,
                    Reason = fineRecord.Reason,
                    TemplateData = new Dictionary<string, object>
                    {
                        { "FineAmount", reservation.User.FineAmount.ToString("C") },
                        { "Reason", fineRecord.Reason },
                        { "BookTitle", reservation.Book.Title },
                        { "DueDate", reservation.ExpirationDate.ToString("dd.MM.yyyy") },
                        { "DaysOverdue", daysOverdue.ToString() }
                    },
                    OverdueBooks = new List<OverdueBookDto>
                    {
                        new OverdueBookDto
                        {
                            BookId = reservation.BookId,
                            BookTitle = reservation.Book.Title,
                            BookAuthors = reservation.Book.Authors,
                            DueDate = reservation.ExpirationDate,
                            DaysOverdue = daysOverdue
                        }
                    }
                };

                // Отправляем уведомление асинхронно
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendFineNotificationAsync(fineNotificationDto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Ошибка отправки уведомления о штрафе для пользователя {reservation.UserId}");
                    }
                });

                _logger.LogInformation($"Начислен штраф {newFineAmount:C} пользователю {reservation.User.FullName} за просрочку книги \"{reservation.Book.Title}\" на {newOverdueDays} дн.");
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Обработано {overdueReservations.Count} просроченных резервирований");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении штрафов в базу данных");
            }
        }

        public async Task CleanupOldNotificationsAsync(int daysOld = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            
            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
                .ToListAsync();

            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Удалено {oldNotifications.Count} старых уведомлений");
        }

        public async Task<bool> SendEmailNotificationAsync(Guid userId, string title, NotificationType type, Dictionary<string, object> templateData)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email) || !user.IsActive)
            {
                return false;
            }

            var templateName = GetTemplateNameForType(type);
            templateData["UserName"] = user.FullName;
            templateData["Title"] = title;

            var expandoModel = new ExpandoObject();
            var modelAsDictionary = (IDictionary<string, object>)expandoModel;
            foreach (var kvp in templateData)
            {
                modelAsDictionary[kvp.Key] = kvp.Value;
            }

            string htmlBody = await _templateRenderer.RenderAsync(templateName, expandoModel);


            return await _emailService.SendBulkEmailAsync(
                new List<string> { user.Email },
                title,
                htmlBody,
                true // isHtml
            );
        }

        public async Task<bool> SendBulkEmailNotificationAsync(List<Guid> userIds, string title, string message, NotificationType type)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => userIds.Contains(u.Id) && !string.IsNullOrEmpty(u.Email))
                    .ToListAsync();

                if (!users.Any())
                {
                    _logger.LogWarning("Не найдено пользователей с валидными email адресами для массовой отправки");
                    return false;
                }

                var emails = users.Select(u => u.Email).ToList();
                var result = await _emailService.SendBulkNotificationEmailAsync(emails, title, message, type.ToString());

                if (result)
                {
                    _logger.LogInformation($"Массовые email уведомления успешно отправлены {users.Count} пользователям");
                }
                else
                {
                    _logger.LogWarning($"Не удалось отправить массовые email уведомления {users.Count} пользователям");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки массовых email уведомлений");
                return false;
            }
        }

        public async Task SendEmailNotificationWithSaveAsync(Guid notificationId, Guid userId, string title, NotificationType type, Dictionary<string, object> templateData)
        {
            var user = await _context.Users.FindAsync(userId);
            string? recipientEmail = null;
            bool success = false;
            string? errorMessage = null;

            if (user != null && !string.IsNullOrEmpty(user.Email) && user.IsActive)
            {
                recipientEmail = user.Email;
                try
                {
                    success = await SendEmailNotificationAsync(userId, title, type, templateData);
                    if (!success)
                    {
                        errorMessage = "Не удалось отправить email по неизвестной причине.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при отправке email для уведомления {NotificationId}", notificationId);
                    errorMessage = ex.Message;
                }
            }
            else
            {
                errorMessage = "Пользователь не найден, не имеет email или неактивен.";
            }

            await UpdateNotificationEmailStatus(notificationId, true, recipientEmail, success, errorMessage);
        }

        public async Task SendBulkEmailNotificationWithSaveAsync(List<Notification> notifications, string title, string message, NotificationType type)
        {
            try
            {
                var userIds = notifications.Select(n => n.UserId).ToList();
                var users = await _context.Users
                    .Where(u => userIds.Contains(u.Id) && !string.IsNullOrEmpty(u.Email))
                    .ToListAsync();

                if (!users.Any())
                {
                    // Обновляем все уведомления как неуспешные
                    foreach (var notification in notifications)
                    {
                        await UpdateNotificationEmailStatus(notification.Id, false, null, false, "Нет пользователей с валидными email адресами");
                    }
                    return;
                }

                var emails = users.Select(u => u.Email).ToList();
                var result = await _emailService.SendBulkNotificationEmailAsync(emails, title, message, type.ToString());

                // Обновляем статус для каждого уведомления
                foreach (var notification in notifications)
                {
                    var user = users.FirstOrDefault(u => u.Id == notification.UserId);
                    if (user != null)
                    {
                        await UpdateNotificationEmailStatus(notification.Id, true, user.Email, result, 
                            result ? null : "Ошибка массовой отправки email");
                    }
                    else
                    {
                        await UpdateNotificationEmailStatus(notification.Id, false, null, false, "Пользователь не найден или у него нет email");
                    }
                }

                if (result)
                {
                    _logger.LogInformation($"Массовые email уведомления успешно отправлены и сохранены для {users.Count} пользователей");
                }
                else
                {
                    _logger.LogWarning($"Не удалось отправить массовые email уведомления {users.Count} пользователям");
                }
            }
            catch (Exception ex)
            {
                // Обновляем все уведомления как неуспешные
                foreach (var notification in notifications)
                {
                    await UpdateNotificationEmailStatus(notification.Id, true, null, false, ex.Message);
                }
                _logger.LogError(ex, "Ошибка отправки массовых email уведомлений");
            }
        }

        // Индивидуальные автоматические уведомления для конкретного пользователя
        public async Task<bool> SendDueReminderToUserAsync(Guid userId)
        {
            try
            {
                var tomorrow = DateTime.UtcNow.AddDays(1).Date;
                var in3Days = DateTime.UtcNow.AddDays(3).Date;

                var reservationsToRemind = await _context.Reservations
                    .Include(r => r.Book)
                    .Include(r => r.User)
                    .Where(r => r.UserId == userId &&
                               r.Status == ReservationStatus.Выдана &&
                               r.ActualReturnDate == null && 
                               r.ExpirationDate.Date >= tomorrow && 
                               r.ExpirationDate.Date <= in3Days)
                    .ToListAsync();

                if (!reservationsToRemind.Any())
                {
                    _logger.LogInformation($"У пользователя {userId} нет книг для напоминания о возврате");
                    return false;
                }

                foreach (var reservation in reservationsToRemind)
                {
                    var daysUntilDue = (reservation.ExpirationDate.Date - DateTime.UtcNow.Date).Days;
                    
                    var dto = new BookDueNotificationDto
                    {
                        UserId = reservation.UserId,
                        BookId = reservation.BookId,
                        BorrowedBookId = null, // Не используем BorrowedBookId для резерваций
                        BookTitle = reservation.Book.Title,
                        BookAuthors = reservation.Book.Authors,
                        DueDate = reservation.ExpirationDate,
                        DaysUntilDue = daysUntilDue
                    };

                    await SendBookDueReminderAsync(dto);
                }

                _logger.LogInformation($"Отправлено {reservationsToRemind.Count} напоминаний о возврате книг пользователю {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка отправки напоминания о возврате книг пользователю {userId}");
                return false;
            }
        }

        public async Task<bool> SendOverdueNotificationToUserAsync(Guid userId)
        {
            try
            {
                var overdueReservations = await _context.Reservations
                    .Include(r => r.Book)
                    .Include(r => r.User)
                    .Where(r => r.UserId == userId &&
                               (r.Status == ReservationStatus.Просрочена || 
                               (r.Status == ReservationStatus.Выдана && r.ExpirationDate < DateTime.UtcNow)) &&
                               r.ActualReturnDate == null)
                    .ToListAsync();

                if (!overdueReservations.Any())
                {
                    _logger.LogInformation($"У пользователя {userId} нет просроченных книг");
                    return false;
                }

                foreach (var reservation in overdueReservations)
                {
                    var daysOverdue = (DateTime.UtcNow - reservation.ExpirationDate).Days;
                    var estimatedFine = daysOverdue * 10m; // 10 рублей за день

                    var dto = new OverdueNotificationDto
                    {
                        UserId = reservation.UserId,
                        BookId = reservation.BookId,
                        BorrowedBookId = null, // Не используем BorrowedBookId для резерваций
                        BookTitle = reservation.Book.Title,
                        BookAuthors = reservation.Book.Authors,
                        DueDate = reservation.ExpirationDate,
                        DaysOverdue = daysOverdue,
                        EstimatedFine = estimatedFine
                    };

                    await SendOverdueNotificationAsync(dto);
                }

                _logger.LogInformation($"Отправлено {overdueReservations.Count} уведомлений о просроченных книгах пользователю {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка отправки уведомления о просроченных книгах пользователю {userId}");
                return false;
            }
        }

        public async Task<bool> SendFineNotificationToUserAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.FineAmount > 0)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogInformation($"Пользователь {userId} не найден или у него нет неоплаченных штрафов");
                    return false;
                }

                // Получаем просроченные резервации для данного пользователя
                var overdueReservations = await _context.Reservations
                    .Include(r => r.Book)
                    .Where(r => r.UserId == userId && 
                               (r.Status == ReservationStatus.Просрочена || 
                               (r.Status == ReservationStatus.Выдана && r.ExpirationDate < DateTime.UtcNow)) &&
                               r.ActualReturnDate == null)
                    .ToListAsync();

                var overdueBooks = overdueReservations
                    .Select(r => new OverdueBookDto
                    {
                        BookId = r.BookId,
                        BookTitle = r.Book.Title,
                        BookAuthors = r.Book.Authors,
                        DueDate = r.ExpirationDate,
                        DaysOverdue = (DateTime.UtcNow - r.ExpirationDate).Days
                    })
                    .ToList();

                var dto = new FineNotificationDto
                {
                    UserId = user.Id,
                    FineAmount = user.FineAmount,
                    PreviousFineAmount = 0, // Можно добавить логику для отслеживания предыдущих штрафов
                    Reason = "Просроченные книги",
                    OverdueBooks = overdueBooks
                };

                await SendFineNotificationAsync(dto);

                _logger.LogInformation($"Отправлено уведомление о штрафе пользователю {userId} на сумму {user.FineAmount:C}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка отправки уведомления о штрафе пользователю {userId}");
                return false;
            }
        }

        private async Task UpdateNotificationEmailStatus(Guid notificationId, bool isEmailSent, string? emailRecipient, bool deliverySuccessful, string? errorMessage)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.IsEmailSent = isEmailSent;
                    notification.EmailSentAt = isEmailSent ? DateTime.UtcNow : null;
                    notification.EmailRecipient = emailRecipient;
                    notification.EmailDeliverySuccessful = deliverySuccessful;
                    notification.EmailErrorMessage = errorMessage;

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка обновления статуса email для уведомления {notificationId}");
            }
        }

        private string GetTemplateNameForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.BookOverdue => "Templates/OverdueEmail.html",
                NotificationType.FineAdded => "Templates/FineEmail.html",
                NotificationType.BookDueSoon => "Templates/ReturnSoonEmail.html",
                NotificationType.BookReturned => "Templates/BookReturnedEmail.html",
                NotificationType.ReservationReady => "Templates/ReservationEmail.html",
                NotificationType.BookReserved => "Templates/ReservationEmail.html",
                _ => "Templates/GeneralEmail.html",
            };
        }
    }
} 