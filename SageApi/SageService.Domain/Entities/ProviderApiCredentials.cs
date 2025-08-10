namespace SageService.Domain.Entities
{
    /// <summary>
    /// Represents the API credentials for a given LMS provider.
    /// </summary>
    public class ProviderApiCredentials
    {
        /// <summary>
        /// API Key used for authenticating with Sage.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Username for Sage API authentication.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password for Sage API authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Sage Company ID associated with the provider.
        /// </summary>
        public int CompanyId { get; set; }
    }
}
