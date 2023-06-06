using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerBackend.Data;
namespace MessengerBackend.Controllers
{
    namespace MessengerBackend.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        [Authorize]
        public class MessagesController : ControllerBase
        {
            private readonly MessengerDbContext _context;

            public MessagesController(MessengerDbContext context)
            {
                _context = context;
            }

            // GET: api/Messages
            [HttpGet]
            public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
            {
                return await _context.Messages.Where(x=>x.isSuspended==false).ToListAsync();
            }

            // GET: api/Messages/5
            [HttpGet("{id}")]
            public async Task<ActionResult<Message>> GetMessage(int id)
            {
                var message = await _context.Messages.FindAsync(id);

                if (message == null)
                {
                    return NotFound();
                }

                return message;
            }

            // PUT: api/Messages/5
            [HttpPut("{id}")]
            public async Task<IActionResult> PutMessage(int id, Message message)
            {
                if (id != message.Id)
                {
                    return BadRequest();
                }

                _context.Entry(message).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(id))
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
            // GET: api/Messages/Chat/{chatId}
            [HttpGet("Chat/{chatId}")]
            public async Task<ActionResult<IEnumerable<Message>>> GetMessagesForChat(string chatId)
            {
                var messages = await _context.Messages
                                             .Where(m => m.ChatId == chatId && m.isSuspended==false)
                                             .ToListAsync();

                if (messages == null || messages.Count == 0)
                {
                    return Ok("no messages");
                }

                return messages;
            }

            // POST: api/Messages
            [HttpPost]
            public async Task<ActionResult<Message>> PostMessage(Message message)
            {
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetMessage", new { id = message.Id }, message);
            }

            // DELETE: api/Messages/5
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteMessage(int id)
            {
                var message = await _context.Messages.FindAsync(id);
                if (message == null)
                {
                    return NotFound();
                }

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            private bool MessageExists(int id)
            {
                return _context.Messages.Any(e => e.Id == id);
            }
        }
    }

}

