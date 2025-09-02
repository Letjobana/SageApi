using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SageService.Domain.Entities;
using SageService.Domain.Interfaces;

namespace SageService.Infrastructure.Repositories
{
    /// <summary>
    /// SQL Server implementation backed by existing stored procedures.
    /// </summary>
    public sealed class LmsRepository : ILmsRepository
    {
        private readonly string _connString;
        private readonly ILogger<LmsRepository> _logger;

        public LmsRepository(IConfiguration config, ILogger<LmsRepository> logger)
        {
            var cs = config.GetConnectionString("strConnection");
            if (string.IsNullOrEmpty(cs))
                throw new InvalidOperationException("ConnectionStrings:strConnection is missing.");

            _connString = cs;
            _logger = logger;
        }


        public async Task<Course> GetCourseAsync(int courseId)
        {
            const string proc = "LMS_CourseGet";

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "GET_BY_ID");
                cmd.Parameters.AddWithValue("@CourseRecordID", courseId);

                await con.OpenAsync().ConfigureAwait(false);
                using (var rd = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (!await rd.ReadAsync().ConfigureAwait(false))
                        return null;

                    var course = new Course();
                    course.Id = courseId;
                    course.ProviderId = rd["Provider_RecordID"] is DBNull ? 0 : Convert.ToInt32(rd["Provider_RecordID"]);
                    course.Title = rd["Course_Title"] as string ?? string.Empty;
                    course.Value = rd["Course_Value"] is DBNull ? 0m : Convert.ToDecimal(rd["Course_Value"]);               
                    course.ProjectReference = rd["Project_Reference"] as string ?? (rd["Course_Reference"] as string ?? string.Empty);
                    course.SageProductId = rd["Sage_ProductID"] is DBNull ? 0 : Convert.ToInt32(rd["Sage_ProductID"]);
                    return course;
                }
            }
        }

        public async Task<IReadOnlyList<Learner>> GetLearnersForCourseAsync(int courseId)
        {
            const string proc = "LMS_CourseLearnersGet";
            var list = new List<Learner>();

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "BY_COURSE");
                cmd.Parameters.AddWithValue("@CourseRecordID", courseId);

                await con.OpenAsync().ConfigureAwait(false);
                using (var rd = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await rd.ReadAsync().ConfigureAwait(false))
                    {
                        var l = new Learner();
                        l.Id = rd["Learner_RecordID"] is DBNull ? 0 : Convert.ToInt32(rd["Learner_RecordID"]);
                        l.PersonId = rd["Person_RecordID"] is DBNull ? 0 : Convert.ToInt32(rd["Person_RecordID"]);
                        l.SageCustomerId = rd["Sage_Customer_ID"] is DBNull ? 0 : Convert.ToInt32(rd["Sage_Customer_ID"]);
                        l.FullName = rd["Full_Name"] as string ?? string.Empty;
                        l.Phone = rd["Phone"] as string;
                        l.Mobile = rd["Cellphone"] as string;
                        l.Email = rd["Email"] as string;
                        l.PhysicalAddress1 = rd["PhysicalAddress1"] as string;
                        l.PhysicalAddress2 = rd["PhysicalAddress2"] as string;
                        l.PhysicalAddress3 = rd["PhysicalAddress3"] as string;
                        l.PhysicalPostalCode = rd["PhysicalPostalCode"] as string;
                        l.PostalAddress1 = rd["PostalAddress1"] as string;
                        l.PostalAddress2 = rd["PostalAddress2"] as string;
                        l.PostalAddress3 = rd["PostalAddress3"] as string;
                        l.PostalAddressCode = rd["PostalAddressCode"] as string;

                        list.Add(l);
                    }
                }
            }

            return list;
        }

        public async Task<int> GetSageCustomerIdForLearnerAsync(int learnerId)
        {
            const string proc = "LMS_FinancialSysAddUpdate";

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "CHECKSAGECUSTEXISTS");
                cmd.Parameters.AddWithValue("@LearnerRecordID", learnerId);

                await con.OpenAsync().ConfigureAwait(false);
                var scalar = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                return scalar == null || scalar == DBNull.Value ? 0 : Convert.ToInt32(scalar);
            }
        }

        public async Task UpdateSageCustomerIdAsync(int learnerId, int courseId, int sageCustomerId, int apiResponseCode)
        {
            const string proc = "LMS_FinancialSysAddUpdate";

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "UPDATESAGECUSTOMERID");
                cmd.Parameters.AddWithValue("@LearnerRecordID", learnerId);
                cmd.Parameters.AddWithValue("@CourseRecordID", courseId);
                cmd.Parameters.AddWithValue("@SageCustomerID", sageCustomerId);
                cmd.Parameters.AddWithValue("@SageAPIResponse", apiResponseCode);

                await con.OpenAsync().ConfigureAwait(false);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> GetSageProductIdForCourseAsync(int courseId)
        {
            var course = await GetCourseAsync(courseId).ConfigureAwait(false);
            return course == null ? 0 : course.SageProductId;
        }

        public async Task UpdateSageProductIdAsync(int courseId, int sageProductId)
        {
            const string proc = "LMS_FinancialSysAddUpdate";

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "UPDATESAGEPRODUCTID");
                cmd.Parameters.AddWithValue("@CourseRecordId", courseId);
                cmd.Parameters.AddWithValue("@SageProductId", sageProductId);

                await con.OpenAsync().ConfigureAwait(false);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task<ProviderApiCredentials> GetProviderApiCredentialsAsync(int providerId)
        {
            const string proc = "LMS_ProvFinancialSysGet";
            var creds = new ProviderApiCredentials();

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "FINANCIALSYSAPIDETAILGET");
                cmd.Parameters.AddWithValue("@ProviderRecordID", providerId);

                await con.OpenAsync().ConfigureAwait(false);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var ds = new DataSet();
                    da.Fill(ds);

                    DataTable t = null;
                    if (ds.Tables.Contains("SageDetail"))
                    {
                        t = ds.Tables["SageDetail"];
                    }
                    else if (ds.Tables.Count > 0)
                    {
                        t = ds.Tables[0];
                    }

                    if (t != null && t.Rows.Count > 0)
                    {
                        var row = t.Rows[0];
                        creds.ApiKey = row["SageAPIKey"] as string ?? string.Empty;
                        creds.Username = row["SageUsername"] as string ?? string.Empty;
                        creds.Password = row["SagePassword"] as string ?? string.Empty;
                        creds.CompanyId = row["CompanyID"] is DBNull ? 0 : Convert.ToInt32(row["CompanyID"]);
                        creds.ProviderId = providerId;
                    }
                }
            }
            return creds;
        }

        public async Task UpdateProviderSageCompanyIdAsync(int providerId, int companyId)
        {
            const string proc = "LMS_FinancialSysAddUpdate";

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "UPDATESAGECOMPANYID");
                cmd.Parameters.AddWithValue("@ProviderRecordID", providerId);
                cmd.Parameters.AddWithValue("@CompanyID", companyId);

                await con.OpenAsync().ConfigureAwait(false);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task<CourseResolution> ResolveCourseAndLearnerAsync(int providerId, string documentReference)
        {
            const string proc = "LMS_FinancialSysAddUpdate";

            if (string.IsNullOrWhiteSpace(documentReference))
                return null;

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "RESOLVECOURSELEARNER");
                cmd.Parameters.AddWithValue("@ProviderRecordID", providerId);
                cmd.Parameters.AddWithValue("@DocumentNumber", documentReference);

                await con.OpenAsync().ConfigureAwait(false);
                using (var rd = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await rd.ReadAsync().ConfigureAwait(false))
                    {
                        var res = new CourseResolution();
                        res.CourseRecordId = Convert.ToInt32(rd["CourseRecordID"]);
                        res.CourseTitle = rd["CourseTitle"] as string ?? string.Empty;
                        res.SageProductId = Convert.ToInt32(rd["SageProductID"]);
                        res.LearnerRecordId = Convert.ToInt32(rd["LearnerRecordID"]);
                        res.Reference = rd["Reference"] as string ?? string.Empty;
                        return res;
                    }
                }
            }
            return null;
        }

        public async Task<Dictionary<int, int>> GetExistingStatementHeadersAsync()
        {
            const string proc = "LMS_FinancialSysAddUpdate";
            var map = new Dictionary<int, int>();

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "GETALLSTATEMENTHEADERS");

                await con.OpenAsync().ConfigureAwait(false);
                using (var rd = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await rd.ReadAsync().ConfigureAwait(false))
                    {
                        var customerId = rd.GetInt32(0);
                        var learnerStatId = rd.GetInt32(1);
                        map[customerId] = learnerStatId;
                    }
                }
            }
            return map;
        }

        public async Task<HashSet<string>> GetExistingStatementLineKeysAsync()
        {
            const string proc = "LMS_FinancialSysAddUpdate";
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "GETEXISTINGSTATEMENTLINEKEYS");

                await con.OpenAsync().ConfigureAwait(false);
                using (var rd = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await rd.ReadAsync().ConfigureAwait(false))
                    {
                        keys.Add(rd.GetString(0));
                    }
                }
            }
            return keys;
        }

        public async Task BulkInsertStatementHeadersAsync(DataTable headerTable)
        {
            if (headerTable == null || headerTable.Rows.Count == 0)
                return;

            using (var con = new SqlConnection(_connString))
            using (var bulk = new SqlBulkCopy(con))
            {
                bulk.DestinationTableName = "LMS_Learner_Sage_Statements";
                foreach (DataColumn col in headerTable.Columns)
                {
                    bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }
                await con.OpenAsync().ConfigureAwait(false);
                await bulk.WriteToServerAsync(headerTable).ConfigureAwait(false);
            }
        }

        public async Task BulkInsertStatementLinesAsync(DataTable lineTable)
        {
            if (lineTable == null || lineTable.Rows.Count == 0)
                return;

            using (var con = new SqlConnection(_connString))
            using (var bulk = new SqlBulkCopy(con))
            {
                bulk.DestinationTableName = "LMS_Learner_Sage_Statement_Lines";
                foreach (DataColumn col in lineTable.Columns)
                {
                    bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }
                await con.OpenAsync().ConfigureAwait(false);
                await bulk.WriteToServerAsync(lineTable).ConfigureAwait(false);
            }
        }

        public async Task UpdateStatementHeaderAsync(int learnerStatementId, StatementHeaderUpdate update)
        {
            const string proc = "LMS_FinancialSysAddUpdate";

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "UPDATESTATEMENTHEADER");
                cmd.Parameters.AddWithValue("@LearnerStatRecordID", learnerStatementId);
                cmd.Parameters.AddWithValue("@TotalDue", update.TotalDue);
                cmd.Parameters.AddWithValue("@TotalPaid", update.TotalPaid);
                cmd.Parameters.AddWithValue("@Days30", update.Days30);
                cmd.Parameters.AddWithValue("@Days60", update.Days60);
                cmd.Parameters.AddWithValue("@Days90", update.Days90);
                cmd.Parameters.AddWithValue("@Days120Plus", update.Days120Plus);
                cmd.Parameters.AddWithValue("@RowHash", update.RowHash ?? string.Empty);

                await con.OpenAsync().ConfigureAwait(false);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task<Tuple<DataRow, List<DataRow>>> GetStatementDataAsync(int providerId, int learnerStatementId)
        {
            var providerTable = new DataTable();
            // Learner statement detail
            var detailTable = new DataTable();

            // Provider detail
            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand("LMS_ProviderGet", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "GET_BY_ID");
                cmd.Parameters.AddWithValue("@ProviderRecordID", providerId);

                using (var da = new SqlDataAdapter(cmd))
                {
                    await Task.Run(() => da.Fill(providerTable)).ConfigureAwait(false);
                }
            }

            // Statement details
            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand("LMS_FinancialSysAddUpdate", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "GETLEARNERSAGESTATEMENTDETAIL");
                cmd.Parameters.AddWithValue("@ProviderRecordID", providerId);
                cmd.Parameters.AddWithValue("@LearnerStatementID", learnerStatementId);

                using (var da = new SqlDataAdapter(cmd))
                {
                    await Task.Run(() => da.Fill(detailTable)).ConfigureAwait(false);
                }
            }

            if (providerTable.Rows.Count == 0 || detailTable.Rows.Count == 0)
                return null;

            // Filter to the target LearnerStatementId
            var list = new List<DataRow>();
            foreach (DataRow r in detailTable.Rows)
            {
                var idObj = r["Stat_RecordID"];
                var id = idObj is DBNull ? 0 : Convert.ToInt32(idObj);
                if (id == learnerStatementId)
                {
                    list.Add(r);
                }
            }

            if (list.Count == 0) 
                return null;
            return Tuple.Create(providerTable.Rows[0], list);
        }

        public async Task SaveStatementPdfPathAsync(int learnerStatementId, string relativeOrFullPath)
        {
            const string proc = "LMS_FinancialSysAddUpdate";

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "UPDATEPDFPATH");
                cmd.Parameters.AddWithValue("@LearnerStatementID", learnerStatementId);
                cmd.Parameters.AddWithValue("@PDFPath", relativeOrFullPath ?? string.Empty);

                await con.OpenAsync().ConfigureAwait(false);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task<string> GetStatementPdfPathAsync(int learnerStatementId)
        {
            const string proc = "LMS_FinancialSysAddUpdate";
            var pdf = string.Empty;

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "GETPDFPATHBYID");
                cmd.Parameters.AddWithValue("@LearnerStatRecordID", learnerStatementId);

                await con.OpenAsync().ConfigureAwait(false);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    await Task.Run(() => da.Fill(dt)).ConfigureAwait(false);
                    if (dt.Rows.Count > 0 && dt.Rows[0]["StatementPDFPath"] != DBNull.Value)
                        pdf = dt.Rows[0]["StatementPDFPath"].ToString();
                }
            }
            return pdf;
        }

        public async Task<DataTable> GetProviderStatementsAsync(int providerId, string searchText)
        {
            const string proc = "LMS_FinancialSysAddUpdate";
            var dt = new DataTable();

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "GETLEARNERSAGESTATEMENTS");
                cmd.Parameters.AddWithValue("@ProviderRecordID", providerId);
                cmd.Parameters.AddWithValue("@SearchText", string.IsNullOrWhiteSpace(searchText) ? "%%" : searchText);

                using (var da = new SqlDataAdapter(cmd))
                {
                    await Task.Run(() => da.Fill(dt)).ConfigureAwait(false);
                }
            }
            return dt;
        }

        public async Task<string> GetPersonFolderGuidByCustomerIdAsync(int sageCustomerId)
        {
            const string proc = "LMS_FinancialSysAddUpdate";
            var guid = string.Empty;

            using (var con = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(proc, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CMD", "GETPERSONFOLDERBYCUSTOMERID");
                cmd.Parameters.AddWithValue("@CustomerID", sageCustomerId);

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    await Task.Run(() => da.Fill(dt)).ConfigureAwait(false);
                    if (dt.Rows.Count > 0)
                        guid = dt.Rows[0]["Folder_GUID"] as string ?? string.Empty;
                }
            }
            return guid;
        }
    }
}
