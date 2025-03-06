using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: api/shelf
        [HttpGet]
        public async Task<IActionResult> GetShelves()
        {
            var shelves = await _context.Shelves
                .Include(s => s.Books)
                .ToListAsync();
            return Ok(shelves);
        }

        // GET: api/shelf/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShelf(int id)
        {
            var shelf = await _context.Shelves
                .Include(s => s.Books)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (shelf == null)
                return NotFound();

            return Ok(shelf);
        }

        // POST: api/shelf
        [HttpPost]
        public async Task<IActionResult> CreateShelf(Shelf shelf)
        {
            _context.Shelves.Add(shelf);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetShelf), new { id = shelf.Id }, shelf);
        }

        // PUT: api/shelf/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShelf(int id, Shelf shelf)
        {
            if (id != shelf.Id)
                return BadRequest();

            // Find the existing shelf with its books
            var existingShelf = await _context.Shelves
                .Include(s => s.Books)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingShelf == null)
                return NotFound();

            // Update basic properties
            existingShelf.Category = shelf.Category;
            existingShelf.Capacity = shelf.Capacity;
            existingShelf.ShelfNumber = shelf.ShelfNumber;
            existingShelf.PosX = shelf.PosX;
            existingShelf.PosY = shelf.PosY;

            // Don't modify the Books collection
            // This is key - we're not touching the books relationship at all

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

            return NoContent();
        }


        // DELETE: api/shelf/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShelf(int id)
        {
            var shelf = await _context.Shelves.FindAsync(id);
            if (shelf == null)
                return NotFound();

            _context.Shelves.Remove(shelf);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
