
using System;

namespace LibraryAPI.Models.DTOs
{
    public class BulkCreateInstancesDto
    {
        public Guid BookId { get; set; }
        public int Count { get; set; }
        public string DefaultStatus { get; set; }
        public string DefaultCondition { get; set; }
    }
} 