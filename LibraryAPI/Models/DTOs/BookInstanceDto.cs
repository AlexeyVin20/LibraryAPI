using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.DTOs
{
    public class BookInstanceDto
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public BookDto Book { get; set; }
        public string InstanceCode { get; set; }
        public string Status { get; set; }
        public string Condition { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime DateAcquired { get; set; }
        public DateTime? DateLastChecked { get; set; }
        public string Notes { get; set; }
        public int? ShelfId { get; set; }
        public ShelfDto Shelf { get; set; }
        public int? Position { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }

    public class BookInstanceCreateDto
    {
        [Required]
        public Guid BookId { get; set; }

        [MaxLength(50)]
        public string? InstanceCode { get; set; } // Необязательное поле, будет генерироваться автоматически

        [Required]
        [MaxLength(100)]
        public string Status { get; set; } = "Доступна";

        [MaxLength(100)]
        public string Condition { get; set; } = "Хорошее";

        public decimal? PurchasePrice { get; set; }

        public DateTime DateAcquired { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int? ShelfId { get; set; }

        public int? Position { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class BookInstanceUpdateDto
    {
        [Required]
        [MaxLength(50)]
        public string InstanceCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Status { get; set; }

        [MaxLength(100)]
        public string Condition { get; set; }

        public decimal? PurchasePrice { get; set; }

        public DateTime DateAcquired { get; set; }

        public DateTime? DateLastChecked { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        public int? ShelfId { get; set; }

        public int? Position { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        public bool IsActive { get; set; }
    }

    public class BookInstanceSimpleDto
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string InstanceCode { get; set; }
        public string Status { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
    }

    public class ForceStatusUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string NewStatus { get; set; }

        [MaxLength(255)]
        public string? Reason { get; set; }

        public bool IgnoreReservation { get; set; } = false;
    }
} 