using LibraryAPI.Data;
using LibraryAPI.Models;
using LibraryAPI.Models.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using LibraryAPI.Hubs;

namespace LibraryAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly LibraryDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            LibraryDbContext context, 
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<Notification> CreateNotificationAsync(NotificationCreateDto dto)
        {
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
                BorrowedBookId = dto.BorrowedBookId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Отправка push уведомления
            await SendPushNotificationAsync(dto.UserId, dto.Title, dto.Message, dto.Type);

            _logger.LogInformation($"Создано уведомление {notification.Id} для пользователя {dto.UserId}");
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

            _logger.LogInformation($"Создано {notifications.Count} массовых уведомлений");
            return notifications;
        }

        public async Task SendBookDueReminderAsync(BookDueNotificationDto dto)
        {
            var title = "Напоминание о возврате книги";
            var message = $"Книга \"{dto.BookTitle}\" должна быть возвращена {dto.DueDate:dd.MM.yyyy}. " +
                         $"Осталось дней: {dto.DaysUntilDue}";

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
                BorrowedBookId = dto.BorrowedBookId,
                AdditionalData = additionalData
            };

            await CreateNotificationAsync(notificationDto);
        }

        public async Task SendOverdueNotificationAsync(OverdueNotificationDto dto)
        {
            var title = "Просроченная книга";
            var message = $"Книга \"{dto.BookTitle}\" просрочена на {dto.DaysOverdue} дн. " +
                         $"Предполагаемый штраф: {dto.EstimatedFine:C}";

            var additionalData = JsonSerializer.Serialize(new
            {
                BookId = dto.BookId,
                BorrowedBookId = dto.BorrowedBookId,
                DueDate = dto.DueDate,
                DaysOverdue = dto.DaysOverdue,
                EstimatedFine = dto.EstimatedFine
            });

            var notificationDto = new NotificationCreateDto
            {
                UserId = dto.UserId,
                Title = title,
                Message = message,
                Type = NotificationType.BookOverdue,
                Priority = NotificationPriority.Critical,
                BookId = dto.BookId,
                BorrowedBookId = dto.BorrowedBookId,
                AdditionalData = additionalData
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
                BookId = bookId
            };

            await CreateNotificationAsync(notificationDto);
        }

        public async Task SendDueRemindersToUsersWithBooksAsync()
        {
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var in3Days = DateTime.UtcNow.AddDays(3).Date;

            var booksToRemind = await _context.BorrowedBooks
                .Include(bb => bb.Book)
                .Include(bb => bb.User)
                .Where(bb => bb.ReturnDate == null && 
                            bb.DueDate.Date >= tomorrow && 
                            bb.DueDate.Date <= in3Days)
                .ToListAsync();

            foreach (var borrowedBook in booksToRemind)
            {
                var daysUntilDue = (borrowedBook.DueDate.Date - DateTime.UtcNow.Date).Days;
                
                var dto = new BookDueNotificationDto
                {
                    UserId = borrowedBook.UserId,
                    BookId = borrowedBook.BookId,
                    BorrowedBookId = borrowedBook.Id,
                    BookTitle = borrowedBook.Book.Title,
                    BookAuthors = borrowedBook.Book.Authors,
                    DueDate = borrowedBook.DueDate,
                    DaysUntilDue = daysUntilDue
                };

                await SendBookDueReminderAsync(dto);
            }

            _logger.LogInformation($"Отправлено {booksToRemind.Count} напоминаний о возврате книг");
        }

        public async Task SendOverdueNotificationsToUsersWithBooksAsync()
        {
            var overdueBooks = await _context.BorrowedBooks
                .Include(bb => bb.Book)
                .Include(bb => bb.User)
                .Where(bb => bb.ReturnDate == null && bb.DueDate < DateTime.UtcNow)
                .ToListAsync();

            foreach (var borrowedBook in overdueBooks)
            {
                var daysOverdue = (DateTime.UtcNow - borrowedBook.DueDate).Days;
                var estimatedFine = daysOverdue * 10m; // 10 рублей за день

                var dto = new OverdueNotificationDto
                {
                    UserId = borrowedBook.UserId,
                    BookId = borrowedBook.BookId,
                    BorrowedBookId = borrowedBook.Id,
                    BookTitle = borrowedBook.Book.Title,
                    BookAuthors = borrowedBook.Book.Authors,
                    DueDate = borrowedBook.DueDate,
                    DaysOverdue = daysOverdue,
                    EstimatedFine = estimatedFine
                };

                await SendOverdueNotificationAsync(dto);
            }

            _logger.LogInformation($"Отправлено {overdueBooks.Count} уведомлений о просроченных книгах");
        }

        public async Task SendFineNotificationsToUsersWithFinesAsync()
        {
            var usersWithFines = await _context.Users
                .Where(u => u.FineAmount > 0)
                .Include(u => u.BorrowedBooks)
                .ThenInclude(bb => bb.Book)
                .ToListAsync();

            foreach (var user in usersWithFines)
            {
                var overdueBooks = user.BorrowedBooks
                    .Where(bb => bb.ReturnDate == null && bb.DueDate < DateTime.UtcNow)
                    .Select(bb => new OverdueBookDto
                    {
                        BookId = bb.BookId,
                        BookTitle = bb.Book.Title,
                        BookAuthors = bb.Book.Authors,
                        DueDate = bb.DueDate,
                        DaysOverdue = (DateTime.UtcNow - bb.DueDate).Days
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
    }
} 