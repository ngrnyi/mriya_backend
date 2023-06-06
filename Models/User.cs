using Microsoft.AspNetCore.Identity;

namespace MessengerBackend.Models;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ConnectionId { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsPhoneNumberVerified { get; set; }
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<Contact> Contacts { get; set; }

}
