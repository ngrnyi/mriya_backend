using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MessengerBackend.Models;
using MessengerBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerBackend.Data;

namespace MessengerBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly MessengerDbContext _context;
        private readonly UserManager<User> _userManager;

        public ChatsController(MessengerDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Chats
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Chat>>> GetUserChats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var chats = await _context.ChatParticipants
                .Include(cp => cp.Chat)
                .Where(cp => cp.UserId == userId)
                .Select(cp => cp.Chat)
                .Include(cp=>cp.ChatParticipants)
                .ToListAsync();

            return Ok(chats);
        }

        [HttpGet("{chatId}/participants")]
        public async Task<ActionResult<Dictionary<string, string>>> GetChatParticipants(string chatId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the current user is part of the chat
            var isUserPartOfChat = await _context.ChatParticipants.AnyAsync(cp => cp.ChatId == chatId && cp.UserId == userId);

            if (!isUserPartOfChat)
            {
                return Unauthorized();
            }

            // Get the participants of the chat
            var participants = await _context.ChatParticipants
                .Include(cp => cp.User)
                .Where(cp => cp.ChatId == chatId)
                .Select(cp => cp.User)
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            return Ok(participants);
        }

        [HttpGet("GetUserChatsWithUsers")]
        public async Task<ActionResult<IEnumerable<ChatsWithUsers>>> GetUserChatsWithUsers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var chats = await _context.ChatParticipants
                .Include(cp => cp.Chat)
                .Where(cp => cp.UserId == userId)
                .Select(cp => cp.Chat)
                .ToListAsync();
            var groupChatsWithUsers = new List<ChatsWithUsers>();
            foreach(Chat chat in chats)
            {
                var participants = await _context.ChatParticipants.Where(x => x.ChatId == chat.Id).Select(x=>x.User).ToListAsync();
                var chatWithUser = new ChatsWithUsers();
                chatWithUser.ChatId = chat.Id;
                chatWithUser.ChatName = chat.Name;
                chatWithUser.Participants = participants;
                chatWithUser.isGroupChat = chat.IsGroupChat;

                groupChatsWithUsers.Add(chatWithUser);
            }

            return Ok(groupChatsWithUsers);
        }

        [HttpGet("GetPrivateChatWithUser/{contactId}")]
        public async Task<IActionResult> GetPrivateChatWithUser(string contactId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var privateChats = await _context.Chats
                .Include(c => c.ChatParticipants)
                .Where(c => !c.IsGroupChat && c.ChatParticipants.Any(cp => cp.UserId == userId))
                .ToListAsync();

            var privateChatWithContact = privateChats.FirstOrDefault(c => c.ChatParticipants.Any(cp => cp.UserId == contactId));

            if (privateChatWithContact == null)
            {
                return NotFound("Private chat with the specified contact not found");
            }
            var idOfChat = privateChatWithContact.Id;
            return Ok(idOfChat);
        }

        [HttpPost("CheckOrCreate/{contactId}")]
        public async Task<ActionResult<Chat>> CheckOrCreate(string contactId)
        {
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Check if there's already a chat between the two users
            var existingChat = await _context.ChatParticipants
                .Where(cp => cp.UserId == currentUserId)
                .Select(cp => cp.Chat)
                .Where(c => c.ChatParticipants.Any(cp => cp.UserId == contactId))
                .FirstOrDefaultAsync();

            if (existingChat != null)
            {
                return existingChat;
            }

            // Create a new chat
            Chat chat = new Chat
            {
                Name = $"{currentUserId}-{contactId}",
                IsGroupChat = false
            };
            _context.Chats.Add(chat);

            // Add chat participants
            ChatParticipant participant1 = new ChatParticipant
            {
                Chat = chat,
                UserId = currentUserId,
            };

            ChatParticipant participant2 = new ChatParticipant
            {
                Chat = chat,
                UserId = contactId,
            };

            _context.ChatParticipants.AddRange(participant1, participant2);

            // Save changes
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetChat", new { id = chat.Id }, chat);
        }

        [HttpPost("group")]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var chat = new Chat
                {
                    Name = model.GroupName,
                    IsGroupChat = true
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();

                var chatUser = new ChatParticipant
                {
                    ChatId = chat.Id,
                    UserId = userId
                };

                _context.ChatParticipants.Add(chatUser);

                foreach (var contactId in model.ContactIds)
                {
                    var contact = await _context.Users.FindAsync(contactId);
                    if (contact == null)
                    {
                        return NotFound($"Contact with ID {contactId} not found.");
                    }

                    var chatUserToAdd = new ChatParticipant
                    {
                        ChatId = chat.Id,
                        UserId = contact.Id
                    };

                    _context.ChatParticipants.Add(chatUserToAdd);
                }

                await _context.SaveChangesAsync();

                return Ok("Group chat created successfully");
            }

            return BadRequest("Invalid model");
        }


        [HttpPost]
        public async Task<ActionResult<Chat>> PostChat(ChatCreateDto chatCreateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var chat = new Chat
            {
                Name = chatCreateDto.Name,
                IsGroupChat = chatCreateDto.IsGroupChat
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            var chatParticipant = new ChatParticipant
            {
                ChatId = chat.Id,
                UserId = userId
            };

            _context.ChatParticipants.Add(chatParticipant);

            if (chatCreateDto.IsGroupChat)
            {
                foreach (var participantId in chatCreateDto.UserIds)
                {
                    var groupParticipant = new ChatParticipant
                    {
                        ChatId = chat.Id,
                        UserId = participantId
                    };

                    _context.ChatParticipants.Add(groupParticipant);
                }
            }
            else
            {
                var oneToOneParticipant = new ChatParticipant
                {
                    ChatId = chat.Id,
                    UserId = chatCreateDto.OtherUserId
                };

                _context.ChatParticipants.Add(oneToOneParticipant);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetChat", new { id = chat.Id }, chat);
        }

    }
}
