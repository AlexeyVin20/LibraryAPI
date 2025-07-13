using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models
{
    /// <summary>
    /// Запись истории вызова инструмента ИИ-ассистента. Фиксирует состояние данных до и после применения POST/PUT/DELETE.
    /// </summary>
    public class DialogHistory
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Уникальный идентификатор диалога (сеанса общения).
        /// </summary>
        [Required]
        public string ConversationId { get; set; }

        /// <summary>
        /// Название инструмента, который был вызван (front-end имя функции).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ToolName { get; set; }

        [Required]
        [MaxLength(10)]
        public string HttpMethod { get; set; }

        [Required]
        public string Endpoint { get; set; }

        /// <summary>
        /// Параметры, переданные в запрос, сериализованные в JSON.
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Снимок состояния данных до изменения (JSON).
        /// </summary>
        public string BeforeState { get; set; }

        /// <summary>
        /// Снимок состояния данных после изменения (JSON).
        /// </summary>
        public string AfterState { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Пользователь, от имени которого выполнялся вызов.
        /// </summary>
        // public Guid? UserId { get; set; }

        // [ForeignKey("UserId")]
        // public User User { get; set; }
    }
} 