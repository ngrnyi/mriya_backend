using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MessengerBackend.Data;
using MessengerBackend.Models;
using MessengerBackend.Helpers;

namespace MessengerBackend
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly MessengerDbContext _context;
        private readonly UserManager<User> _userManager;

        public ChatHub(MessengerDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SendMessage(string chatId, string message)
        {
            var userId = Context.UserIdentifier;
            var chat = await _context.Chats.FindAsync(chatId);

            if (chat == null)
            {
                await Clients.Caller.SendAsync("Error", "Chat not found");
                return;
            }

            var isParticipant = _context.ChatParticipants.Any(cp => cp.ChatId == chat.Id && cp.UserId == userId);

            if (!isParticipant)
            {
                await Clients.Caller.SendAsync("Error", "You are not a participant of this chat");
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);
            var messageToSend = message;

            var newMessage = new Message
            {
                Text = messageToSend,
                Timestamp = DateTime.Now,
                isSuspended = false,
                isChecked = false,
                UserId = userId,
                ChatId = chatId
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            var otherParticipants = _context.ChatParticipants
                .Where(cp => cp.ChatId == chat.Id && cp.UserId != userId)
                .Select(cp => cp.User);

            foreach (var participant in otherParticipants)
            {
                await Clients.User(participant.Id).SendAsync("ReceiveMessage", chatId, messageToSend, user.Id);
            }
        }

    }
}
