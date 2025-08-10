namespace SageService.Domain.Entities
{
    /// <summary>
    /// Learner linked to a course, mapped to a Sage customer.
    /// </summary>
    public class Learner
    {
        public int Id { get; set; }                 
        public int PersonId { get; set; }           
        public int SageCustomerId { get; set; }    
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? PhysicalAddress1 { get; set; }
        public string? PhysicalAddress2 { get; set; }
        public string? PhysicalAddress3 { get; set; }
        public string? PhysicalPostalCode { get; set; }
        public string? PostalAddress1 { get; set; }
        public string? PostalAddress2 { get; set; }
        public string? PostalAddress3 { get; set; }
        public string? PostalAddressCode { get; set; }
    }
}
