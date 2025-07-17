
using System;
using System.Collections.Generic;

namespace LibraryAPI.Models.DTOs
{
    public class BookAvailabilityDto
    {
        public int AvailableCount { get; set; }
        public DateTime? NearestReturnDate { get; set; }
        public List<ReservationQueueItemDto> ReservationQueue { get; set; }
    }

    public class ReservationQueueItemDto
    {
        public Guid ReservationId { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; }
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; }
    }
} 