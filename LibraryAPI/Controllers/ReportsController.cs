using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Data;
using LibraryAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Администратор,Библиотекарь")]
    public class ReportsController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public ReportsController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ReportDto>> GenerateReport([FromBody] ReportRequestDto request)
        {
            if (string.IsNullOrEmpty(request.ReportType))
            {
                return BadRequest("Не указан тип отчета.");
            }

            var report = new ReportDto
            {
                Title = $"Отчет от {DateTime.UtcNow:dd.MM.yyyy HH:mm:ss} UTC",
                Headers = new List<string>(),
                Data = new List<Dictionary<string, object>>()
            };

            switch (request.ReportType.ToLower())
            {
                case "general":
                    if (request.DateFrom == default || request.DateTo == default)
                    {
                        return BadRequest("Для общего отчета необходимо указать период (DateFrom, DateTo).");
                    }
                    report.Title = $"Общий сводный отчет за период c {request.DateFrom:dd.MM.yyyy} по {request.DateTo:dd.MM.yyyy}";

                    var bookInstanceIds = _context.BookInstances
                        .Where(bi => bi.DateAcquired >= request.DateFrom && bi.DateAcquired <= request.DateTo)
                        .Select(bi => bi.BookId);

                    var booksQuery = _context.Books
                        .Where(b => bookInstanceIds.Contains(b.Id));
                    var usersQuery = _context.Users
                        .Where(u => u.DateRegistered >= request.DateFrom && u.DateRegistered <= request.DateTo);
                    var reservationsQuery = _context.Reservations
                        .Where(r => r.ReservationDate >= request.DateFrom && r.ReservationDate <= request.DateTo);
                    var instancesQuery = _context.BookInstances
                        .Where(i => i.DateAcquired >= request.DateFrom && i.DateAcquired <= request.DateTo);

                    var reportData = new Dictionary<string, object>
                    {
                        { "Новые книги (на основе экземпляров)", await booksQuery.CountAsync() },
                        { "Новые пользователи", await usersQuery.CountAsync() },
                        { "Всего резервирований за период", await reservationsQuery.CountAsync() },
                        { "Новые экземпляры книг", await instancesQuery.CountAsync() }
                    };

                    report.Headers = new List<string> { "Секция", "Количество" };
                    foreach (var kvp in reportData)
                    {
                        report.Data.Add(new Dictionary<string, object>
                        {
                            { "Секция", kvp.Key },
                            { "Количество", kvp.Value }
                        });
                    }
                    break;

                case "useractivity":
                    if (!request.Parameters.ContainsKey("UserId") || !Guid.TryParse(request.Parameters["UserId"].ToString(), out var userId))
                    {
                        return BadRequest("Неверный или отсутствующий параметр UserId.");
                    }
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null) return NotFound("Пользователь не найден.");

                    report.Title = $"Отчет по активности пользователя: {user.FullName}";
                    report.Headers = new List<string> { "Книга", "Статус", "Дата резервирования", "Дата истечения" };
                    var userReservations = await _context.Reservations
                        .Where(r => r.UserId == userId)
                        .Include(r => r.Book)
                        .OrderByDescending(r => r.ReservationDate)
                        .ToListAsync();

                    foreach (var res in userReservations)
                    {
                        report.Data.Add(new Dictionary<string, object>
                        {
                            { "Книга", res.Book.Title },
                            { "Статус", res.Status },
                            { "Дата резервирования", res.ReservationDate.ToString("dd.MM.yyyy") },
                            { "Дата истечения", res.ExpirationDate.ToString("dd.MM.yyyy") }
                        });
                    }
                    break;
                    
                case "bookcirculation":
                    if (!request.Parameters.ContainsKey("BookId") || !Guid.TryParse(request.Parameters["BookId"].ToString(), out var bookId))
                    {
                        return BadRequest("Неверный или отсутствующий параметр BookId.");
                    }
                    var book = await _context.Books.FindAsync(bookId);
                    if (book == null) return NotFound("Книга не найдена.");

                    report.Title = $"Отчет по обороту книги: {book.Title}";
                    report.Headers = new List<string> { "Пользователь", "Статус", "Дата резервирования", "Дата истечения" };
                    var bookReservations = await _context.Reservations
                        .Where(r => r.BookId == bookId)
                        .Include(r => r.User)
                        .OrderByDescending(r => r.ReservationDate)
                        .ToListAsync();

                    foreach (var res in bookReservations)
                    {
                        report.Data.Add(new Dictionary<string, object>
                        {
                            { "Пользователь", res.User?.FullName ?? "N/A" },
                            { "Статус", res.Status },
                            { "Дата резервирования", res.ReservationDate.ToString("dd.MM.yyyy") },
                            { "Дата истечения", res.ExpirationDate.ToString("dd.MM.yyyy") }
                        });
                    }
                    break;

                case "reservationssummary":
                     if (request.DateFrom == default || request.DateTo == default)
                    {
                        return BadRequest("Для отчета по резервированиям необходимо указать период (DateFrom, DateTo).");
                    }
                    report.Title = $"Сводка по резервированиям и экземплярам за период с {request.DateFrom:dd.MM.yyyy} по {request.DateTo:dd.MM.yyyy}";
                    report.Headers = new List<string> { "Книга", "Пользователь", "Статус резервирования", "Дата резервирования" };

                    var reservationsInPeriod = await _context.Reservations
                        .Where(r => r.ReservationDate >= request.DateFrom && r.ReservationDate <= request.DateTo)
                        .Include(r => r.Book)
                        .Include(r => r.User)
                        .OrderByDescending(r => r.ReservationDate)
                        .ToListAsync();

                    foreach (var res in reservationsInPeriod)
                    {
                        var row = new Dictionary<string, object>
                        {
                            { "Книга", res.Book?.Title ?? "N/A" },
                            { "Пользователь", res.User?.FullName ?? "N/A" },
                            { "Статус резервирования", res.Status },
                            { "Дата резервирования", res.ReservationDate.ToString("dd.MM.yyyy") }
                        };
                        report.Data.Add(row);
                    }
                    break;
                default:
                    return BadRequest("Неизвестный тип отчета.");
            }
            
            return Ok(report);
        }

        [HttpGet("monthly-summary")]
        public async Task<ActionResult<ReportDto>> GetMonthlySummary()
        {
            var request = new ReportRequestDto
            {
                ReportType = "ReservationsSummary",
                DateFrom = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                DateTo = DateTime.UtcNow
            };
            return await GenerateReport(request);
        }

        [HttpGet("user-activity/{userId}")]
        public async Task<ActionResult<ReportDto>> GetUserActivity(Guid userId)
        {
            var request = new ReportRequestDto
            {
                ReportType = "UserActivity",
                Parameters = new Dictionary<string, object> { { "UserId", userId } }
            };
            return await GenerateReport(request);
        }

        [HttpGet("book-circulation/{bookId}")]
        public async Task<ActionResult<ReportDto>> GetBookCirculation(Guid bookId)
        {
            var request = new ReportRequestDto
            {
                ReportType = "BookCirculation",
                Parameters = new Dictionary<string, object> { { "BookId", bookId } }
            };
            return await GenerateReport(request);
        }
    }
} 