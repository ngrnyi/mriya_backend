using System;
using MessengerBackend.Models;
using Microsoft.AspNetCore.Mvc;
using MessengerBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MessengerBackend.ViewModels;

namespace MessengerBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly MessengerDbContext _context;
        private readonly UserManager<User> _userManager;


        public ContactsController(MessengerDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Contacts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactWithChatId>>> GetContacts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var contacts = await _context.Contacts
                .Where(c => c.UserId == userId)
                .Include(c => c.ContactUser)
                .ToListAsync();

            var contactsWithChatId = new List<ContactWithChatId>();

            foreach (var contact in contacts)
            {
                var chat = await _context.Chats
                    .Include(c => c.ChatParticipants)
                    .Where(c => !c.IsGroupChat && c.ChatParticipants.Any(cp => cp.UserId == userId))
                    .FirstOrDefaultAsync(c => c.ChatParticipants.Any(cp => cp.UserId == contact.ContactId));

                contactsWithChatId.Add(new ContactWithChatId { Contact = contact, ChatId = chat?.Id });
            }

            return contactsWithChatId;
        }


        // POST: api/Contacts
        [HttpPost]
            public async Task<IActionResult> AddContact([FromBody] AddContactViewModel model)
            {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUser = await _userManager.FindByIdAsync(currentUserId);

                var existingContact = await _context.Contacts
                .Where(c => c.UserId == currentUserId && c.ContactId == user.Id)
                .FirstOrDefaultAsync();

                if (existingContact != null)
                {
                    return BadRequest("The contact already exists");
                }
                // Create a new contact with the provided UserName and Nickname
                var newContact = new Contact
                {
                    User = currentUser,
                    ContactUser = user,
                    Nickname = model.Nickname
                };

                _context.Contacts.Add(newContact);

                // Check if a chat between the two users already exists
                var existingChat = await _context.ChatParticipants
                    .Where(cp => cp.UserId == currentUserId)
                    .Select(cp => cp.Chat)
                    .Where(c => !c.IsGroupChat)
                    .Where(c => c.ChatParticipants.Any(cp => cp.UserId == user.Id))
                    .FirstOrDefaultAsync();

                if (existingChat == null)
                {

                    var newChat = new Chat
                    {
                        IsGroupChat = false,
                        Name = $"{currentUserId}-{user.Id}",

                    };
                    _context.Chats.Add(newChat);

                    ChatParticipant participant1 = new ChatParticipant
                    {
                        Chat = newChat,
                        UserId = currentUserId,
                    };

                    ChatParticipant participant2 = new ChatParticipant
                    {
                        Chat = newChat,
                        UserId = user.Id,
                    };
                    _context.ChatParticipants.AddRange(participant1, participant2);
                }
                await _context.SaveChangesAsync();

                return Ok("Contact added successfully");
            }

            return BadRequest("Invalid model");
        }

        // DELETE: api/Contacts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}

