using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class Book
    {
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        // Теперь в книге хранится строка с именами авторов
        [MaxLength(500)]
        public string Authors { get; set; }

        [MaxLength(100)]
        public string Genre { get; set; }

        [MaxLength(100)]
        public string Categorization { get; set; }

        [MaxLength(20)]
        public string ISBN { get; set; }

        [MaxLength(255)]
        public string Cover { get; set; }

        public string Description { get; set; }
        public string Summary { get; set; }
        public int PublicationYear { get; set; }

        [MaxLength(100)]
        public string Publisher { get; set; }

        public int PageCount { get; set; }

        [MaxLength(50)]
        public string Language { get; set; }

        public int AvailableCopies { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateModified { get; set; }
    }
}
