namespace LibraryAPI.Models.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PassportNumber { get; set; }
        public string? PassportIssuedBy { get; set; }
        public DateTime? PassportIssuedDate { get; set; }
        public string? Address { get; set; }
        public DateTime DateRegistered { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int? BorrowedBooksCount { get; set; }
        public int? MaxBooksAllowed { get; set; }
        public int LoanPeriodDays { get; set; }
        public decimal FineAmount { get; set; }
        public List<UserRole>? UserRoles { get; set; }
        public List<Book>? BorrowedBooks { get; set; }
    }

    public class UserCreateDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PassportNumber { get; set; }
        public string? PassportIssuedBy { get; set; }
        public DateTime? PassportIssuedDate { get; set; }
        public string? Address { get; set; }
        public DateTime DateRegistered { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public int? BorrowedBooksCount { get; set; }
        public int? MaxBooksAllowed { get; set; }
        public int LoanPeriodDays { get; set; }
        public decimal FineAmount { get; set; }
        public List<UserRole>? UserRoles { get; set; }
        public List<Book>? BorrowedBooks { get; set; }
    }

    public class UserUpdateDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PassportNumber { get; set; }
        public string? PassportIssuedBy { get; set; }
        public DateTime? PassportIssuedDate { get; set; }
        public string? Address { get; set; }
        public DateTime DateRegistered { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
        public int? BorrowedBooksCount { get; set; }
        public int? MaxBooksAllowed { get; set; }
        public int LoanPeriodDays { get; set; }
        public decimal FineAmount { get; set; }
        public List<UserRole>? UserRoles { get; set; }
        public List<Book>? BorrowedBooks { get; set; }
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
}
