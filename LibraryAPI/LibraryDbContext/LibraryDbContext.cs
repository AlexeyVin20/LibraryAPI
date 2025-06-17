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
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<FavoriteBook> FavoriteBooks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BorrowedBook> BorrowedBooks { get; set; }
        public DbSet<FineRecord> FineRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Shelf>().ToTable("Shelves");
            modelBuilder.Entity<Book>().ToTable("Books");
            
            // Конфигурация для UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
                
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Конфигурация для FavoriteBook
            modelBuilder.Entity<FavoriteBook>()
                .HasKey(fb => new { fb.UserId, fb.BookId });
                
            modelBuilder.Entity<FavoriteBook>()
                .HasOne(fb => fb.User)
                .WithMany(u => u.FavoriteBooks)
                .HasForeignKey(fb => fb.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<FavoriteBook>()
                .HasOne(fb => fb.Book)
                .WithMany()
                .HasForeignKey(fb => fb.BookId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Конфигурация для Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Book)
                .WithMany()
                .HasForeignKey(n => n.BookId)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.BorrowedBook)
                .WithMany()
                .HasForeignKey(n => n.BorrowedBookId)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.CreatedAt })
                .HasDatabaseName("IX_Notifications_UserId_CreatedAt");
                
            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.IsRead)
                .HasDatabaseName("IX_Notifications_IsRead");
            
            // Конфигурация для BorrowedBook
            modelBuilder.Entity<BorrowedBook>()
                .HasOne(bb => bb.User)
                .WithMany(u => u.BorrowedBooks)
                .HasForeignKey(bb => bb.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<BorrowedBook>()
                .HasOne(bb => bb.Book)
                .WithMany()
                .HasForeignKey(bb => bb.BookId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<BorrowedBook>()
                .HasIndex(bb => new { bb.UserId, bb.BorrowDate })
                .HasDatabaseName("IX_BorrowedBooks_UserId_BorrowDate");
                
            modelBuilder.Entity<BorrowedBook>()
                .HasIndex(bb => bb.DueDate)
                .HasDatabaseName("IX_BorrowedBooks_DueDate");
                
            modelBuilder.Entity<BorrowedBook>()
                .HasIndex(bb => bb.ReturnDate)
                .HasDatabaseName("IX_BorrowedBooks_ReturnDate");
            
            // Конфигурация для FineRecord
            modelBuilder.Entity<FineRecord>()
                .HasOne(fr => fr.User)
                .WithMany()
                .HasForeignKey(fr => fr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<FineRecord>()
                .HasOne(fr => fr.Reservation)
                .WithMany()
                .HasForeignKey(fr => fr.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<FineRecord>()
                .HasIndex(fr => new { fr.UserId, fr.CreatedAt })
                .HasDatabaseName("IX_FineRecords_UserId_CreatedAt");
                
            modelBuilder.Entity<FineRecord>()
                .HasIndex(fr => fr.IsPaid)
                .HasDatabaseName("IX_FineRecords_IsPaid");
                
            modelBuilder.Entity<FineRecord>()
                .HasIndex(fr => new { fr.ReservationId, fr.CalculatedForDate, fr.FineType })
                .HasDatabaseName("IX_FineRecords_Reservation_Date_Type");
        }
    }
}