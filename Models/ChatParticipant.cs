using System;
namespace MessengerBackend.Models
{
    public class ChatParticipant
    {
        public int Id { get; set; }
        public string ChatId { get; set; }
        public Chat Chat { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}

