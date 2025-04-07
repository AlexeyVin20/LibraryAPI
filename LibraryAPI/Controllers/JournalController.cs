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
                    Description = j.Description,
                    Publisher = j.Publisher,
                    FoundationDate = j.FoundationDate,
                    IsOpenAccess = j.IsOpenAccess,
                    Category = j.Category.ToString(),
                    TargetAudience = j.TargetAudience,
                    IsPeerReviewed = j.IsPeerReviewed,
                    IsIndexedInRINTS = j.IsIndexedInRINTS,
                    IsIndexedInScopus = j.IsIndexedInScopus,
                    IsIndexedInWebOfScience = j.IsIndexedInWebOfScience,
                    Website = j.Website,
                    EditorInChief = j.EditorInChief,
                    EditorialBoard = j.EditorialBoard
                })
                .ToListAsync();
        }

        // GET: api/Journals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JournalDto>> GetJournal(int id)
        {
            var journal = await _context.Journals
                .Include(j => j.Issues)
                .FirstOrDefaultAsync(j => j.Id == id);

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
                Description = journal.Description,
                Publisher = journal.Publisher,
                FoundationDate = journal.FoundationDate,
                IsOpenAccess = journal.IsOpenAccess,
                Category = journal.Category.ToString(),
                TargetAudience = journal.TargetAudience,
                IsPeerReviewed = journal.IsPeerReviewed,
                IsIndexedInRINTS = journal.IsIndexedInRINTS,
                IsIndexedInScopus = journal.IsIndexedInScopus,
                IsIndexedInWebOfScience = journal.IsIndexedInWebOfScience,
                Website = journal.Website,
                EditorInChief = journal.EditorInChief,
                EditorialBoard = journal.EditorialBoard,
                Issues = journal.Issues?.Select(i => new IssueShortDto
                {
                    Id = i.Id,
                    VolumeNumber = i.VolumeNumber,
                    IssueNumber = i.IssueNumber,
                    PublicationDate = i.PublicationDate,
                    Cover = i.Cover,
                    SpecialTheme = i.SpecialTheme
                }).ToList()
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
                Format = journalDto.Format,
                Periodicity = journalDto.Periodicity,
                Description = journalDto.Description,
                Publisher = journalDto.Publisher,
                FoundationDate = journalDto.FoundationDate,
                IsOpenAccess = journalDto.IsOpenAccess,
                Category = journalDto.Category,
                TargetAudience = journalDto.TargetAudience,
                IsPeerReviewed = journalDto.IsPeerReviewed,
                IsIndexedInRINTS = journalDto.IsIndexedInRINTS,
                IsIndexedInScopus = journalDto.IsIndexedInScopus,
                IsIndexedInWebOfScience = journalDto.IsIndexedInWebOfScience,
                Website = journalDto.Website,
                EditorInChief = journalDto.EditorInChief,
                EditorialBoard = journalDto.EditorialBoard
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
                    Description = journal.Description,
                    Publisher = journal.Publisher,
                    FoundationDate = journal.FoundationDate,
                    IsOpenAccess = journal.IsOpenAccess,
                    Category = journal.Category.ToString(),
                    TargetAudience = journal.TargetAudience,
                    IsPeerReviewed = journal.IsPeerReviewed,
                    IsIndexedInRINTS = journal.IsIndexedInRINTS,
                    IsIndexedInScopus = journal.IsIndexedInScopus,
                    IsIndexedInWebOfScience = journal.IsIndexedInWebOfScience,
                    Website = journal.Website,
                    EditorInChief = journal.EditorInChief,
                    EditorialBoard = journal.EditorialBoard
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
            journal.Format = journalDto.Format;
            journal.Periodicity = journalDto.Periodicity;
            journal.Description = journalDto.Description;
            journal.Publisher = journalDto.Publisher;
            journal.FoundationDate = journalDto.FoundationDate;
            journal.IsOpenAccess = journalDto.IsOpenAccess;
            journal.Category = journalDto.Category;
            journal.TargetAudience = journalDto.TargetAudience;
            journal.IsPeerReviewed = journalDto.IsPeerReviewed;
            journal.IsIndexedInRINTS = journalDto.IsIndexedInRINTS;
            journal.IsIndexedInScopus = journalDto.IsIndexedInScopus;
            journal.IsIndexedInWebOfScience = journalDto.IsIndexedInWebOfScience;
            journal.Website = journalDto.Website;
            journal.EditorInChief = journalDto.EditorInChief;
            journal.EditorialBoard = journalDto.EditorialBoard;

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
                    Description = j.Description,
                    Publisher = j.Publisher,
                    FoundationDate = j.FoundationDate,
                    IsOpenAccess = j.IsOpenAccess,
                    Category = j.Category.ToString(),
                    TargetAudience = j.TargetAudience,
                    IsPeerReviewed = j.IsPeerReviewed,
                    IsIndexedInRINTS = j.IsIndexedInRINTS,
                    IsIndexedInScopus = j.IsIndexedInScopus,
                    IsIndexedInWebOfScience = j.IsIndexedInWebOfScience,
                    Website = j.Website,
                    EditorInChief = j.EditorInChief,
                    EditorialBoard = j.EditorialBoard
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
                    Description = j.Description,
                    Publisher = j.Publisher,
                    FoundationDate = j.FoundationDate,
                    IsOpenAccess = j.IsOpenAccess,
                    Category = j.Category.ToString(),
                    TargetAudience = j.TargetAudience,
                    IsPeerReviewed = j.IsPeerReviewed,
                    IsIndexedInRINTS = j.IsIndexedInRINTS,
                    IsIndexedInScopus = j.IsIndexedInScopus,
                    IsIndexedInWebOfScience = j.IsIndexedInWebOfScience,
                    Website = j.Website,
                    EditorInChief = j.EditorInChief,
                    EditorialBoard = j.EditorialBoard
                })
                .ToListAsync();
        }

        private bool JournalExists(int id)
        {
            return _context.Journals.Any(e => e.Id == id);
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public IssuesController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/Issues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IssueDto>>> GetIssues()
        {
            return await _context.Issues
                .Include(i => i.Journal)
                .Select(i => new IssueDto
                {
                    Id = i.Id,
                    JournalId = i.JournalId,
                    JournalTitle = i.Journal.Title,
                    VolumeNumber = i.VolumeNumber,
                    IssueNumber = i.IssueNumber,
                    PublicationDate = i.PublicationDate,
                    PageCount = i.PageCount,
                    Cover = i.Cover,
                    Circulation = i.Circulation,
                    SpecialTheme = i.SpecialTheme,
                    ShelfId = i.ShelfId,
                    Position = i.Position
                })
                .ToListAsync();
        }

        // GET: api/Issues/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IssueDto>> GetIssue(int id)
        {
            var issue = await _context.Issues
                .Include(i => i.Journal)
                .Include(i => i.Articles)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (issue == null)
            {
                return NotFound();
            }

            return new IssueDto
            {
                Id = issue.Id,
                JournalId = issue.JournalId,
                JournalTitle = issue.Journal.Title,
                VolumeNumber = issue.VolumeNumber,
                IssueNumber = issue.IssueNumber,
                PublicationDate = issue.PublicationDate,
                PageCount = issue.PageCount,
                Cover = issue.Cover,
                Circulation = issue.Circulation,
                SpecialTheme = issue.SpecialTheme,
                ShelfId = issue.ShelfId,
                Position = issue.Position,
                Articles = issue.Articles?.Select(a => new ArticleShortDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Authors = a.Authors,
                    StartPage = a.StartPage,
                    EndPage = a.EndPage,
                    DOI = a.DOI
                }).ToList()
            };
        }

        // GET: api/Issues/Journal/5
        [HttpGet("Journal/{journalId}")]
        public async Task<ActionResult<IEnumerable<IssueDto>>> GetIssuesByJournal(int journalId)
        {
            var journal = await _context.Journals.FindAsync(journalId);
            if (journal == null)
            {
                return NotFound();
            }

            return await _context.Issues
                .Where(i => i.JournalId == journalId)
                .Include(i => i.Journal)
                .Select(i => new IssueDto
                {
                    Id = i.Id,
                    JournalId = i.JournalId,
                    JournalTitle = i.Journal.Title,
                    VolumeNumber = i.VolumeNumber,
                    IssueNumber = i.IssueNumber,
                    PublicationDate = i.PublicationDate,
                    PageCount = i.PageCount,
                    Cover = i.Cover,
                    Circulation = i.Circulation,
                    SpecialTheme = i.SpecialTheme,
                    ShelfId = i.ShelfId,
                    Position = i.Position
                })
                .ToListAsync();
        }

        // POST: api/Issues
        [HttpPost]
        public async Task<ActionResult<IssueDto>> CreateIssue(IssueCreateDto issueDto)
        {
            var journal = await _context.Journals.FindAsync(issueDto.JournalId);
            if (journal == null)
            {
                return BadRequest(new { Message = "Журнал не найден" });
            }

            var issue = new Issue
            {
                JournalId = issueDto.JournalId,
                VolumeNumber = issueDto.VolumeNumber,
                IssueNumber = issueDto.IssueNumber,
                PublicationDate = issueDto.PublicationDate,
                PageCount = issueDto.PageCount,
                Cover = issueDto.Cover,
                Circulation = issueDto.Circulation,
                SpecialTheme = issueDto.SpecialTheme,
                ShelfId = issueDto.ShelfId,
                Position = issueDto.Position
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetIssue),
                new { id = issue.Id },
                new IssueDto
                {
                    Id = issue.Id,
                    JournalId = issue.JournalId,
                    JournalTitle = journal.Title,
                    VolumeNumber = issue.VolumeNumber,
                    IssueNumber = issue.IssueNumber,
                    PublicationDate = issue.PublicationDate,
                    PageCount = issue.PageCount,
                    Cover = issue.Cover,
                    Circulation = issue.Circulation,
                    SpecialTheme = issue.SpecialTheme,
                    ShelfId = issue.ShelfId,
                    Position = issue.Position
                });
        }

        // PUT: api/Issues/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIssue(int id, IssueUpdateDto issueDto)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
            {
                return NotFound();
            }

            issue.VolumeNumber = issueDto.VolumeNumber;
            issue.IssueNumber = issueDto.IssueNumber;
            issue.PublicationDate = issueDto.PublicationDate;
            issue.PageCount = issueDto.PageCount;
            issue.Cover = issueDto.Cover;
            issue.Circulation = issueDto.Circulation;
            issue.SpecialTheme = issueDto.SpecialTheme;
            issue.ShelfId = issueDto.ShelfId;
            issue.Position = issueDto.Position;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IssueExists(id))
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

        // DELETE: api/Issues/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
            {
                return NotFound();
            }

            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IssueExists(int id)
        {
            return _context.Issues.Any(e => e.Id == id);
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public ArticlesController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/Articles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticleDto>>> GetArticles()
        {
            return await _context.Articles
                .Include(a => a.Issue)
                    .ThenInclude(i => i.Journal)
                .Select(a => new ArticleDto
                {
                    Id = a.Id,
                    IssueId = a.IssueId,
                    Title = a.Title,
                    Authors = a.Authors,
                    Abstract = a.Abstract,
                    StartPage = a.StartPage,
                    EndPage = a.EndPage,
                    Keywords = a.Keywords,
                    DOI = a.DOI,
                    ArticleType = a.Type.ToString(),
                    FullText = a.FullText,
                    Issue = new IssueShortDto
                    {
                        Id = a.Issue.Id,
                        VolumeNumber = a.Issue.VolumeNumber,
                        IssueNumber = a.Issue.IssueNumber,
                        PublicationDate = a.Issue.PublicationDate,
                        Cover = a.Issue.Cover,
                        SpecialTheme = a.Issue.SpecialTheme
                    }
                })
                .ToListAsync();
        }

        // GET: api/Articles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleDto>> GetArticle(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Issue)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            return new ArticleDto
            {
                Id = article.Id,
                IssueId = article.IssueId,
                Title = article.Title,
                Authors = article.Authors,
                Abstract = article.Abstract,
                StartPage = article.StartPage,
                EndPage = article.EndPage,
                Keywords = article.Keywords,
                DOI = article.DOI,
                ArticleType = article.Type.ToString(),
                FullText = article.FullText,
                Issue = new IssueShortDto
                {
                    Id = article.Issue.Id,
                    VolumeNumber = article.Issue.VolumeNumber,
                    IssueNumber = article.Issue.IssueNumber,
                    PublicationDate = article.Issue.PublicationDate,
                    Cover = article.Issue.Cover,
                    SpecialTheme = article.Issue.SpecialTheme
                }
            };
        }

        // GET: api/Articles/Issue/5
        [HttpGet("Issue/{issueId}")]
        public async Task<ActionResult<IEnumerable<ArticleDto>>> GetArticlesByIssue(int issueId)
        {
            var issue = await _context.Issues.FindAsync(issueId);
            if (issue == null)
            {
                return NotFound();
            }

            return await _context.Articles
                .Where(a => a.IssueId == issueId)
                .Include(a => a.Issue)
                .Select(a => new ArticleDto
                {
                    Id = a.Id,
                    IssueId = a.IssueId,
                    Title = a.Title,
                    Authors = a.Authors,
                    Abstract = a.Abstract,
                    StartPage = a.StartPage,
                    EndPage = a.EndPage,
                    Keywords = a.Keywords,
                    DOI = a.DOI,
                    ArticleType = a.Type.ToString(),
                    FullText = a.FullText,
                    Issue = new IssueShortDto
                    {
                        Id = a.Issue.Id,
                        VolumeNumber = a.Issue.VolumeNumber,
                        IssueNumber = a.Issue.IssueNumber,
                        PublicationDate = a.Issue.PublicationDate,
                        Cover = a.Issue.Cover,
                        SpecialTheme = a.Issue.SpecialTheme
                    }
                })
                .ToListAsync();
        }

        // POST: api/Articles
        [HttpPost]
        public async Task<ActionResult<ArticleDto>> CreateArticle(ArticleCreateDto articleDto)
        {
            var issue = await _context.Issues.FindAsync(articleDto.IssueId);
            if (issue == null)
            {
                return BadRequest(new { Message = "Выпуск не найден" });
            }

            var article = new Article
            {
                IssueId = articleDto.IssueId,
                Title = articleDto.Title,
                Authors = articleDto.Authors,
                Abstract = articleDto.Abstract,
                StartPage = articleDto.StartPage,
                EndPage = articleDto.EndPage,
                Keywords = articleDto.Keywords,
                DOI = articleDto.DOI,
                Type = articleDto.Type,
                FullText = articleDto.FullText
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetArticle),
                new { id = article.Id },
                new ArticleDto
                {
                    Id = article.Id,
                    IssueId = article.IssueId,
                    Title = article.Title,
                    Authors = article.Authors,
                    Abstract = article.Abstract,
                    StartPage = article.StartPage,
                    EndPage = article.EndPage,
                    Keywords = article.Keywords,
                    DOI = article.DOI,
                    ArticleType = article.Type.ToString(),
                    FullText = article.FullText,
                    Issue = new IssueShortDto
                    {
                        Id = issue.Id,
                        VolumeNumber = issue.VolumeNumber,
                        IssueNumber = issue.IssueNumber,
                        PublicationDate = issue.PublicationDate,
                        Cover = issue.Cover,
                        SpecialTheme = issue.SpecialTheme
                    }
                });
        }

        // PUT: api/Articles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, ArticleUpdateDto articleDto)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            article.Title = articleDto.Title;
            article.Authors = articleDto.Authors;
            article.Abstract = articleDto.Abstract;
            article.StartPage = articleDto.StartPage;
            article.EndPage = articleDto.EndPage;
            article.Keywords = articleDto.Keywords;
            article.DOI = articleDto.DOI;
            article.Type = articleDto.Type;
            article.FullText = articleDto.FullText;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
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

        // DELETE: api/Articles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Articles/search?query=keyword
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ArticleDto>>> SearchArticles(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return await GetArticles();
            }

            return await _context.Articles
                .Where(a => a.Title.Contains(query) ||
                           a.Abstract.Contains(query) ||
                           a.Authors.Any(author => author.Contains(query)) ||
                           a.Keywords.Any(keyword => keyword.Contains(query)))
                .Include(a => a.Issue)
                .Select(a => new ArticleDto
                {
                    Id = a.Id,
                    IssueId = a.IssueId,
                    Title = a.Title,
                    Authors = a.Authors,
                    Abstract = a.Abstract,
                    StartPage = a.StartPage,
                    EndPage = a.EndPage,
                    Keywords = a.Keywords,
                    DOI = a.DOI,
                    ArticleType = a.Type.ToString(),
                    FullText = a.FullText,
                    Issue = new IssueShortDto
                    {
                        Id = a.Issue.Id,
                        VolumeNumber = a.Issue.VolumeNumber,
                        IssueNumber = a.Issue.IssueNumber,
                        PublicationDate = a.Issue.PublicationDate,
                        Cover = a.Issue.Cover,
                        SpecialTheme = a.Issue.SpecialTheme
                    }
                })
                .ToListAsync();
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}
