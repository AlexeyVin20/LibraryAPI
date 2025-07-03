using System;

namespace LibraryAPI.Models.DTOs
{
    public class FavoriteBookDto
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthors { get; set; }
        public string BookCover { get; set; }
        public DateTime DateAdded { get; set; }
    }

    public class FavoriteBookCreateDto
    {
        public Guid BookId { get; set; }
    }
    
    public class FavoriteBookUserDto
    {
        public Guid UserId { get; set; }
    }
} 