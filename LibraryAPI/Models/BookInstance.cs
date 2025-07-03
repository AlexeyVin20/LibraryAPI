using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class BookInstance
    {
        public Guid Id { get; set; }

        public Guid BookId { get; set; }
        public Book Book { get; set; }

        [MaxLength(50)]
        public string InstanceCode { get; set; } // Уникальный код экземпляра (например, штрих-код)

        [MaxLength(100)]
        public string Status { get; set; } // Available, Borrowed, Reserved, Damaged, Lost

        [MaxLength(100)]
        public string Condition { get; set; } // New, Good, Fair, Poor

        public decimal? PurchasePrice { get; set; }

        public DateTime DateAcquired { get; set; }

        public DateTime? DateLastChecked { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } // Заметки о состоянии, повреждениях и т.д.

        public int? ShelfId { get; set; }
        public Shelf Shelf { get; set; }

        public int? Position { get; set; } // Позиция на полке

        [MaxLength(100)]
        public string Location { get; set; } // Дополнительная информация о местоположении

        public bool IsActive { get; set; } = true; // Активен ли экземпляр

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
} 