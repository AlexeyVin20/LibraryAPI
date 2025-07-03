using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.DTOs
{
    public class TestEmailDto
    {
        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(200, ErrorMessage = "Заголовок не может быть длиннее 200 символов")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Сообщение обязательно")]
        [StringLength(1000, ErrorMessage = "Сообщение не может быть длиннее 1000 символов")]
        public string Message { get; set; } = string.Empty;
    }
} 