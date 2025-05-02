using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LibraryAPI.Models
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [MaxLength(255)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(20)]
        public string PassportNumber { get; set; }

        [Required]
        [MaxLength(255)]
        public string PassportIssuedBy { get; set; }

        public DateTime? PassportIssuedDate { get; set; }

        public string Address { get; set; }

        [Required]
        [MinLength(4)]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public AuthUserDto User { get; set; }
    }

    public class AuthUserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; }
        public string PassportIssuedBy { get; set; }
        public DateTime? PassportIssuedDate { get; set; }
        public string Address { get; set; }
        public DateTime DateRegistered { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int BorrowedBooksCount { get; set; }
        public int MaxBooksAllowed { get; set; }
        public int LoanPeriodDays { get; set; }
        public decimal FineAmount { get; set; }
        public string[] Roles { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
} 