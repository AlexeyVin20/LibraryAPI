using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DialogHistoryController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly ILogger<DialogHistoryController> _logger;

        public DialogHistoryController(LibraryDbContext context, ILogger<DialogHistoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Сохранить запись истории диалога (вызывается фронтом).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DialogHistory dto)
        {
            if (dto == null)
            {
                return BadRequest("Тело запроса пустое");
            }

            try
            {
                dto.Timestamp = DateTime.UtcNow;
                _context.DialogHistories.Add(dto);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, dto.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении истории диалога");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера." });
            }
        }

        /// <summary>
        /// Получить историю по conversationId. Если ID не указан, вернется вся история.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? conversationId = null)
        {
            var query = _context.DialogHistories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(conversationId))
            {
                query = query.Where(d => d.ConversationId == conversationId);
            }

            var result = await query.OrderBy(d => d.Timestamp).ToListAsync();
            return Ok(result);
        }
    }
}