using LibraryAPI.Models.DTOs;

namespace LibraryAPI.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string body, bool isHtml = true);
        Task<bool> SendNotificationEmailAsync(string email, NotificationDto notification);
        Task<bool> SendBulkNotificationEmailAsync(List<string> emails, string subject, string message, string notificationType);
    }
} 