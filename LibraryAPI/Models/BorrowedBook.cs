using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class BorrowedBook
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        public Guid BookId { get; set; }
        
        [Required]
        public DateTime BorrowDate { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }
        
        public DateTime? ReturnDate { get; set; }
        
        public decimal? FineAmount { get; set; } = 0;
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        public bool IsReturned => ReturnDate.HasValue;
        
        public bool IsOverdue => !IsReturned && DueDate < DateTime.UtcNow;
        
        public int DaysOverdue => IsOverdue ? (int)(DateTime.UtcNow - DueDate).TotalDays : 0;
        
        // Навигационные свойства
        public virtual User User { get; set; }
        public virtual Book Book { get; set; }
    }
} 