using SageService.Domain.Entities;

namespace SageService.Domain.Interfaces
{
    /// <summary>
    /// Client Interface
    /// </summary>
    public interface ISageClient
    {
        /// <summary>
        /// Talks to Sage One: creates/looks up products (items), customers and invoices.
        /// All calls are async and safe to cancel.
        /// </summary>
        public interface ISageClient
        {
            /// <summary>
            /// Makes sure the provider's Sage CompanyID is available (fetches and caches if needed).
            /// </summary>
            Task EnsureCompanyIdAsync(int providerId, CancellationToken ct = default);

            /// <summary>
            /// Returns the Sage Item (product) ID for this course. Creates it if it doesn't exist.
            /// </summary>
            Task<int> GetOrCreateProductForCourseAsync(Course course, CancellationToken ct = default);

            /// <summary>
            /// Returns the Sage Customer ID for this learner. Creates it if it doesn't exist.
            /// </summary>
            Task<int> GetOrCreateCustomerForLearnerAsync(int providerId, Learner learner, Course course, CancellationToken ct = default);

            /// <summary>
            /// True if an unpaid invoice already exists for this customer + course reference.
            /// </summary>
            Task<bool> HasUnpaidInvoiceAsync(int providerId, int sageCustomerId, string projectReference, CancellationToken ct = default);

            /// <summary>
            /// Creates a new invoice in Sage for the given learner and course details.
            /// </summary>
            Task<InvoiceResult> CreateInvoiceAsync(int providerId, int sageCustomerId,int sageProductId,decimal courseValue, string courseTitle, string projectReference,CancellationToken ct = default);
        }
    }
}
