using Dapper;
using FitStackDBL.Model;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace FitStackDBL.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly object? _ConnectionString;
        private readonly ILogger<UserService> _logger;

        public UserService(IConfiguration configuration, ILogger<UserService> logger)
        {
            _configuration = configuration;
            _ConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        public async Task<Users?> GetUserByIdAsync(int id)
        {
            const string sql = @"
                SELECT * FROM Users WHERE Id = @Id;
                SELECT * FROM UserProfiles WHERE UserId = @Id;";

            using var connection = new SqlConnection((string)_ConnectionString);
            using var multi = await connection.QueryMultipleAsync(sql, new { Id = id });

            var user = await multi.ReadSingleOrDefaultAsync<Users>();
            if (user != null)
            {
                user.Profile = await multi.ReadSingleOrDefaultAsync<UserProfile>();
            }

            return user;
        }

        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            const string sql = "SELECT * FROM Users WHERE Email = @Email";
            using var connection = new SqlConnection((string)_ConnectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { Email = email });
        }

        public async Task<Users?> GetUserByVerificationTokenAsync(string token)
        {
            const string sql = "SELECT * FROM Users WHERE EmailVerificationToken = @Token";
            using var connection = new SqlConnection((string)_ConnectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { Token = token });
        }

        public async Task<Users?> GetUserByPasswordResetTokenAsync(string token)
        {
            const string sql = "SELECT * FROM Users WHERE PasswordResetToken = @Token AND PasswordResetTokenExpiry > @Now";
            using var connection = new SqlConnection((string)_ConnectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { Token = token, Now = DateTime.UtcNow });
        }

        public async Task<int> CreateUserAsync(Users user)
        {
            const string sql = @"
                INSERT INTO Users (
                    FullName, Email, PasswordHash, Salt, DateOfBirth, Gender, 
                    Height, Weight, FitnessGoal, ActivityLevel, IsEmailVerified,
                    EmailVerificationToken, SelectedPlan, SubscribeToNewsletter,
                    CreatedAt, IsActive
                ) VALUES (
                    @FullName, @Email, @PasswordHash, @Salt, @DateOfBirth, @Gender,
                    @Height, @Weight, @FitnessGoal, @ActivityLevel, @IsEmailVerified,
                    @EmailVerificationToken, @SelectedPlan, @SubscribeToNewsletter,
                    @CreatedAt, @IsActive
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = new SqlConnection((string)_ConnectionString);
            var id = await connection.ExecuteScalarAsync<int>(sql, user);

            // Create empty profile
            const string profileSql = "INSERT INTO UserProfiles (UserId) VALUES (@UserId)";
            await connection.ExecuteAsync(profileSql, new { UserId = id });

            return id;
        }

        public async Task UpdateUserAsync(Users user)
        {
            user.UpdatedAt = DateTime.UtcNow;

            const string sql = @"
                UPDATE Users SET
                    FullName = @FullName,
                    PasswordHash = @PasswordHash,
                    Salt = @Salt,
                    DateOfBirth = @DateOfBirth,
                    Gender = @Gender,
                    Height = @Height,
                    Weight = @Weight,
                    FitnessGoal = @FitnessGoal,
                    ActivityLevel = @ActivityLevel,
                    IsEmailVerified = @IsEmailVerified,
                    EmailVerificationToken = @EmailVerificationToken,
                    PasswordResetToken = @PasswordResetToken,
                    PasswordResetTokenExpiry = @PasswordResetTokenExpiry,
                    RefreshToken = @RefreshToken,
                    RefreshTokenExpiry = @RefreshTokenExpiry,
                    SelectedPlan = @SelectedPlan,
                    SubscribeToNewsletter = @SubscribeToNewsletter,
                    UpdatedAt = @UpdatedAt,
                    LastLoginAt = @LastLoginAt,
                    IsActive = @IsActive,
                    ProfilePictureUrl = @ProfilePictureUrl
                WHERE Id = @Id";

            using var connection = new SqlConnection((string)_ConnectionString);
            await connection.ExecuteAsync(sql, user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            const string sql = "DELETE FROM Users WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }
    
        public async Task<bool> HasExceededLoginAttemptsAsync(int userId)
        {
            const string sql = @"
        SELECT 
            CASE 
                WHEN LoginAttempts >= 5 AND LastLoginAttempt > DATEADD(minute, -15, GETUTCDATE()) 
                THEN 1 
                ELSE 0 
            END
        FROM Users 
        WHERE Id = @UserId";

            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<bool>(sql, new { UserId = userId });
        }

        public async Task ResetLoginAttemptsAsync(int userId)
        {
            const string sql = @"
        UPDATE Users 
        SET LoginAttempts = 0, 
            LastLoginAttempt = NULL 
        WHERE Id = @UserId";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { UserId = userId });
        }

        public async Task IncrementLoginAttemptsAsync(int userId)
        {
            const string sql = @"
        UPDATE Users 
        SET LoginAttempts = ISNULL(LoginAttempts, 0) + 1,
            LastLoginAttempt = GETUTCDATE()
        WHERE Id = @UserId";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { UserId = userId });
        }

        
    }
    
}
