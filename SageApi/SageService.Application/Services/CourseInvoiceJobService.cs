using Hangfire;
using Microsoft.Extensions.Logging;
using SageService.Domain.Entities;
using SageService.Domain.Interfaces;

namespace SageService.Application
{
    /// <summary>
    /// Generate invoices for all learners in a course.
    /// </summary>
    public sealed class CourseInvoiceJobService : ICourseInvoiceJobService
    {
        private readonly ILogger<CourseInvoiceJobService> _logger;
        private readonly ILmsRepository _lms;
        private readonly ISageClient _sage;

        public CourseInvoiceJobService(ILogger<CourseInvoiceJobService> logger, ILmsRepository lmsRepository, ISageClient sageClient)
        {
            _logger = logger;
            _lms = lmsRepository;
            _sage = sageClient;
        }

        public Task<string> EnqueueCourseInvoicesAsync(int courseId)
        {
            var jobId = BackgroundJob.Enqueue(() => ProcessCourseInvoicesAsync(courseId, CancellationToken.None));
            _logger.LogInformation("Enqueued invoice job {JobId} for course {CourseId}", jobId, courseId);
            return Task.FromResult(jobId);
        }

        [JobDisplayName("Process course invoices (CourseId: {0})")]
        public async Task ProcessCourseInvoicesAsync(int courseId, CancellationToken ct)
        {
            _logger.LogInformation("Started job for CourseId={CourseId}", courseId);

            var course = await _lms.GetCourseAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("CourseId={CourseId} not found. Aborting.", courseId);
                return;
            }

            await _sage.EnsureCompanyIdAsync(course.ProviderId, ct);

            // Ensure Sage product for this course
            var sageProductId = await _lms.GetSageProductIdForCourseAsync(courseId);
            if (sageProductId == 0)
            {
                _logger.LogInformation("No Sage product for course {CourseId}. Creating…", courseId);
                sageProductId = await _sage.GetOrCreateProductForCourseAsync(course, ct);
                await _lms.UpdateSageProductIdAsync(courseId, sageProductId);
            }

            // Load learners
            var learners = await _lms.GetLearnersForCourseAsync(courseId);
            if (learners.Count == 0)
            {
                _logger.LogInformation("No learners for CourseId={CourseId}. Done.", courseId);
                return;
            }

            // Limited parallelism = 5
            const int maxParallel = 5;
            var throttler = new SemaphoreSlim(maxParallel);

            var tasks = learners.Select(async learner =>
            {
                await throttler.WaitAsync(ct);
                try
                {
                    await ProcessOneLearnerAsync(course, learner, sageProductId, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing learner {LearnerId} for course {CourseId}", learner.Id, course.Id);
                }
                finally
                {
                    throttler.Release();
                }
            }).ToList();

            await Task.WhenAll(tasks);

            _logger.LogInformation("Finished job for CourseId={CourseId}", courseId);
        }

        private async Task ProcessOneLearnerAsync(Course course, Learner learner, int sageProductId, CancellationToken ct)
        {
            // Ensure Sage customer
            var sageCustomerId = await _lms.GetSageCustomerIdForLearnerAsync(learner.Id);
            if (sageCustomerId == 0)
            {
                sageCustomerId = await _sage.GetOrCreateCustomerForLearnerAsync(course.ProviderId, learner, course, ct);
                await _lms.UpdateSageCustomerIdAsync(learner.Id, course.Id, sageCustomerId, 200);
            }

            // Skip if unpaid invoice already exists for this reference
            var hasUnpaid = await _sage.HasUnpaidInvoiceAsync(course.ProviderId, sageCustomerId, course.ProjectReference, ct);
            if (hasUnpaid)
            {
                _logger.LogInformation("Skip learner {LearnerId} (unpaid invoice exists).", learner.Id);
                return;
            }

            // Create invoice
            var result = await _sage.CreateInvoiceAsync(course.ProviderId, sageCustomerId, sageProductId, course.Value, course.Title, course.ProjectReference, ct);

            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                var level = (result.MessageType ?? "INFO").ToUpperInvariant();
                if (result.Success)
                    _logger.LogInformation("{Level}: {Message}", level, result.Message);
                else
                    _logger.LogWarning("{Level}: {Message}", level, result.Message);
            }
        }
    }
}
