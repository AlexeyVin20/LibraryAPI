namespace LibraryAPI.Models
{
    public class Shelf
    {
        public int Id { get; set; }
        public string Category { get; set; }             // Рубрика полки
        public int Capacity { get; set; }                // Количество мест
        public int ShelfNumber { get; set; }             // Номер полки
        public float PosX { get; set; }                  // Позиция в редакторе (X)
        public float PosY { get; set; }                  // Позиция в редакторе (Y)

        // Навигационное свойство для связанных книг
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
