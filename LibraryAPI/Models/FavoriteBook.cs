using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models
{
    public class FavoriteBook
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
} 