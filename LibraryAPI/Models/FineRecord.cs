using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class FineRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid UserId { get; set; }
        
        public Guid? ReservationId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }
        
        public int? OverdueDays { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? PaidAt { get; set; }
        
        public bool IsPaid { get; set; } = false;
        
        [MaxLength(1000)]
        public string? Notes { get; set; }
        
        // Поля для предотвращения дублирования штрафов
        public DateTime? CalculatedForDate { get; set; } // За какую дату рассчитан штраф
        
        [MaxLength(100)]
        public string? FineType { get; set; } // Тип штрафа (например, "Overdue", "Damage", "Lost")
        
        // Навигационные свойства
        public virtual User User { get; set; }
        public virtual Reservation? Reservation { get; set; }
    }
} 