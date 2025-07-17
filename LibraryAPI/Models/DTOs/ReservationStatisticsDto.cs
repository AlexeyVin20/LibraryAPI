
using System.Collections.Generic;

namespace LibraryAPI.Models.DTOs
{
    public class ReservationStatisticsDto
    {
        public int TotalReservations { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; }
        public Dictionary<string, int> Dynamics { get; set; }
    }
} 