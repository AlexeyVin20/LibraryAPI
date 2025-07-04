using LibraryAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.DTOs
{
    public class BulkPushNotificationRequestDto
    {
        [Required]
        public List<Guid> UserIds { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public NotificationType Type { get; set; } = NotificationType.GeneralInfo;
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    }
} 