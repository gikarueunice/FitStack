using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace FitStackDBL.Repository
{
    public class BaseRepository
    {
        private readonly string _ConnectionString;

        public BaseRepository(string ConnectionString)
        {
            _ConnectionString = ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));
        }

        // Minimal stub implementations so derived repositories can compile.
        protected virtual Task<T?> QueryFirstOrDefaultAsync<T>(string storedProc, object? parameters = null)
        {
            return Task.FromResult<T?>(default);
        }

        protected virtual SqlConnection CreateConnection()
        {
            return new SqlConnection(_ConnectionString);
        }
    }
}
