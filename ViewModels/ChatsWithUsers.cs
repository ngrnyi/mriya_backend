using System;
using MessengerBackend.Models;
namespace MessengerBackend.ViewModels
{
	public class ChatsWithUsers
    {
        public string ChatId { get; set; }
		public string ChatName { get; set; }
		public bool isGroupChat { get; set; }
		public List<User> Participants { get; set; }

        public ChatsWithUsers()
		{
		}
	}
}

