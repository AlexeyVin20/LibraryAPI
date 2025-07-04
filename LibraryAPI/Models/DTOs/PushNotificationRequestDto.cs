using LibraryAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.DTOs
{
    public class PushNotificationRequestDto
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public NotificationType Type { get; set; } = NotificationType.GeneralInfo;
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    }
} 