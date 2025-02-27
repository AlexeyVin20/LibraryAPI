using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        public DateTime DateOfBirth { get; set; }

        // Паспортные данные
        [Required]
        [MaxLength(20)]
        public string PassportNumber { get; set; }

        [Required]
        [MaxLength(255)]
        public string PassportIssuedBy { get; set; }

        public DateTime PassportIssuedDate { get; set; }

        // Дополнительно
        public string Address { get; set; }
        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
    }
}
