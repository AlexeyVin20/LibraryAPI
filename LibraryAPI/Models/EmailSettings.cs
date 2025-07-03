namespace LibraryAPI.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string? SmtpUsername { get; set; } = null;
        public string? SmtpPassword { get; set; } = null;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public bool EnableEmailNotifications { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 30;
    }
} 