﻿namespace LibraryAPI.Models.DTOs
{
    public class ReservationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public Guid? BookInstanceId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public UserDto User { get; set; }
        public BookDto Book { get; set; }
        public BookInstanceDto? BookInstance { get; set; }
    }

    public class ReservationCreateDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
    }

    public class ReservationUpdateDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
    }
}