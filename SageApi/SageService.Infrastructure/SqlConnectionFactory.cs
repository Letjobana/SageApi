using System.Data;
using System.Data.SqlClient;

namespace SageService.Infrastructure
{
    /// <summary>
    /// Simple factory so repositories can open SQL connections safely.
    /// </summary>
    public sealed class SqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Method to create a connection
        /// </summary>
        /// <returns></returns>
        public IDbConnection Create()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
