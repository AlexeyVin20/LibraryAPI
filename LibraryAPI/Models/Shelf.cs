using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
    public class Shelf
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        public int Capacity { get; set; }
        public int ShelfNumber { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }

        [JsonIgnore]
        public List<Book> Books { get; set; } = new List<Book>();
        public DateTime? LastReorganized { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
