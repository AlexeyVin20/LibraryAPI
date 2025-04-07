using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JournalsController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public JournalsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/Journals
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JournalDto>>> GetJournals()
        {
            return await _context.Journals
                .Select(j => new JournalDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    ISSN = j.ISSN,
                    RegistrationNumber = j.RegistrationNumber,
                    Format = j.Format.ToString(),
                    Periodicity = j.Periodicity.ToString(),
                    PagesPerIssue = j.PagesPerIssue,
                    Description = j.Description,
                    Publisher = j.Publisher,
                    FoundationDate = j.FoundationDate,
                    Circulation = j.Circulation,
                    IsOpenAccess = j.IsOpenAccess,
                    Category = j.Category.ToString(),
                    TargetAudience = j.TargetAudience,
                    IsPeerReviewed = j.IsPeerReviewed,
                    IsIndexedInRINTS = j.IsIndexedInRINTS,
                    IsIndexedInScopus = j.IsIndexedInScopus,
                    IsIndexedInWebOfScience = j.IsIndexedInWebOfScience,
                    PublicationDate = j.PublicationDate,
                    PageCount = j.PageCount,
                    Cover = j.Cover,
                    ShelfId = j.ShelfId ?? 0,
                    Position = j.Position ?? 0
                })
                .ToListAsync();
        }

        // GET: api/Journals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JournalDto>> GetJournal(int id)
        {
            var journal = await _context.Journals.FindAsync(id);
            if (journal == null)
            {
                return NotFound();
            }

            return new JournalDto
            {
                Id = journal.Id,
                Title = journal.Title,
                ISSN = journal.ISSN,
                RegistrationNumber = journal.RegistrationNumber,
                Format = journal.Format.ToString(),
                Periodicity = journal.Periodicity.ToString(),
                PagesPerIssue = journal.PagesPerIssue,
                Description = journal.Description,
                Publisher = journal.Publisher,
                FoundationDate = journal.FoundationDate,
                Circulation = journal.Circulation,
                IsOpenAccess = journal.IsOpenAccess,
                Category = journal.Category.ToString(),
                TargetAudience = journal.TargetAudience,
                IsPeerReviewed = journal.IsPeerReviewed,
                IsIndexedInRINTS = journal.IsIndexedInRINTS,
                IsIndexedInScopus = journal.IsIndexedInScopus,
                IsIndexedInWebOfScience = journal.IsIndexedInWebOfScience,
                PublicationDate = journal.PublicationDate,
                PageCount = journal.PageCount,
                Cover = journal.Cover,
                ShelfId = journal.ShelfId ?? 0,
                Position = journal.Position ?? 0
            };
        }

        // POST: api/Journals
        [HttpPost]
        public async Task<ActionResult<JournalDto>> CreateJournal(JournalCreateDto journalDto)
        {
            var journal = new Journal
            {
                Title = journalDto.Title,
                ISSN = journalDto.ISSN,
                RegistrationNumber = journalDto.RegistrationNumber,
                Format = Enum.Parse<JournalFormat>(journalDto.Format ?? throw new ArgumentNullException(nameof(journalDto.Format))),
                Periodicity = Enum.Parse<JournalPeriodicity>(journalDto.Periodicity ?? throw new ArgumentNullException(nameof(journalDto.Periodicity))),
                PagesPerIssue = journalDto.PagesPerIssue ?? 0,
                Description = journalDto.Description ?? string.Empty,
                Publisher = journalDto.Publisher ?? string.Empty,
                FoundationDate = journalDto.FoundationDate ?? DateTime.MinValue,
                Circulation = journalDto.Circulation ?? 0,
                IsOpenAccess = journalDto.IsOpenAccess,
                Category = Enum.Parse<JournalCategory>(journalDto.Category ?? throw new ArgumentNullException(nameof(journalDto.Category))),
                TargetAudience = journalDto.TargetAudience ?? string.Empty,
                IsPeerReviewed = journalDto.IsPeerReviewed ?? false,
                IsIndexedInRINTS = journalDto.IsIndexedInRINTS ?? false,
                IsIndexedInScopus = journalDto.IsIndexedInScopus ?? false,
                IsIndexedInWebOfScience = journalDto.IsIndexedInWebOfScience ?? false,
                PublicationDate = journalDto.PublicationDate ?? DateTime.MinValue,
                PageCount = journalDto.PageCount ?? 0,
                Cover = journalDto.Cover ?? string.Empty
            };

            _context.Journals.Add(journal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetJournal),
                new { id = journal.Id },
                new JournalDto
                {
                    Id = journal.Id,
                    Title = journal.Title,
                    ISSN = journal.ISSN,
                    RegistrationNumber = journal.RegistrationNumber,
                    Format = journal.Format.ToString(),
                    Periodicity = journal.Periodicity.ToString(),
                    PagesPerIssue = journal.PagesPerIssue,
                    Description = journal.Description,
                    Publisher = journal.Publisher,
                    FoundationDate = journal.FoundationDate,
                    Circulation = journal.Circulation,
                    IsOpenAccess = journal.IsOpenAccess,
                    Category = journal.Category.ToString(),
                    TargetAudience = journal.TargetAudience,
                    IsPeerReviewed = journal.IsPeerReviewed,
                    IsIndexedInRINTS = journal.IsIndexedInRINTS,
                    IsIndexedInScopus = journal.IsIndexedInScopus,
                    IsIndexedInWebOfScience = journal.IsIndexedInWebOfScience,
                    PublicationDate = journal.PublicationDate,
                    PageCount = journal.PageCount,
                    Cover = journal.Cover,
                    ShelfId = journal.ShelfId ?? 0,
                    Position = journal.Position ?? 0
                });
        }

        // PUT: api/Journals/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJournal(int id, JournalUpdateDto journalDto)
        {
            var journal = await _context.Journals.FindAsync(id);
            if (journal == null)
            {
                return NotFound();
            }

            journal.Title = journalDto.Title;
            journal.ISSN = journalDto.ISSN;
            journal.RegistrationNumber = journalDto.RegistrationNumber;
            journal.Format = Enum.Parse<JournalFormat>(journalDto.Format ?? throw new ArgumentNullException(nameof(journalDto.Format)));
            journal.Periodicity = Enum.Parse<JournalPeriodicity>(journalDto.Periodicity ?? throw new ArgumentNullException(nameof(journalDto.Periodicity)));
            journal.PagesPerIssue = journalDto.PagesPerIssue ?? 0;
            journal.Description = journalDto.Description ?? string.Empty;
            journal.Publisher = journalDto.Publisher ?? string.Empty;
            journal.FoundationDate = journalDto.FoundationDate ?? DateTime.MinValue;
            journal.Circulation = journalDto.Circulation ?? 0;
            journal.IsOpenAccess = journalDto.IsOpenAccess;
            journal.Category = Enum.Parse<JournalCategory>(journalDto.Category ?? throw new ArgumentNullException(nameof(journalDto.Category)));
            journal.TargetAudience = journalDto.TargetAudience ?? string.Empty;
            journal.IsPeerReviewed = journalDto.IsPeerReviewed ?? false;
            journal.IsIndexedInRINTS = journalDto.IsIndexedInRINTS ?? false;
            journal.IsIndexedInScopus = journalDto.IsIndexedInScopus ?? false;
            journal.IsIndexedInWebOfScience = journalDto.IsIndexedInWebOfScience ?? false;
            journal.PublicationDate = journalDto.PublicationDate ?? DateTime.MinValue;
            journal.PageCount = journalDto.PageCount ?? 0;
            journal.Cover = journalDto.Cover ?? string.Empty;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JournalExists(id))
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

        // DELETE: api/Journals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJournal(int id)
        {
            var journal = await _context.Journals.FindAsync(id);
            if (journal == null)
            {
                return NotFound();
            }

            _context.Journals.Remove(journal);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Journals/search?query=science
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<JournalDto>>> SearchJournals(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return await GetJournals();
            }

            return await _context.Journals
                .Where(j => j.Title.Contains(query) ||
                          j.Description.Contains(query) ||
                          j.Publisher.Contains(query))
                .Select(j => new JournalDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    ISSN = j.ISSN,
                    RegistrationNumber = j.RegistrationNumber,
                    Format = j.Format.ToString(),
                    Periodicity = j.Periodicity.ToString(),
                    PagesPerIssue = j.PagesPerIssue,
                    Description = j.Description,
                    Publisher = j.Publisher,
                    FoundationDate = j.FoundationDate,
                    Circulation = j.Circulation,
                    IsOpenAccess = j.IsOpenAccess,
                    Category = j.Category.ToString(),
                    TargetAudience = j.TargetAudience,
                    IsPeerReviewed = j.IsPeerReviewed,
                    IsIndexedInRINTS = j.IsIndexedInRINTS,
                    IsIndexedInScopus = j.IsIndexedInScopus,
                    IsIndexedInWebOfScience = j.IsIndexedInWebOfScience,
                    PublicationDate = j.PublicationDate,
                    PageCount = j.PageCount,
                    Cover = j.Cover,
                    ShelfId = j.ShelfId ?? 0,
                    Position = j.Position ?? 0
                })
                .ToListAsync();
        }

        // GET: api/Journals/category/Scientific
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<JournalDto>>> GetJournalsByCategory(JournalCategory category)
        {
            return await _context.Journals
                .Where(j => j.Category == category)
                .Select(j => new JournalDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    ISSN = j.ISSN,
                    RegistrationNumber = j.RegistrationNumber,
                    Format = j.Format.ToString(),
                    Periodicity = j.Periodicity.ToString(),
                    PagesPerIssue = j.PagesPerIssue,
                    Description = j.Description,
                    Publisher = j.Publisher,
                    FoundationDate = j.FoundationDate,
                    Circulation = j.Circulation,
                    IsOpenAccess = j.IsOpenAccess,
                    Category = j.Category.ToString(),
                    TargetAudience = j.TargetAudience,
                    IsPeerReviewed = j.IsPeerReviewed,
                    IsIndexedInRINTS = j.IsIndexedInRINTS,
                    IsIndexedInScopus = j.IsIndexedInScopus,
                    IsIndexedInWebOfScience = j.IsIndexedInWebOfScience,
                    PublicationDate = j.PublicationDate,
                    PageCount = j.PageCount,
                    Cover = j.Cover,
                    ShelfId = j.ShelfId ?? 0,
                    Position = j.Position ?? 0
                })
                .ToListAsync();
        }

        // Модель для обновления позиции журнала
        public class JournalPositionDto
        {
            public int ShelfId { get; set; }
            public int Position { get; set; }
        }

        [HttpPut("{id}/position")]
        public async Task<ActionResult<JournalDto>> UpdateJournalPosition(int id, [FromBody] JournalPositionDto positionDto)
        {
            var journal = await _context.Journals.FindAsync(id);
            if (journal == null)
                return NotFound();

            // Проверяем, что полка существует
            var shelf = await _context.Shelves.FindAsync(positionDto.ShelfId);
            if (shelf == null)
                return NotFound(new { Message = "Полка не найдена" });

            // Проверяем, что позиция в пределах емкости полки
            if (positionDto.Position >= shelf.Capacity)
                return BadRequest(new { Message = "Позиция превышает емкость полки" });

            // Проверяем, что позиция не занята другим журналом
            var existingJournal = await _context.Journals
                .FirstOrDefaultAsync(j => j.ShelfId == positionDto.ShelfId && j.Position == positionDto.Position);

            if (existingJournal != null && existingJournal.Id != id)
                return BadRequest(new { Message = "Эта позиция уже занята" });

            journal.ShelfId = positionDto.ShelfId;
            journal.Position = positionDto.Position;
            await _context.SaveChangesAsync();

            return Ok(new JournalDto
            {
                Id = journal.Id,
                Title = journal.Title,
                ISSN = journal.ISSN,
                RegistrationNumber = journal.RegistrationNumber,
                Format = journal.Format.ToString(),
                Periodicity = journal.Periodicity.ToString(),
                PagesPerIssue = journal.PagesPerIssue,
                Description = journal.Description,
                Publisher = journal.Publisher,
                FoundationDate = journal.FoundationDate,
                Circulation = journal.Circulation,
                IsOpenAccess = journal.IsOpenAccess,
                Category = journal.Category.ToString(),
                TargetAudience = journal.TargetAudience,
                IsPeerReviewed = journal.IsPeerReviewed,
                IsIndexedInRINTS = journal.IsIndexedInRINTS,
                IsIndexedInScopus = journal.IsIndexedInScopus,
                IsIndexedInWebOfScience = journal.IsIndexedInWebOfScience,
                PublicationDate = journal.PublicationDate,
                PageCount = journal.PageCount,
                Cover = journal.Cover,
                ShelfId = journal.ShelfId ?? 0,
                Position = journal.Position ?? 0
            });
        }

        private bool JournalExists(int id)
        {
            return _context.Journals.Any(e => e.Id == id);
        }
    }
}
