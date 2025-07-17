
using System;

namespace LibraryAPI.Models.DTOs
{
    public class BulkInstanceStatusUpdateDto
    {
        public Guid Id { get; set; }
        public string NewStatus { get; set; }
    }
} 