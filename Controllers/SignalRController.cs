using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MessengerBackend.Helpers;

namespace MessengerBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SignalRController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SignalRController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetSignalRToken")]
        public IActionResult GetSignalRToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var connectionString = _configuration.GetConnectionString("AzureSignalR");
            var (endpoint, accessKey) = ParseSignalRConnectionString(connectionString);

            var hubName = "chathub";
            var signalRServiceUtils = new SignalRServiceUtils(endpoint, accessKey);
            var token = signalRServiceUtils.GenerateAccessToken(hubName, userId);

            return Ok(new { token });
        }
        private static (string Endpoint, string AccessKey) ParseSignalRConnectionString(string connectionString)
        {
            string endpoint = null;
            string accessKey = null;

            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.StartsWith("Endpoint="))
                {
                    endpoint = part.Substring("Endpoint=".Length);
                }
                else if (part.StartsWith("AccessKey="))
                {
                    accessKey = part.Substring("AccessKey=".Length);
                }
            }

            if (endpoint == null || accessKey == null)
            {
                throw new ArgumentException("Invalid connection string format.");
            }

            return (Endpoint: endpoint, AccessKey: accessKey);
        }

    }
}
