using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SageService.Web.Controllers
{
    /// <summary>
    /// Test controller with both public and private (secured) endpoints.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SecureTestController : ControllerBase
    {
        /// <summary>
        /// Public endpoint.  
        /// Anyone can call this without logging in — great for health checks.
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult Public()
        {
            return Ok(new { ok = true, message = "This is public. No token required." });
        }

        /// <summary>
        /// Private endpoint.  
        /// Only users with a valid JWT token can access this.  
        /// Returns the logged-in username and tenant info.
        /// </summary>
        [HttpGet("private")]
        [Authorize]
        public IActionResult Private()
        {
            var user = User.Identity?.Name ?? "Unknown";
            var tenant = User.FindFirstValue("tenant_id") ?? "Unknown Tenant";

            return Ok(new { ok = true, message = $"Welcome {user} from {tenant}!" });
        }
    }
}
