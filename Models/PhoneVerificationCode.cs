namespace MessengerBackend.Models;
public class PhoneVerificationCode
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; }
    public string Code { get; set; }
    public DateTimeOffset ExpirationTime { get; set; }
    public bool Verified { get; set; }

}