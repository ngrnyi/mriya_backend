using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MessengerBackend.Helpers
{
    public class SignalRServiceUtils
    {
        private readonly string _endpoint;
        private readonly string _accessKey;

        public SignalRServiceUtils(string endpoint, string accessKey)
        {
            _endpoint = endpoint;
            _accessKey = accessKey;
        }

        public string GenerateAccessToken(string hubName, string userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var symmetricKey = Convert.FromBase64String(_accessKey);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _endpoint,
                audience: hubName,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
