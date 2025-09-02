namespace SageService.Domain.Entities
{
    /// <summary>
    /// Result of resolving course + learner info from a document reference
    /// (maps directly to what your stored proc RESOLVECOURSELEARNER returns).
    /// </summary>
    public sealed class CourseResolution
    {
        public int CourseRecordId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int SageProductId { get; set; }
        public int LearnerRecordId { get; set; }
        public string Reference { get; set; } = string.Empty;
    }
}
