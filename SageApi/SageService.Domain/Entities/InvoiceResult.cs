namespace SageService.Domain.Entities
{
    /// <summary>
    /// Result from creating an invoice in Sage.
    /// </summary>
    public class InvoiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty; 
        public string MessageType { get; set; } = "INFO";
    }
}
