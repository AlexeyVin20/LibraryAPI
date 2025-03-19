using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.DTOs
{
    public class BookPositionDto
    {
        [Required]
        public int ShelfId { get; set; }

        [Required]
        public int Position { get; set; }
    }
}
