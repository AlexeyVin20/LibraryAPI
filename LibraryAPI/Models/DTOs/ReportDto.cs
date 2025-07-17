
using System.Collections.Generic;

namespace LibraryAPI.Models.DTOs
{
    public class ReportDto
    {
        public string Title { get; set; }
        public List<string> Headers { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }
    }
} 