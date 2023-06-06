using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System;
namespace MessengerBackend.Helpers
{

    public class TwilioHelper
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhoneNumber;

        public TwilioHelper(string accountSid, string authToken, string fromPhoneNumber)
        {
            _accountSid = accountSid;
            _authToken = authToken;
            _fromPhoneNumber = fromPhoneNumber;
        }

        public void SendSms(string toPhoneNumber, string message)
        {
            TwilioClient.Init(_accountSid, _authToken);

            var messageResource = MessageResource.Create(
                body: message,
                from: new Twilio.Types.PhoneNumber(_fromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(toPhoneNumber)
            );
        }
    }

}

