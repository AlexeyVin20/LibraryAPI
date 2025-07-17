
using System.Collections.Generic;

namespace LibraryAPI.Models.DTOs
{
    public class BookStatisticsDto
    {
        public List<BookDto> MostPopularBooks { get; set; }
        public Dictionary<string, int> GenreDistribution { get; set; }
        public AvailabilityStats Availability { get; set; }
    }

    public class AvailabilityStats
    {
        public int Available { get; set; }
        public int NotAvailable { get; set; }
        public int Total { get; set; }
    }
} 