using System;
namespace MessengerBackend.ViewModels
{
    public class CreateGroupChatViewModel
    {
        public string GroupName { get; set; }
        public List<string> ContactIds { get; set; }
    }
}

