using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace LibraryAPI.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Journal> Journals { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Article> Articles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Shelf>().ToTable("Shelves");
            modelBuilder.Entity<Book>().ToTable("Books");
        }
    }
}
