using MessengerBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using MessengerBackend.Data;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessengerBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AIAnalysysController : ControllerBase
    {
        private readonly MessengerDbContext _context;

        public AIAnalysysController(MessengerDbContext context)
        {
            _context = context;
        }

        // Method to get all unseen AI analysis for the current user
        [HttpGet("unseen")]
        public async Task<ActionResult<IEnumerable<AIAnalysys>>> GetUnseenAIAnalysis()
        {
            // Get the current user's Id.
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Get all unseen AI analysis where the current user is part of the chat
            var unseenAnalysis = await _context.AIAnalysys
                  .Where(a => !_context.AIAnalysysSeen.Any(s => s.AnalysysId == a.Id && s.UserId == currentUserId) &&
                              a.Chat.ChatParticipants.Any(x => x.UserId == currentUserId) &&
                              a.Message.isSuspended)
                  .Select(a => new
                  {
                      a.Id,
                      a.User.UserName,
                      a.Reason,
                      MessageContent = a.Message.Text,
                  })
                  .ToListAsync();

            return Ok(unseenAnalysis);
        }

        // Method to mark an AI analysis as seen
        [HttpPut("seen/{id}")]
        public async Task<IActionResult> MarkAsSeen(int id)
        {
            var analysis = await _context.AIAnalysys
                .SingleOrDefaultAsync(a => a.Id == id);

            if (analysis == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the current user has already seen this analysis
            if (_context.AIAnalysysSeen.Any(s => s.AnalysysId == analysis.Id && s.UserId == currentUserId))
            {
                return BadRequest("Already seen this analysis");
            }

            var seen = new AIAnalysysSeen
            {
                UserId = currentUserId,
                AnalysysId = id,
                WasSeen = true
            };

            _context.AIAnalysysSeen.Add(seen);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }
    }
}
