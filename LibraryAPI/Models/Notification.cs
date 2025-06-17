using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }
        
        [Required]
        public NotificationType Type { get; set; }
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRead { get; set; } = false;
        
        public DateTime? ReadAt { get; set; }
        
        public bool IsDelivered { get; set; } = false;
        
        public DateTime? DeliveredAt { get; set; }
        
        // Дополнительные данные в JSON формате
        public string? AdditionalData { get; set; }
        
        // Внешние ключи для связанных объектов
        public Guid? BookId { get; set; }
        
        public Guid? BorrowedBookId { get; set; }
        
        // Навигационные свойства
        public virtual User User { get; set; }
        public virtual Book? Book { get; set; }
        public virtual BorrowedBook? BorrowedBook { get; set; }
    }

    public enum NotificationType
    {
        BookDueSoon = 1,        // Книга скоро должна быть возвращена
        BookOverdue = 2,        // Книга просрочена
        FineAdded = 3,          // Добавлен штраф
        FineIncreased = 4,      // Штраф увеличен
        BookReturned = 5,       // Книга возвращена
        BookReserved = 6,       // Книга зарезервирована
        ReservationExpired = 7, // Бронь истекла
        NewBookAvailable = 8,   // Новая книга доступна
        AccountBlocked = 9,     // Аккаунт заблокирован
        AccountUnblocked = 10,  // Аккаунт разблокирован
        SystemMaintenance = 11, // Техническое обслуживание
        GeneralInfo = 12        // Общая информация
    }

    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }
} 