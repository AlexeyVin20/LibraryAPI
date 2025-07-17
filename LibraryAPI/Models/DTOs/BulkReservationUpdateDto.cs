
using System;
using LibraryAPI.Models;

namespace LibraryAPI.Models.DTOs
{
    public class BulkReservationUpdateDto
    {
        public Guid Id { get; set; }
        public ReservationStatus Status { get; set; }
    }
} 