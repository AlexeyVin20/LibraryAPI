using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LibraryAPI.Models;

namespace LibraryAPI.Models.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? AdditionalData { get; set; }
        public Guid? BookId { get; set; }
        public Guid? BorrowedBookId { get; set; }
        
        // Email уведомления
        public bool IsEmailSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
        public string? EmailRecipient { get; set; }
        public bool EmailDeliverySuccessful { get; set; }
        public string? EmailErrorMessage { get; set; }
        
        // Дополнительная информация о связанных объектах
        public string? BookTitle { get; set; }
        public string? BookAuthors { get; set; }
        public string? BookCover { get; set; }
    }

    public class NotificationCreateDto
    {
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
        
        public string? AdditionalData { get; set; }
        
        public Guid? BookId { get; set; }
        
        public Guid? BorrowedBookId { get; set; }

        public Dictionary<string, object> TemplateData { get; set; } = new Dictionary<string, object>();
    }

    public class NotificationMarkReadDto
    {
        [Required]
        public Guid NotificationId { get; set; }
    }

    public class NotificationBulkMarkReadDto
    {
        [Required]
        public List<Guid> NotificationIds { get; set; }
    }

    public class NotificationPushDto
    {
        [Required]
        public List<Guid> UserIds { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }
        
        [Required]
        public NotificationType Type { get; set; }
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public string? AdditionalData { get; set; }
    }

    public class NotificationStatsDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int DeliveredNotifications { get; set; }
        public int PendingNotifications { get; set; }
        public int EmailsSent { get; set; }
        public int EmailsDelivered { get; set; }
        public int EmailsFailed { get; set; }
        public Dictionary<string, int> NotificationsByType { get; set; }
        public Dictionary<string, int> NotificationsByPriority { get; set; }
    }

    public class AdminNotificationStatsDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int DeliveredNotifications { get; set; }
        public int PendingNotifications { get; set; }
        public int TotalUsers { get; set; }
        public int UsersWithNotifications { get; set; }
        public int UsersWithUnreadNotifications { get; set; }
        public int EmailsSent { get; set; }
        public int EmailsDelivered { get; set; }
        public int EmailsFailed { get; set; }
        public Dictionary<string, int> NotificationsByType { get; set; }
        public Dictionary<string, int> NotificationsByPriority { get; set; }
        public Dictionary<string, int> NotificationsLastDays { get; set; }
        public Dictionary<string, int> TopUsersWithNotifications { get; set; }
    }

    public class BookDueNotificationDto
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public Guid? BorrowedBookId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthors { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysUntilDue { get; set; }
        public Dictionary<string, object> TemplateData { get; set; } = new Dictionary<string, object>();
    }

    public class OverdueNotificationDto
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public Guid? BorrowedBookId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthors { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public decimal EstimatedFine { get; set; }
        public Dictionary<string, object> TemplateData { get; set; } = new Dictionary<string, object>();
    }

    public class FineNotificationDto
    {
        public Guid UserId { get; set; }
        public decimal FineAmount { get; set; }
        public decimal PreviousFineAmount { get; set; }
        public string Reason { get; set; }
        public List<OverdueBookDto> OverdueBooks { get; set; }
        public Dictionary<string, object> TemplateData { get; set; } = new Dictionary<string, object>();
    }

    public class OverdueBookDto
    {
        public Guid BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthors { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class AdminNotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? AdditionalData { get; set; }
        public Guid? BookId { get; set; }
        public Guid? BorrowedBookId { get; set; }
        
        // Email уведомления
        public bool IsEmailSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
        public string? EmailRecipient { get; set; }
        public bool EmailDeliverySuccessful { get; set; }
        public string? EmailErrorMessage { get; set; }
        
        // Дополнительная информация о связанных объектах
        public string? BookTitle { get; set; }
        public string? BookAuthors { get; set; }
        public string? BookCover { get; set; }
    }
} 