using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class JournalPositionDto
    {
        [Required]
        public int ShelfId { get; set; }

        [Required]
        public int Position { get; set; }
    }
}
