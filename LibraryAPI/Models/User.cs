using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [InverseProperty("User")]
        public List<UserRole>? UserRoles { get; set; }

        // Навигационное свойство для взятых книг
        public List<BorrowedBook>? BorrowedBooks { get; set; }
        
        // Избранные книги
        [InverseProperty("User")]
        public List<FavoriteBook>? FavoriteBooks { get; set; }
    }

    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [InverseProperty("Role")]
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class UserRole
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
