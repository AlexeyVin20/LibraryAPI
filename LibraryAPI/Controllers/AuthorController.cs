using LibraryAPI.Data;
using LibraryAPI.Models.DTOs;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public AuthorController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors()
        {
            var authors = await _context.Authors.ToListAsync();
            return Ok(authors.Select(a => new AuthorDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                DateOfBirth = a.DateOfBirth,
                DateOfDeath = a.DateOfDeath,
                Biography = a.Biography,
                Nationality = a.Nationality
            }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorDto>> GetAuthor(Guid id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            return Ok(new AuthorDto
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                DateOfBirth = author.DateOfBirth,
                DateOfDeath = author.DateOfDeath,
                Biography = author.Biography,
                Nationality = author.Nationality
            });
        }

        [HttpPost]
        public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorCreateDto authorDto)
        {
            var author = new Author
            {
                Id = authorDto.Id,
                FirstName = authorDto.FirstName,
                LastName = authorDto.LastName,
                DateOfBirth = authorDto.DateOfBirth,
                DateOfDeath = authorDto.DateOfDeath,
                Biography = authorDto.Biography,
                Nationality = authorDto.Nationality
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, new AuthorDto
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                DateOfBirth = author.DateOfBirth,
                DateOfDeath = author.DateOfDeath,
                Biography = author.Biography,
                Nationality = author.Nationality
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(Guid id, AuthorUpdateDto authorDto)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            author.FirstName = authorDto.FirstName;
            author.LastName = authorDto.LastName;
            author.DateOfBirth = authorDto.DateOfBirth;
            author.DateOfDeath = authorDto.DateOfDeath;
            author.Biography = authorDto.Biography;
            author.Nationality = authorDto.Nationality;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}