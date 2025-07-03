using LibraryAPI.Data;
using LibraryAPI.Models;
using LibraryAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteBookController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public FavoriteBookController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriteBookDto>>> GetAllFavorites()
        {
            var favorites = await _context.FavoriteBooks
                .Include(fb => fb.Book)
                .Include(fb => fb.User)
                .ToListAsync();

            return Ok(favorites.Select(fb => new FavoriteBookDto
            {
                UserId = fb.UserId,
                BookId = fb.BookId,
                BookTitle = fb.Book.Title,
                BookAuthors = fb.Book.Authors,
                BookCover = fb.Book.Cover,
                DateAdded = fb.DateAdded
            }));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<FavoriteBookDto>>> GetUserFavorites(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var favorites = await _context.FavoriteBooks
                .Where(fb => fb.UserId == userId)
                .Include(fb => fb.Book)
                .ToListAsync();

            return Ok(favorites.Select(fb => new FavoriteBookDto
            {
                UserId = fb.UserId,
                BookId = fb.BookId,
                BookTitle = fb.Book.Title,
                BookAuthors = fb.Book.Authors,
                BookCover = fb.Book.Cover,
                DateAdded = fb.DateAdded
            }));
        }

        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<int>> GetBookFavoritesCount(Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return NotFound(new { message = "Книга не найдена" });

            var count = await _context.FavoriteBooks
                .CountAsync(fb => fb.BookId == bookId);

            return Ok(new { bookId = bookId, favoritesCount = count });
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteBookDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null)
                return NotFound(new { message = "Книга не найдена" });

            var existingFavorite = await _context.FavoriteBooks
                .FirstOrDefaultAsync(fb => fb.UserId == dto.UserId && fb.BookId == dto.BookId);

            if (existingFavorite != null)
                return BadRequest(new { message = "Эта книга уже в избранном" });

            var favoriteBook = new FavoriteBook
            {
                UserId = dto.UserId,
                BookId = dto.BookId,
                DateAdded = DateTime.UtcNow
            };

            _context.FavoriteBooks.Add(favoriteBook);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Книга добавлена в избранное" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ошибка при добавлении книги в избранное: {ex.Message}" });
            }
        }

        [HttpDelete("{userId}/{bookId}")]
        public async Task<IActionResult> RemoveFavorite(Guid userId, Guid bookId)
        {
            var favoriteBook = await _context.FavoriteBooks
                .FirstOrDefaultAsync(fb => fb.UserId == userId && fb.BookId == bookId);

            if (favoriteBook == null)
                return NotFound(new { message = "Книга не найдена в избранном" });

            _context.FavoriteBooks.Remove(favoriteBook);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Книга удалена из избранного" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ошибка при удалении книги из избранного: {ex.Message}" });
            }
        }
    }
} 