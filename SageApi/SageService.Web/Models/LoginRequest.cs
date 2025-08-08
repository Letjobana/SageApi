namespace SageService.Web.Models
{
    /// <summary>
    /// Simple class used to pass login data from the client.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// The username of the person trying to log in.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The password for authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// The tenant ID representing the specific client or environment.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;
    }
}
