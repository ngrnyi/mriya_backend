using System;
namespace MessengerBackend.Models
{
    public class Contact
    {
        public int Id { get; set; } // A unique identifier for the contact relationship
        public string UserId { get; set; } // The user ID of the user who has this contact in their list
        public User User { get; set; } // The user who has this contact in their list (Navigation property)
        public string ContactId { get; set; } // The user ID of the contact in the user's list
        public User ContactUser { get; set; } // The contact in the user's list (Navigation property)
        public string Nickname { get; set; } // The nickname the user has given to this contact (Optional)
    }
}


