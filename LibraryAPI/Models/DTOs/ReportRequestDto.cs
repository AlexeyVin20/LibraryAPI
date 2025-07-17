
using System;
using System.Collections.Generic;

namespace LibraryAPI.Models.DTOs
{
    public class ReportRequestDto
    {
        public string ReportType { get; set; } // "UserActivity", "BookCirculation", "ReservationsSummary"
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public Dictionary<string, object> Parameters { get; set; } // Для доп. параметров, например, UserId, BookId
    }
} 