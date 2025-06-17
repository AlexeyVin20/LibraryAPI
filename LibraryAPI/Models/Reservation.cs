using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class Reservation
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }

        [Required]
        public Guid BookId { get; set; }
        public Book Book { get; set; }

        public DateTime ReservationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public ReservationStatus Status { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }
    }

    public enum ReservationStatus
    {
        Обрабатывается,
        Одобрена,
        Отменена,
        Истекла,
        Выдана,
        Возвращена,
        Просрочена,
        Отменена_пользователем,
    }
}