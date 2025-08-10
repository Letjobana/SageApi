namespace SageService.Domain.Interfaces
{
    /// <summary>
    /// Handles the process of generating invoices for an entire course's learners.
    /// Intended to run in background jobs.
    /// </summary>
    public interface ICourseInvoiceJobService
    {
        // <summary>
        /// Enqueues a background job to create invoices for all learners in the given course.
        /// Returns the background job ID.
        /// </summary>
        Task<string> EnqueueCourseInvoicesAsync(int courseId);
    }
}
