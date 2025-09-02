using System.Data;
using SageService.Domain.Entities;

namespace SageService.Domain.Interfaces
{
    /// <summary>
    /// LMS database operations needed by the Sage integration
    /// </summary>
    public interface ILmsRepository
    {
        /// <summary>
        /// Gets one course by ID
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        Task<Course> GetCourseAsync(int courseId);

        /// <summary>
        /// Gets all learners enrolled in a course
        /// </summary>
        /// <param name="courseId">Course Id</param>
        /// <returns></returns>
        Task<IReadOnlyList<Learner>> GetLearnersForCourseAsync(int courseId);

        /// <summary>
        /// Returns Sage Customer ID for a learner
        /// </summary>
        /// <param name="learnerId">Learner Id</param>
        /// <returns></returns>
        Task<int> GetSageCustomerIdForLearnerAsync(int learnerId);

        /// <summary>
        /// Updates Sage Customer ID + last API response for a learner/course
        /// </summary>
        /// <param name="learnerId">Learner Id</param>
        /// <param name="courseId">Course Id</param>
        /// <param name="sageCustomerId">Sage Customer Id</param>
        /// <param name="apiResponseCode">Api Response Code</param>
        /// <returns></returns>
        Task UpdateSageCustomerIdAsync(int learnerId, int courseId, int sageCustomerId, int apiResponseCode);

        /// <summary>
        /// Returns Sage Product ID for a course
        /// </summary>
        /// <param name="courseId">Course Id</param>
        /// <returns></returns>
        Task<int> GetSageProductIdForCourseAsync(int courseId);

        /// <summary>
        /// Updates Sage Product ID on a course
        /// </summary>
        /// <param name="courseId">CourseId</param>
        /// <param name="sageProductId">Sage Product Id</param>
        /// <returns></returns>
        Task UpdateSageProductIdAsync(int courseId, int sageProductId);

        /// <summary>
        /// Gets Sage API credentials for a provider
        /// </summary>
        /// <param name="providerId">Provider Id</param>
        /// <returns></returns>
        Task<ProviderApiCredentials> GetProviderApiCredentialsAsync(int providerId);

        /// <summary>
        /// Updates Sage Company ID on the provider record
        /// </summary>
        /// <param name="providerId">Provider Id</param>
        /// <param name="companyId">Company Id</param>
        /// <returns></returns>
        Task UpdateProviderSageCompanyIdAsync(int providerId, int companyId);

        /// <summary>
        /// Resolves course/learner/product from a reference
        /// </summary>
        /// <param name="providerId">Provider Id</param>
        /// <param name="documentReference">Document Reference</param>
        /// <returns></returns>
        Task<CourseResolution> ResolveCourseAndLearnerAsync(int providerId, string documentReference);

        /// <summary>
        /// Returns all statement header existing map: SageCustomerId -> LearnerStatementId.
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<int, int>> GetExistingStatementHeadersAsync();

        /// <summary>
        /// Returns existing statement-line keys to avoid duplicates
        /// </summary>
        /// <returns></returns>
        Task<HashSet<string>> GetExistingStatementLineKeysAsync();

        /// <summary>
        /// Bulk inserts new statement headers
        /// </summary>
        /// <param name="headerTable">Header Table</param>
        /// <returns></returns>
        Task BulkInsertStatementHeadersAsync(DataTable headerTable);

        /// <summary>
        /// Bulk inserts new statement lines
        /// </summary>
        /// <param name="lineTable">Line Table</param>
        /// <returns></returns>
        Task BulkInsertStatementLinesAsync(DataTable lineTable);

        /// <summary>
        /// Updates a statement header aggregate for a given LearnerStatementId
        /// </summary>
        /// <param name="learnerStatementId">Learner Statement Id</param>
        /// <param name="update">Update</param>
        /// <returns></returns>
        Task UpdateStatementHeaderAsync(int learnerStatementId, StatementHeaderUpdate update);

        /// <summary>
        /// Gets provider + learner statement detail rows for PDF generation
        /// </summary>
        /// <param name="providerId">provider Id"</param>
        /// <param name="learnerStatementId">Learner Statement Id</param>
        /// <returns></returns>
        Task<Tuple<DataRow, List<DataRow>>> GetStatementDataAsync(int providerId, int learnerStatementId);

        /// <summary>
        /// Saves generated Statement PDF path for a LearnerStatementId
        /// </summary>
        /// <param name="learnerStatementId">Learner Statement Id</param>
        /// <param name="relativeOrFullPath">Relative Or FullPath</param>
        /// <returns></returns>
        Task SaveStatementPdfPathAsync(int learnerStatementId, string relativeOrFullPath);

        /// <summary>
        /// Gets an existing statement PDF path (if any)
        /// </summary>
        /// <param name="learnerStatementId">LearnerStatementId</param>
        /// <returns></returns>
        Task<string> GetStatementPdfPathAsync(int learnerStatementId);

        /// <summary>
        /// Returns provider statements
        /// </summary>
        /// <param name="providerId">Provider Id</param>
        /// <param name="searchText">Search Text</param>
        /// <returns></returns>
        Task<DataTable> GetProviderStatementsAsync(int providerId, string searchText);

        /// <summary>
        /// Gets a Person folder GUID by Sage Customer ID (for storing PDFs
        /// </summary>
        /// <param name="sageCustomerId">Sage Customer Id</param>
        /// <returns></returns>
        Task<string> GetPersonFolderGuidByCustomerIdAsync(int sageCustomerId);
    }
}
