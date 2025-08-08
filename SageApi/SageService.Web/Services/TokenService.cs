using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SageService.Web.Services
{
    /// <summary>
    /// Provides functionality for generating JSON Web Tokens (JWT) 
    /// to authenticate and authorize API users.
    /// </summary>
    public class TokenService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenService"/> class.
        /// </summary>
        /// <param name="config">Application configuration for reading JWT settings.</param>
        public TokenService(IConfiguration config) => _config = config;

        /// <summary>
        /// Generates a signed JWT token containing the username and tenant ID claims.
        /// </summary>
        /// <param name="username">The username of the authenticated user.</param>
        /// <param name="tenantId">The tenant identifier for multi-tenant support.</param>
        /// <returns>A signed JWT token string.</returns>
        public string GenerateToken(string username, string tenantId)
        {
            var jwt = _config.GetSection("JwtSettings");

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username),
                new("tenant_id", tenantId)
            };

            // Symmetric key signing for internal services
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

