using System;
namespace MessengerBackend.Models
{
	public class AIAnalysys
	{
        public int Id { get; set; } 
        public string UserId { get; set; }
        public string ChatId { get; set; }
        public int MessageId { get; set; }
        public User User { get; set; } 
        public Chat Chat { get; set; }
        public Message Message { get; set; }
        public string Reason { get; set; }
        public bool WasSeen { get; set; }
    }
}

