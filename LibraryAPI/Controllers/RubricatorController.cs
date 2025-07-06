using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Models;
using LibraryAPI.Data;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RubricatorController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public RubricatorController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/Rubricator
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rubricator>>> GetRubricators()
        {
            return await _context.Rubricators.ToListAsync();
        }

        // GET: api/Rubricator/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Rubricator>> GetRubricator(int id)
        {
            var rubricator = await _context.Rubricators.FindAsync(id);

            if (rubricator == null)
            {
                return NotFound();
            }

            return rubricator;
        }

        // POST: api/Rubricator
        [HttpPost]
        public async Task<ActionResult<Rubricator>> PostRubricator(Rubricator rubricator)
        {
            _context.Rubricators.Add(rubricator);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRubricator", new { id = rubricator.Id }, rubricator);
        }

        // PUT: api/Rubricator/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRubricator(int id, Rubricator rubricator)
        {
            if (id != rubricator.Id)
            {
                return BadRequest();
            }

            _context.Entry(rubricator).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RubricatorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Rubricator/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRubricator(int id)
        {
            var rubricator = await _context.Rubricators.FindAsync(id);
            if (rubricator == null)
            {
                return NotFound();
            }

            _context.Rubricators.Remove(rubricator);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RubricatorExists(int id)
        {
            return _context.Rubricators.Any(e => e.Id == id);
        }
    }
} 