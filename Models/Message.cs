using MessengerBackend.Models;

public class Message
{
    public int Id { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public bool isSuspended { get; set; }
    public bool isChecked { get; set; }
    public virtual User User { get; set; }
    public string ChatId { get; set; } // New property for ChatId
    public virtual Chat Chat { get; set; } // New property for Chat
}