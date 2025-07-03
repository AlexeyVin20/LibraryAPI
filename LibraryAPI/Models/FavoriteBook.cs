using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
    public class FavoriteBook
    {
        public Guid UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        
        public Guid BookId { get; set; }
        [JsonIgnore]
        public Book Book { get; set; }
        
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
} 