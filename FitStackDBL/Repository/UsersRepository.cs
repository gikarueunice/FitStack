using Dapper;
using FitStackDBL.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace FitStackDBL.Repository
{
    public class UsersRepository : BaseRepository, IUsersRepository
    {
        public UsersRepository(string connectionString) : base(connectionString) { }

        public async Task<int> CreateUser(Users user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new Exception("Email is required");

            using var connection = CreateConnection() as SqlConnection;

            if (connection == null)
                throw new InvalidOperationException("Connection is not SqlConnection");

            // Note: hashing here; callers may already provide hashed password.
            string hashedPassword = user.Password ?? string.Empty;
            try
            {
                hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password ?? string.Empty);
            }
            catch
            {
                // If BCrypt is not available or hashing fails, fall back to raw value (not recommended).
            }

            var parameters = new DynamicParameters();
            parameters.Add("@FullName", user.FullName);
            parameters.Add("@Email", user.Email);
            parameters.Add("@Password", hashedPassword);
            parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            try
            {
                await connection.ExecuteAsync(
                    "sp_CreateUser",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return parameters.Get<int>("@UserId");
            }
            catch (SqlException ex) when (ex.Number == 51000)
            {
                throw new Exception("Email already exists", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating user", ex);
            }
        }

        public async Task<Users?> GetByEmail(string email)
        {
            var parameters = new { Email = email };
            return await QueryFirstOrDefaultAsync<Users>("sp_GetUserByEmail", parameters);
        }

        public async Task<Users?> GetById(int userId)
        {
            var parameters = new { UserId = userId };
            return await QueryFirstOrDefaultAsync<Users>("sp_GetUserById", parameters);
        }

        public Task UpdateLoginStats(int userId)
        {
            // Stub: update last login or stats in DB. Implement as needed.
            return Task.CompletedTask;
        }
        // In UserService.cs
        public async Task<Users?> GetUserByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Users WHERE Id = @Id AND IsActive = 1";
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Users>(sql, new { Id = id });
        }
    }
}
