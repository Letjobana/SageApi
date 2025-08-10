using SageService.Domain.Entities;

namespace SageService.Domain.Interfaces
{
    /// <summary>
    /// Repository for LMS-related data, such as courses, learners and Sage IDs.
    /// </summary>
    public interface ILmsRepository
    {
        /// <summary>
        /// Retrieves the course information for the given course ID.
        /// </summary>
        Task<Course?> GetCourseAsync(int courseId);

        /// <summary>
        /// Retrieves all learners enrolled in the given course.
        /// </summary>
        Task<IReadOnlyList<Learner>> GetLearnersForCourseAsync(int courseId);

        /// <summary>
        /// Gets the Sage Customer ID for the given learner.
        /// </summary>
        Task<int> GetSageCustomerIdForLearnerAsync(int learnerId);

        /// <summary>
        /// Updates the Sage Customer ID for the given learner and course.
        /// </summary>
        Task UpdateSageCustomerIdAsync(int learnerId, int courseId, int sageCustomerId, int apiResponseCode);

        /// <summary>
        /// Gets the Sage Product ID for the given course.
        /// </summary>
        Task<int> GetSageProductIdForCourseAsync(int courseId);

        /// <summary>
        /// Updates the Sage Product ID for the given course.
        /// </summary>
        Task UpdateSageProductIdAsync(int courseId, int sageProductId);

        /// <summary>
        /// Retrieves the Sage API credentials for the given provider.
        /// </summary>
        Task<ProviderApiCredentials> GetProviderApiCredentialsAsync(int providerId);
    }
}
