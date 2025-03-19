using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.DTOs
{
    public class ShelfDto
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public int Capacity { get; set; }
        public int ShelfNumber { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public DateTime? LastReorganized { get; set; }
    }

    public class ShelfCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public int ShelfNumber { get; set; }

        [Required]
        public float PosX { get; set; }

        [Required]
        public float PosY { get; set; }

        public DateTime? LastReorganized { get; set; }
    }

    public class ShelfUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public int ShelfNumber { get; set; }

        [Required]
        public float PosX { get; set; }

        [Required]
        public float PosY { get; set; }

        public DateTime? LastReorganized { get; set; }
    }
}
