using System;
namespace MessengerBackend.ViewModels
{
    public class RegisterWithPhoneNumberRequest
    {
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

}

