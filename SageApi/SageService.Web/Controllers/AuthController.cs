using Microsoft.AspNetCore.Mvc;
using SageService.Web.Models;
using SageService.Web.Services;

namespace SageService.Web.Controllers
{
    /// <summary>
    /// Handles logging in and giving you a token so you can talk to the secure parts of the API.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        /// <summary>
        ///  AuthController
        /// </summary>
        /// <param name="tokenService"></param>
        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }
        /// <summary>
        /// Logs you in  username/password for now)
        /// and returns a shiny new JWT token to use for other requests.
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Username == "admin" && request.Password == "password")
            {
                var token = _tokenService.GenerateToken(request.Username, request.TenantId);
                return Ok(new { token });
            }

            return Unauthorized("Invalid credentials");
        }
    }
}
