using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.DTOs
{
    public class SendEmailRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "General";

        public Dictionary<string, object> TemplateData { get; set; } = new Dictionary<string, object>();
    }
}
