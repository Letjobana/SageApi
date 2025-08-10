
using Microsoft.Extensions.Logging;
using SageService.Domain.Interfaces;

namespace SageService.Application
{
    /// <summary>
    /// Background-friendly use case to generate invoices for all learners in a course.
    /// </summary>
    public sealed class CourseInvoiceJobService : ICourseInvoiceJobService
    {
        private readonly ILogger<CourseInvoiceJobService> _logger;
        private readonly ILmsRepository _lmsRepository;
        private readonly ISageClient _sageClient;

        public CourseInvoiceJobService(ILogger<CourseInvoiceJobService> logger, ILmsRepository lmsRepository, ISageClient sageClient)
        {
            _logger = logger;
            _lmsRepository = lmsRepository;
            _sageClient = sageClient;
        }
        public Task<string> EnqueueCourseInvoicesAsync(int courseId)
        {
            // keep API responsive; actual work will move to a background worker next step.
            _logger.LogInformation("Invoice job requested for CourseId={CourseId} at {UtcNow}", courseId, DateTime.UtcNow);

            // For now return a fake id so the controller has something to show.
            var jobId = Guid.NewGuid().ToString("N");
            return Task.FromResult(jobId);
        }
    }
}
