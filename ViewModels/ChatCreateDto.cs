namespace MessengerBackend.ViewModels;
public class ChatCreateDto
{
    public string Name { get; set; }
    public bool IsGroupChat { get; set; }
    public string OtherUserId { get; set; } // Only used for one-to-one chats
    public List<string> UserIds { get; set; } // Only used for group chats
}