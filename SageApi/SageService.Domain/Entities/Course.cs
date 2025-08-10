
namespace SageService.Domain.Entities
{
    /// <summary>
    /// Course info needed to sync with Sage
    /// </summary>
    public class Course
    {
        public int Id { get; set; }                 
        public int ProviderId { get; set; }         
        public string Title { get; set; } = string.Empty;
        public string ProjectReference { get; set; } = string.Empty; 
        public decimal Value { get; set; }          
        public int SageProductId { get; set; }
    }
}
