using LibraryAPI.Models;

namespace LibraryAPI.Models.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public DateTime DateRegistered { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool PasswordResetRequired { get; set; }
        public int? BorrowedBooksCount { get; set; }
        public int? MaxBooksAllowed { get; set; }
        public int LoanPeriodDays { get; set; }
        public decimal FineAmount { get; set; }
        public List<UserRoleDto>? UserRoles { get; set; }
        public List<BorrowedBook>? BorrowedBooks { get; set; }
        public List<Book>? FavoriteBooks { get; set; }
    }

    public class UserCreateDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public DateTime DateRegistered { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public bool PasswordResetRequired { get; set; }
        public int? BorrowedBooksCount { get; set; }
        public int? MaxBooksAllowed { get; set; }
        public int LoanPeriodDays { get; set; }
        public decimal FineAmount { get; set; }
        public List<int>? RoleIds { get; set; }
        public List<BorrowedBook>? BorrowedBooks { get; set; }
        public List<Book>? FavoriteBooks { get; set; }
    }

    public class UserUpdateDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public DateTime DateRegistered { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
        public bool PasswordResetRequired { get; set; }
        public int? BorrowedBooksCount { get; set; }
        public int? MaxBooksAllowed { get; set; }
        public int LoanPeriodDays { get; set; }
        public decimal FineAmount { get; set; }
        public List<int>? RoleIds { get; set; } 
        public List<BorrowedBook>? BorrowedBooks { get; set; }
        public List<Book>? FavoriteBooks { get; set; }
    }

    public class UserChangePasswordDto
    {
        public Guid Id { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class UserRoleDto
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class UserRoleCreateDto
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
    }

    public class UserRoleUpdateDto
    {
        public Guid UserId { get; set; }
        public int OldRoleId { get; set; }
        public int NewRoleId { get; set; }
    }

    public class UserRoleDeleteDto
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
    }

    public class UserUpdateBorrowedCountDto
    {
        public int BorrowedBooksCount { get; set; }
    }

    public class AssignRolesDto
    {
        public List<Guid> UserIds { get; set; }
        public int RoleId { get; set; }
    }

    public class RemoveRolesDto
    {
        public List<Guid> UserIds { get; set; }
        public int RoleId { get; set; }
    }

    public class ExtendReservationDto
    {
        public int ExtensionDays { get; set; }
        public string? Notes { get; set; }
    }

    // DTO для работы со штрафами
    public class UserFineCreateDto
    {
        public Guid? ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public int? OverdueDays { get; set; }
        public string? Notes { get; set; }
        public string? FineType { get; set; } = "Overdue";
    }

    public class FineRecordDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public int? OverdueDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public bool IsPaid { get; set; }
        public string? Notes { get; set; }
        public DateTime? CalculatedForDate { get; set; }
        public string? FineType { get; set; }
        
        public string? UserName { get; set; }
        public string? BookTitle { get; set; }
        public string? ReservationStatus { get; set; }
    }

    public class UserFinePaymentDto
    {
        public Guid FineId { get; set; }
        public string? PaymentNotes { get; set; }
    }

    public class UserFineHistoryDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalFineAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal UnpaidAmount { get; set; }
        public int TotalFines { get; set; }
        public int PaidFines { get; set; }
        public int UnpaidFines { get; set; }
        public List<FineRecordDto> FineRecords { get; set; } = new List<FineRecordDto>();
    }
}
