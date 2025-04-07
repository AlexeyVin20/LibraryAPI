using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class Author
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }

        [MaxLength(1000)]
        public string? Biography { get; set; }

        [MaxLength(255)]
        public string? Nationality { get; set; }

        public List<Book> Books { get; set; } = new List<Book>();
    }
}