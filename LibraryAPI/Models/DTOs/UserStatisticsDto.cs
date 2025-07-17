
using System.Collections.Generic;

namespace LibraryAPI.Models.DTOs
{
    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public Dictionary<string, int> RolesDistribution { get; set; }
        public double AverageLoanPeriodDays { get; set; }
    }
} 