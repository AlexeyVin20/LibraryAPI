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
        public DbSet<BookInstance> BookInstances { get; set; }
        public DbSet<Rubricator> Rubricators { get; set; }
        // Журнал вызовов инструментов ИИ-ассистента
        public DbSet<DialogHistory> DialogHistories { get; set; }

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
            
            // Конфигурация для BookInstance
            modelBuilder.Entity<BookInstance>()
                .HasOne(bi => bi.Book)
                .WithMany()
                .HasForeignKey(bi => bi.BookId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<BookInstance>()
                .HasOne(bi => bi.Shelf)
                .WithMany()
                .HasForeignKey(bi => bi.ShelfId)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<BookInstance>()
                .HasIndex(bi => bi.InstanceCode)
                .IsUnique()
                .HasDatabaseName("IX_BookInstances_InstanceCode");
                
            modelBuilder.Entity<BookInstance>()
                .HasIndex(bi => new { bi.BookId, bi.Status })
                .HasDatabaseName("IX_BookInstances_BookId_Status");
                
            modelBuilder.Entity<BookInstance>()
                .HasIndex(bi => new { bi.ShelfId, bi.Position })
                .HasDatabaseName("IX_BookInstances_ShelfId_Position");

            // Конфигурация связи Reservation -> BookInstance
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.BookInstance)
                .WithMany()
                .HasForeignKey(r => r.BookInstanceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Заполнение начальными данными
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Создание начальных ролей
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Администратор", Description = "Администратор системы" },
                new Role { Id = 2, Name = "Библиотекарь", Description = "Библиотекарь" },
                new Role { Id = 3, Name = "Сотрудник", Description = "Сотрудники МНТК" },
                new Role { Id = 4, Name = "Гость", Description = "Гости библиотеки" }
            );

            // Создание административного пользователя
            // Пароль: Admin123! (предварительно захешированный)
            var adminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            // Статический хеш для пароля "Admin123!" с солью
            var adminPasswordHash = "$2a$12$qs8l2GwWrvN3h2MDgxrZ.uHsNaaKWvjtJXCDrDNDM6gcOeC05vA8u";

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId,
                    FullName = "Системный Администратор",
                    Email = "admin@library.com",
                    Phone = "+7 (999) 999-99-99",
                    Username = "admin",
                    PasswordHash = adminPasswordHash,
                    DateRegistered = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    MaxBooksAllowed = 100,
                    LoanPeriodDays = 30,
                    BorrowedBooksCount = 0,
                    FineAmount = 0
                }
            );

            // Назначение роли администратора
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = adminUserId, RoleId = 1 }
            );
        }
    }
}