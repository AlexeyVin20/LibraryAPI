using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.DTOs
{
    public class BookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public string Genre { get; set; }
        public string Categorization { get; set; }
        public string UDK { get; set; }
        public string BBK { get; set; }
        public string ISBN { get; set; }
        public string Cover { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public int? PublicationYear { get; set; }
        public string Publisher { get; set; }
        public int? PageCount { get; set; }
        public string Language { get; set; }
        public int AvailableCopies { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateModified { get; set; }
        public string Edition { get; set; }
        public decimal? Price { get; set; }
        public string Format { get; set; }
        public string OriginalTitle { get; set; }
        public string OriginalLanguage { get; set; }
        public DateTime? DateRead { get; set; }
        public bool? IsEbook { get; set; }
        public string Condition { get; set; }
        public int? ShelfId { get; set; }
        public int Position { get; internal set; }
    }

    public class BookCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Authors { get; set; }

        [MaxLength(100)]
        public string Genre { get; set; }

        [MaxLength(100)]
        public string Categorization { get; set; }

        public string UDK { get; set; }
        public string BBK { get; set; }

        [MaxLength(20)]
        public string ISBN { get; set; }

        [MaxLength(255)]
        public string Cover { get; set; }

        public string Description { get; set; }
        public string Summary { get; set; }
        public int? PublicationYear { get; set; }

        [MaxLength(100)]
        public string Publisher { get; set; }

        public int? PageCount { get; set; }

        [MaxLength(50)]
        public string Language { get; set; }

        public int AvailableCopies { get; set; }

        [MaxLength(50)]
        public string Edition { get; set; }

        public decimal? Price { get; set; }

        [MaxLength(100)]
        public string Format { get; set; }

        [MaxLength(255)]
        public string OriginalTitle { get; set; }

        [MaxLength(50)]
        public string OriginalLanguage { get; set; }

        public bool? IsEbook { get; set; }

        [MaxLength(100)]
        public string Condition { get; set; }

        public int? ShelfId { get; set; }
    }

    public class BookUpdateDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Authors { get; set; }

        [MaxLength(100)]
        public string Genre { get; set; }

        [MaxLength(100)]
        public string Categorization { get; set; }

        public string UDK { get; set; }
        public string BBK { get; set; }

        [MaxLength(20)]
        public string ISBN { get; set; }

        [MaxLength(255)]
        public string Cover { get; set; }

        public string Description { get; set; }
        public string Summary { get; set; }
        public int? PublicationYear { get; set; }

        [MaxLength(100)]
        public string Publisher { get; set; }

        public int? PageCount { get; set; }

        [MaxLength(50)]
        public string Language { get; set; }

        public int AvailableCopies { get; set; }

        [MaxLength(50)]
        public string Edition { get; set; }

        public decimal? Price { get; set; }

        [MaxLength(100)]
        public string Format { get; set; }

        [MaxLength(255)]
        public string OriginalTitle { get; set; }

        [MaxLength(50)]
        public string OriginalLanguage { get; set; }

        public bool? IsEbook { get; set; }

        [MaxLength(100)]
        public string Condition { get; set; }

        public int? ShelfId { get; set; }
    }
}
