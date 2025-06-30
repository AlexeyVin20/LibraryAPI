using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Services
{
    public interface IBookInstanceAllocationService
    {
        Task<BookInstance?> AllocateBestAvailableInstance(Guid bookId);
        Task<bool> ReleaseBookInstance(Guid? bookInstanceId);
        Task<BookInstance?> GetAllocatedInstance(Guid reservationId);
        Task<bool> ReserveBookInstance(Guid? bookInstanceId);
        Task<bool> UpdateBookInstanceStatus(Guid? bookInstanceId, string status);
        Task<BookInstance?> FindBestAvailableInstance(Guid bookId);
        Task RecalculateBookAvailableCopies(Guid bookId);
    }

    public class BookInstanceAllocationService : IBookInstanceAllocationService
    {
        private readonly LibraryDbContext _context;

        public BookInstanceAllocationService(LibraryDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Находит и выделяет лучший доступный экземпляр книги
        /// Приоритет: состояние (Новое > Хорошее > Удовлетворительное > Плохое), затем по порядковому номеру
        /// </summary>
        public async Task<BookInstance?> AllocateBestAvailableInstance(Guid bookId)
        {
            // Сначала получаем подходящие экземпляры из БД
            var candidates = await _context.BookInstances
                .Where(bi => bi.BookId == bookId &&
                           bi.Status == "Доступна" &&
                           bi.IsActive)
                .ToListAsync();

            // Сортируем в памяти по приоритету состояния, затем по номеру экземпляра
            var bestInstance = candidates
                .OrderBy(bi => GetConditionPriority(bi.Condition))
                .ThenBy(bi => ExtractInstanceNumber(bi.InstanceCode))
                .FirstOrDefault();

            if (bestInstance != null)
            {
                // Помечаем экземпляр как "Выдана" (выдан)
                bestInstance.Status = "Выдана";
                bestInstance.DateModified = DateTime.UtcNow;
                
                // Обновляем количество доступных копий книги
                await RecalculateBookAvailableCopies(bookId);
                
                await _context.SaveChangesAsync();
            }

            return bestInstance;
        }

        /// <summary>
        /// Освобождает экземпляр книги (возврат)
        /// </summary>
        public async Task<bool> ReleaseBookInstance(Guid? bookInstanceId)
        {
            if (!bookInstanceId.HasValue)
                return false;

            var instance = await _context.BookInstances.FindAsync(bookInstanceId.Value);
            if (instance == null)
                return false;

            // Помечаем экземпляр как доступный
            instance.Status = "Доступна";
            instance.DateLastChecked = DateTime.UtcNow;
            instance.DateModified = DateTime.UtcNow;

            // Обновляем количество доступных копий книги
            await RecalculateBookAvailableCopies(instance.BookId);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Получает экземпляр книги, назначенный резервации
        /// </summary>
        public async Task<BookInstance?> GetAllocatedInstance(Guid reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.BookInstance)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            return reservation?.BookInstance;
        }

        /// <summary>
        /// Резервирует экземпляр книги (помечает как зарезервированный)
        /// </summary>
        public async Task<bool> ReserveBookInstance(Guid? bookInstanceId)
        {
            if (!bookInstanceId.HasValue)
                return false;

            var instance = await _context.BookInstances.FindAsync(bookInstanceId.Value);
            if (instance == null || instance.Status != "Доступна")
                return false;

            instance.Status = "Зарезервирована";
            instance.DateModified = DateTime.UtcNow;

            // Обновляем количество доступных копий книги
            await RecalculateBookAvailableCopies(instance.BookId);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Обновляет статус экземпляра книги
        /// </summary>
        public async Task<bool> UpdateBookInstanceStatus(Guid? bookInstanceId, string status)
        {
            if (!bookInstanceId.HasValue)
                return false;

            var instance = await _context.BookInstances.FindAsync(bookInstanceId.Value);
            if (instance == null)
                return false;

            instance.Status = status;
            instance.DateModified = DateTime.UtcNow;

            if (status == "Доступна")
            {
                instance.DateLastChecked = DateTime.UtcNow;
            }

            // Обновляем количество доступных копий книги
            await RecalculateBookAvailableCopies(instance.BookId);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Находит лучший доступный экземпляр книги без выделения
        /// </summary>
        public async Task<BookInstance?> FindBestAvailableInstance(Guid bookId)
        {
            var candidates = await _context.BookInstances
                .Where(bi => bi.BookId == bookId &&
                           bi.Status == "Доступна" &&
                           bi.IsActive)
                .ToListAsync();

            return candidates
                .OrderBy(bi => GetConditionPriority(bi.Condition))
                .ThenBy(bi => ExtractInstanceNumber(bi.InstanceCode))
                .FirstOrDefault();
        }

        private int GetConditionPriority(string condition)
        {
            return condition?.ToLower() switch
            {
                "новое" => 1,
                "хорошее" => 2,
                "удовлетворительное" => 3,
                "плохое" => 4,
                "new" => 1,
                "good" => 2,
                "fair" => 3,
                "poor" => 4,
                _ => 5 // неизвестное состояние - самый низкий приоритет
            };
        }

        private int ExtractInstanceNumber(string instanceCode)
        {
            if (string.IsNullOrEmpty(instanceCode))
                return int.MaxValue;

            var parts = instanceCode.Split('#');
            if (parts.Length == 2 && int.TryParse(parts[1], out int number))
            {
                return number;
            }

            return int.MaxValue; // если не удалось распарсить номер
        }

        public async Task RecalculateBookAvailableCopies(Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book != null)
            {
                var availableCount = await _context.BookInstances
                    .Where(bi => bi.BookId == bookId && bi.Status == "Доступна" && bi.IsActive)
                    .CountAsync();

                book.AvailableCopies = availableCount;
                _context.Books.Update(book);
            }
        }
    }
} 