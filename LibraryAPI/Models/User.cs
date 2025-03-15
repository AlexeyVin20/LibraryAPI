using System;
using System.Collections.Generic;
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
        public string? Phone { get; set; }

        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(20)]
        public string? PassportNumber { get; set; }

        [Required]
        [MaxLength(255)]
        public string? PassportIssuedBy { get; set; }

        public DateTime? PassportIssuedDate { get; set; }

        public string? Address { get; set; }
        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;

        // Новые поля
        [MaxLength(50)]
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginDate { get; set; }

        public int? BorrowedBooksCount { get; set; } = 0;

        public int? MaxBooksAllowed { get; set; } = 5;

        public int LoanPeriodDays { get; set; } = 14;

        public decimal FineAmount { get; set; } = 0; // кол-во задолженностей

        // Система ролей
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // Навигационное свойство для связанных книг
        public List<Book> BorrowedBooks { get; set; } = new List<Book>();
    }

    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class UserRole
    {
        [Key]
        public Guid UserId { get; set; }
        public User User { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
