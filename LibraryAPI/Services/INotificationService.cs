using LibraryAPI.Models;
using LibraryAPI.Models.DTOs;
using System.Collections.Generic;

namespace LibraryAPI.Services
{
    public interface INotificationService
    {
        // Основные методы для создания уведомлений
        Task<Notification> CreateNotificationAsync(NotificationCreateDto dto);
        Task<List<Notification>> CreateBulkNotificationsAsync(NotificationPushDto dto);
        
        // Специализированные методы для библиотечных уведомлений
        Task SendBookDueReminderAsync(BookDueNotificationDto dto);
        Task SendOverdueNotificationAsync(OverdueNotificationDto dto);
        Task SendFineNotificationAsync(FineNotificationDto dto);
        Task SendBookReturnedNotificationAsync(Guid userId, string bookTitle, Guid bookId);
        
        // Массовые уведомления для пользователей с книгами/штрафами
        Task SendDueRemindersToUsersWithBooksAsync();
        Task SendOverdueNotificationsToUsersWithBooksAsync();
        Task SendFineNotificationsToUsersWithFinesAsync();
        
        // Индивидуальные автоматические уведомления для конкретного пользователя
        Task<bool> SendDueReminderToUserAsync(Guid userId);
        Task<bool> SendOverdueNotificationToUserAsync(Guid userId);
        Task<bool> SendFineNotificationToUserAsync(Guid userId);
        
        // Управление уведомлениями
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task<bool> MarkMultipleAsReadAsync(List<Guid> notificationIds, Guid userId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);
        
        // Получение уведомлений
        Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool? isRead = null, int page = 1, int pageSize = 20);
        Task<NotificationStatsDto> GetUserNotificationStatsAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        
        // Административные методы для получения всех уведомлений
        Task<List<AdminNotificationDto>> GetAllNotificationsAsync(bool? isRead = null, int page = 1, int pageSize = 20, Guid? userId = null, NotificationType? type = null, NotificationPriority? priority = null);
        Task<AdminNotificationStatsDto> GetAllNotificationStatsAsync();
        
        // Push уведомления (реальное время)
        Task SendPushNotificationAsync(Guid userId, string title, string message, NotificationType type);
        Task SendPushNotificationToMultipleUsersAsync(List<Guid> userIds, string title, string message, NotificationType type);
        
        // Автоматические проверки
        Task CheckAndSendDueRemindersAsync();
        Task CheckAndSendOverdueNotificationsAsync();
        Task CheckAndUpdateFinesAsync();
        
        // Очистка старых уведомлений
        Task CleanupOldNotificationsAsync(int daysOld = 90);
        
        // Email уведомления
        Task<bool> SendEmailNotificationAsync(Guid userId, string title, NotificationType type, Dictionary<string, object> templateData);
        Task<bool> SendBulkEmailNotificationAsync(List<Guid> userIds, string title, string message, NotificationType type);
    }
} 