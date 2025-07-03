using System.Net;
using System.Net.Mail;
using System.Text;
using LibraryAPI.Models;
using LibraryAPI.Models.DTOs;
using Microsoft.Extensions.Options;

namespace LibraryAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            if (!_emailSettings.EnableEmailNotifications)
            {
                _logger.LogInformation("Email уведомления отключены в настройках");
                return false;
            }

            try
            {
                using var client = CreateSmtpClient();
                using var message = new MailMessage();

                message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;
                message.BodyEncoding = Encoding.UTF8;
                message.SubjectEncoding = Encoding.UTF8;

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email успешно отправлен на {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка отправки email на {to}");
                return false;
            }
        }

        public async Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string body, bool isHtml = true)
        {
            if (!_emailSettings.EnableEmailNotifications)
            {
                _logger.LogInformation("Email уведомления отключены в настройках");
                return false;
            }

            var successCount = 0;
            var totalCount = recipients.Count;

            foreach (var recipient in recipients)
            {
                if (await SendEmailAsync(recipient, subject, body, isHtml))
                {
                    successCount++;
                }
                
                // Небольшая задержка между отправками для избежания спама
                await Task.Delay(100);
            }

            _logger.LogInformation($"Массовая отправка завершена: {successCount}/{totalCount} успешно");
            return successCount > 0;
        }

        public async Task<bool> SendNotificationEmailAsync(string email, NotificationDto notification)
        {
            var subject = $"[Библиотека] {notification.Title}";
            var body = GenerateNotificationEmailBody(notification);
            
            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendBulkNotificationEmailAsync(List<string> emails, string subject, string message, string notificationType)
        {
            var emailSubject = $"[Библиотека] {subject}";
            var body = GenerateBulkNotificationEmailBody(subject, message, notificationType);
            
            return await SendBulkEmailAsync(emails, emailSubject, body, true);
        }

        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Timeout = _emailSettings.TimeoutSeconds * 1000
            };

            // Устанавливаем аутентификацию только если указаны логин и пароль
            if (!string.IsNullOrEmpty(_emailSettings.SmtpUsername) && !string.IsNullOrEmpty(_emailSettings.SmtpPassword))
            {
                client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                _logger.LogInformation("SMTP аутентификация включена");
            }
            else
            {
                client.Credentials = null; // Анонимная отправка
                _logger.LogInformation("SMTP аутентификация отключена (анонимная отправка)");
            }

            return client;
        }

        private string GenerateNotificationEmailBody(NotificationDto notification)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='utf-8'>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine("        .header { background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin-bottom: 20px; }");
            sb.AppendLine("        .content { background-color: #ffffff; padding: 20px; border: 1px solid #dee2e6; border-radius: 5px; }");
            sb.AppendLine("        .footer { margin-top: 20px; font-size: 12px; color: #6c757d; }");
            sb.AppendLine("        .priority-high { border-left: 4px solid #dc3545; }");
            sb.AppendLine("        .priority-critical { border-left: 4px solid #dc3545; background-color: #f8d7da; }");
            sb.AppendLine("        .priority-normal { border-left: 4px solid #28a745; }");
            sb.AppendLine("        .book-info { background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 10px 0; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class='container'>");
            sb.AppendLine("        <div class='header'>");
            sb.AppendLine("            <h2>Система управления библиотекой</h2>");
            sb.AppendLine("        </div>");
            
            var priorityClass = notification.Priority?.ToLower() switch
            {
                "high" => "priority-high",
                "critical" => "priority-critical",
                _ => "priority-normal"
            };
            
            sb.AppendLine($"        <div class='content {priorityClass}'>");
            sb.AppendLine($"            <h3>{notification.Title}</h3>");
            sb.AppendLine($"            <p>{notification.Message}</p>");
            
            if (!string.IsNullOrEmpty(notification.BookTitle))
            {
                sb.AppendLine("            <div class='book-info'>");
                sb.AppendLine("                <strong>Информация о книге:</strong><br>");
                sb.AppendLine($"                <strong>Название:</strong> {notification.BookTitle}<br>");
                if (!string.IsNullOrEmpty(notification.BookAuthors))
                {
                    sb.AppendLine($"                <strong>Авторы:</strong> {notification.BookAuthors}<br>");
                }
                sb.AppendLine("            </div>");
            }
            
            sb.AppendLine($"            <p><strong>Тип уведомления:</strong> {GetNotificationTypeDisplay(notification.Type)}</p>");
            sb.AppendLine($"            <p><strong>Приоритет:</strong> {GetPriorityDisplay(notification.Priority)}</p>");
            sb.AppendLine($"            <p><strong>Дата создания:</strong> {notification.CreatedAt:dd.MM.yyyy HH:mm}</p>");
            sb.AppendLine("        </div>");
            
            sb.AppendLine("        <div class='footer'>");
            sb.AppendLine("            <p>Это автоматическое уведомление от системы управления библиотекой.</p>");
            sb.AppendLine("            <p>Пожалуйста, не отвечайте на это письмо.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }

        private string GenerateBulkNotificationEmailBody(string title, string message, string notificationType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='utf-8'>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine("        .header { background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin-bottom: 20px; }");
            sb.AppendLine("        .content { background-color: #ffffff; padding: 20px; border: 1px solid #dee2e6; border-radius: 5px; }");
            sb.AppendLine("        .footer { margin-top: 20px; font-size: 12px; color: #6c757d; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class='container'>");
            sb.AppendLine("        <div class='header'>");
            sb.AppendLine("            <h2>Система управления библиотекой</h2>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class='content'>");
            sb.AppendLine($"            <h3>{title}</h3>");
            sb.AppendLine($"            <p>{message}</p>");
            sb.AppendLine($"            <p><strong>Тип уведомления:</strong> {GetNotificationTypeDisplay(notificationType)}</p>");
            sb.AppendLine($"            <p><strong>Дата отправки:</strong> {DateTime.UtcNow:dd.MM.yyyy HH:mm}</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class='footer'>");
            sb.AppendLine("            <p>Это массовое уведомление от системы управления библиотекой.</p>");
            sb.AppendLine("            <p>Пожалуйста, не отвечайте на это письмо.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }

        private string GetNotificationTypeDisplay(string? type)
        {
            return type switch
            {
                "BookDueSoon" => "Напоминание о возврате книги",
                "BookOverdue" => "Просроченная книга",
                "BookReturned" => "Книга возвращена",
                "FineAdded" => "Начислен штраф",
                "GeneralInfo" => "Общая информация",
                "SystemAlert" => "Системное уведомление",
                _ => "Неизвестный тип"
            };
        }

        private string GetPriorityDisplay(string? priority)
        {
            return priority switch
            {
                "Low" => "Низкий",
                "Normal" => "Обычный",
                "High" => "Высокий",
                "Critical" => "Критический",
                _ => "Обычный"
            };
        }
    }
} 