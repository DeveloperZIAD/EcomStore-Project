using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Business_layer.Dtos.Auth;

namespace Business_layer
{
    public static class AuthService
    {
        private const string Key = "$2y$10$yXC2KMkgSYtSpI.3zEhhNOu5yKosPSjlllfbm9G6yKz8gziRTmdfy"; // نفس اللي في appsettings
        private const string Issuer = "EcomStoreAPI";
        private const string Audience = "EcomStoreClients";

        public static AuthResponseDto GenerateToken(int userId, string email, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Role = role,
                UserId = userId
            };
        }
    }
}