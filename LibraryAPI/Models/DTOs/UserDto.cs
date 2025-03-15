namespace LibraryAPI.Models.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public bool IsActive { get; set; }
        public int BorrowedBooksCount { get; set; }
        public int MaxBooksAllowed { get; set; }
        public decimal FineAmount { get; set; }
        public DateTime DateRegistered { get; set; }
        public string? Phone { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string? PassportNumber { get; set; }
        public string? PassportIssuedBy { get; set; }
        public string? PasswordHash { get; set; }
    }

    public class UserCreateDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public bool IsActive { get; set; }
        public int BorrowedBooksCount { get; set; }
        public int MaxBooksAllowed { get; set; }
        public decimal FineAmount { get; set; }
        public DateTime DateRegistered { get; set; }
        public string? Phone { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string? PassportNumber { get; set; }
        public string? PassportIssuedBy { get; set; }
        public string? PasswordHash { get; set; }
    }

    public class UserUpdateDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public bool IsActive { get; set; }
        public int BorrowedBooksCount { get; set; }
        public int MaxBooksAllowed { get; set; }
        public decimal FineAmount { get; set; }
        public DateTime DateRegistered { get; set; }
        public string? Phone { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string? PassportNumber { get; set; }
        public string? PassportIssuedBy { get; set; }
        public string? PasswordHash { get; set; }
    }
}
