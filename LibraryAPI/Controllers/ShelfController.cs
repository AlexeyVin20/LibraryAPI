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
    [Route("api/[controller]")]
    [ApiController]
    public class ShelfController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public ShelfController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShelfDto>>> GetShelves()
        {
            var shelves = await _context.Shelves
                .Include(s => s.Books)
                .ToListAsync();

            var shelfDtos = shelves.Select(shelf => new ShelfDto
            {
                Id = shelf.Id,
                Category = shelf.Category,
                Capacity = shelf.Capacity,
                ShelfNumber = shelf.ShelfNumber,
                PosX = shelf.PosX,
                PosY = shelf.PosY,
                LastReorganized = shelf.LastReorganized
            }).ToList();

            return Ok(shelfDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShelfDto>> GetShelf(int id)
        {
            var shelf = await _context.Shelves
                .Include(s => s.Books)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shelf == null)
                return NotFound();

            var shelfDto = new ShelfDto
            {
                Id = shelf.Id,
                Category = shelf.Category,
                Capacity = shelf.Capacity,
                ShelfNumber = shelf.ShelfNumber,
                PosX = shelf.PosX,
                PosY = shelf.PosY,
                LastReorganized = shelf.LastReorganized
            };

            return Ok(shelfDto);
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<ShelfDto>> CreateShelf(ShelfCreateDto shelfDto)
        {
            if (shelfDto == null)
            {
                return BadRequest(new { message = "Данные полки отсутствуют" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Вызываем метод для автоматического позиционирования полки
            return await CreateShelfWithAutoPosition(shelfDto);
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<ShelfDto>> UpdateShelf(int id, ShelfUpdateDto shelfDto)
        {
            var existingShelf = await _context.Shelves
                .Include(s => s.Books)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingShelf == null)
                return NotFound();

            existingShelf.Category = shelfDto.Category;
            existingShelf.Capacity = shelfDto.Capacity;
            existingShelf.ShelfNumber = shelfDto.ShelfNumber;
            existingShelf.PosX = shelfDto.PosX;
            existingShelf.PosY = shelfDto.PosY;
            existingShelf.DateModified = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Shelves.Any(s => s.Id == id))
                    return NotFound();
                else
                    throw;
            }

            // Вместо NoContent() возвращаем обновленную полку
            var updatedShelfDto = new ShelfDto
            {
                Id = existingShelf.Id,
                Category = existingShelf.Category,
                Capacity = existingShelf.Capacity,
                ShelfNumber = existingShelf.ShelfNumber,
                PosX = existingShelf.PosX,
                PosY = existingShelf.PosY,
                LastReorganized = existingShelf.LastReorganized
            };

            return Ok(updatedShelfDto);
        }

        [HttpDelete("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<object>> DeleteShelf(int id)
        {
            var shelf = await _context.Shelves.FindAsync(id);
            if (shelf == null)
                return NotFound();

            // Сохраняем информацию о полке перед удалением
            var shelfInfo = new
            {
                Id = shelf.Id,
                Category = shelf.Category,
                ShelfNumber = shelf.ShelfNumber,
                DeletedAt = DateTime.UtcNow
            };

            _context.Shelves.Remove(shelf);
            await _context.SaveChangesAsync();

            // Возвращаем информацию об удаленной полке
            return Ok(new
            {
                Message = "Полка успешно удалена",
                DeletedShelf = shelfInfo
            });
        }

        [HttpPost("auto-position")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<ShelfDto>> CreateShelfWithAutoPosition(ShelfCreateDto shelfDto)
        {
            if (shelfDto == null)
            {
                return BadRequest(new { message = "Данные полки отсутствуют" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Получаем все существующие полки
            var existingShelves = await _context.Shelves.ToListAsync();

            // Если полок нет, ставим первую полку в начальную позицию
            if (!existingShelves.Any())
            {
                shelfDto.PosX = 100;
                shelfDto.PosY = 100;
            }
            else
            {
                // Находим оптимальное место для новой полки
                var lastShelf = existingShelves.OrderByDescending(s => s.DateModified).First();

                // Проверяем, достаточно ли места справа
                var shelvesAtSameY = existingShelves.Where(s => Math.Abs(s.PosY - lastShelf.PosY) < 50).ToList();
                var maxRightX = shelvesAtSameY.Any() ? shelvesAtSameY.Max(s => s.PosX) : lastShelf.PosX;

                // Стандартная ширина полки + отступ
                const float shelfWidth = 200;
                const float padding = 50;

                // Если по X достигли предела (например, 1500), то переходим на новый ряд
                if (maxRightX + shelfWidth + padding > 1500)
                {
                    shelfDto.PosX = 100;
                    shelfDto.PosY = existingShelves.Max(s => s.PosY) + 300; // Новый ряд
                }
                else
                {
                    shelfDto.PosX = maxRightX + shelfWidth + padding;
                    shelfDto.PosY = lastShelf.PosY;
                }
            }

            var shelf = new Shelf
            {
                Category = shelfDto.Category,
                Capacity = shelfDto.Capacity,
                ShelfNumber = shelfDto.ShelfNumber,
                PosX = shelfDto.PosX,
                PosY = shelfDto.PosY,
                LastReorganized = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            _context.Shelves.Add(shelf);
            await _context.SaveChangesAsync();

            var createdShelfDto = new ShelfDto
            {
                Id = shelf.Id,
                Category = shelf.Category,
                Capacity = shelf.Capacity,
                ShelfNumber = shelf.ShelfNumber,
                PosX = shelf.PosX,
                PosY = shelf.PosY,
                LastReorganized = shelf.LastReorganized
            };

            return CreatedAtAction(nameof(GetShelf), new { id = shelf.Id }, createdShelfDto);
        }

        // GET: api/Shelf/{id}/books
        [HttpGet("{id}/books")]
        public async Task<ActionResult<IEnumerable<object>>> GetBooksOnShelf(int id)
        {
            var shelf = await _context.Shelves
                .Include(s => s.Books)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shelf == null)
                return NotFound();

            var books = shelf.Books.Select(book => new
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                ISBN = book.ISBN,
                PublicationYear = book.PublicationYear
            });

            return Ok(books);
        }

        // GET: api/Shelf/category/{category}
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<ShelfDto>>> GetShelvesByCategory(string category)
        {
            var shelves = await _context.Shelves
                .Where(s => s.Category.ToLower() == category.ToLower())
                .Include(s => s.Books)
                .ToListAsync();

            if (!shelves.Any())
                return Ok(new List<ShelfDto>());

            var shelfDtos = shelves.Select(shelf => new ShelfDto
            {
                Id = shelf.Id,
                Category = shelf.Category,
                Capacity = shelf.Capacity,
                ShelfNumber = shelf.ShelfNumber,
                PosX = shelf.PosX,
                PosY = shelf.PosY,
                LastReorganized = shelf.LastReorganized
            }).ToList();

            return Ok(shelfDtos);
        }
    }
}
