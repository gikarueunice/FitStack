using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FitStackDBL.Model;

namespace FitStackDBL.Services
{
    public class UserService : IUserService
    {
        private readonly string _connectionString;
        private readonly ILogger<UserService> _logger;

        public UserService(IConfiguration configuration, ILogger<UserService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        public async Task UpdateProfilePictureAsync(int userId, string pictureUrl, string picturePath)
        {
            const string sql = @"
        UPDATE Users 
        SET ProfilePictureUrl = @PictureUrl,
            ProfilePicturePath = @PicturePath,
            UpdatedAt = GETUTCDATE()
        WHERE Id = @UserId";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { UserId = userId, PictureUrl = pictureUrl, PicturePath = picturePath });
        }
        public async Task<Users?> GetUserByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Users WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { Id = id });
        }

        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            const string sql = "SELECT * FROM Users WHERE Email = @Email";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { Email = email });
        }
        public async Task<Users?> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            const string sql = "SELECT * FROM Users WHERE PhoneNumber = @PhoneNumber";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { PhoneNumber = phoneNumber });
        }
        public async Task<Users?> GetUserByVerificationTokenAsync(string token)
        {
            const string sql = "SELECT * FROM Users WHERE EmailVerificationToken = @Token";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { Token = token });
        }

        public async Task<Users?> GetUserByPasswordResetTokenAsync(string token)
        {
            const string sql = "SELECT * FROM Users WHERE PasswordResetToken = @Token AND PasswordResetTokenExpiry > @Now";
            using var connection = new SqlConnection(_connectionString);
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

            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task UpdateUserAsync(Users user)
        {
            user.UpdatedAt = DateTime.UtcNow;

            const string sql = @"
                UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
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
                    ProfilePictureUrl = @ProfilePictureUrl,
                    LoginAttempts = @LoginAttempts
                WHERE Id = @Id";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            const string sql = "DELETE FROM Users WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        //public async Task<bool> HasExceededLoginAttemptsAsync(int userId)
        //{
        //    const string sql = @"
        //        SELECT 
        //            CASE 
        //                WHEN LoginAttempts >= 5 AND LastLoginAttempt > DATEADD(minute, -15, GETUTCDATE()) 
        //                THEN 1 
        //                ELSE 0 
        //            END
        //        FROM Users 
        //        WHERE Id = @UserId";

        //    using var connection = new SqlConnection(_connectionString);
        //    return await connection.ExecuteScalarAsync<bool>(sql, new { UserId = userId });
        //}

        //public async Task ResetLoginAttemptsAsync(int userId)
        //{
        //    const string sql = @"
        //        UPDATE Users 
        //        SET LoginAttempts = 0, 
        //            LastLoginAttempt = NULL 
        //        WHERE Id = @UserId";

        //    using var connection = new SqlConnection(_connectionString);
        //    await connection.ExecuteAsync(sql, new { UserId = userId });
        //}

        //public async Task IncrementLoginAttemptsAsync(int userId)
        //{
        //    const string sql = @"
        //        UPDATE Users 
        //        SET LoginAttempts = ISNULL(LoginAttempts, 0) + 1,
        //            LastLoginAttempt = GETUTCDATE()
        //        WHERE Id = @UserId";

        //    using var connection = new SqlConnection(_connectionString);
        //    await connection.ExecuteAsync(sql, new { UserId = userId });
        //}

        public async Task<Users?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            const string sql = "SELECT * FROM Users WHERE RefreshToken = @RefreshToken AND RefreshTokenExpiry > @Now";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { RefreshToken = refreshToken, Now = DateTime.UtcNow });
        }
        public async Task<Users?> GetUserByEmailOTPAsync(string emailOTP)
        {
            const string sql = "SELECT * FROM Users WHERE EmailOTP = @EmailOTP AND EmailOTPExpiry > @Now";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { EmailOTP = emailOTP, Now = DateTime.UtcNow });
        }

        public async Task<Users?> GetUserByPhoneOTPAsync(string phoneOTP)
        {
            const string sql = "SELECT * FROM Users WHERE PhoneOTP = @PhoneOTP AND PhoneOTPExpiry > @Now";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { PhoneOTP = phoneOTP, Now = DateTime.UtcNow });
        }
        public async Task<Users?> GetUserByProfilePictureUrlAsync(string profilePictureUrl)
        {
            const string sql = "SELECT * FROM Users WHERE ProfilePictureUrl = @ProfilePictureUrl";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { ProfilePictureUrl = profilePictureUrl });
        }
        public async Task<Users?> GetUserBySelectedPlanAsync(string selectedPlan)
        {
            const string sql = "SELECT * FROM Users WHERE SelectedPlan = @SelectedPlan";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { SelectedPlan = selectedPlan });
        }
        public async Task<Users?> GetUserByFitnessGoalAsync(string fitnessGoal)
        {
            const string sql = "SELECT * FROM Users WHERE FitnessGoal = @FitnessGoal";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { FitnessGoal = fitnessGoal });
        }
        public async Task<Users?> GetUserByActivityLevelAsync(string activityLevel)
        {
            const string sql = "SELECT * FROM Users WHERE ActivityLevel = @ActivityLevel";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { ActivityLevel = activityLevel });
        }
        public async Task<Users?> GetUserBySubscriptionStatusAsync(bool subscribeToNewsletter)
        {
            const string sql = "SELECT * FROM Users WHERE SubscribeToNewsletter = @SubscribeToNewsletter";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { SubscribeToNewsletter = subscribeToNewsletter });
        }
        public async Task<Users?> GetUserByIsActiveAsync(bool isActive)
        {
            const string sql = "SELECT * FROM Users WHERE IsActive = @IsActive";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { IsActive = isActive });
        }
        public async Task<Users?> GetUserByIsEmailVerifiedAsync(bool isEmailVerified)
        {
            const string sql = "SELECT * FROM Users WHERE IsEmailVerified = @IsEmailVerified";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { IsEmailVerified = isEmailVerified });
        }
        public async Task<Users?> GetUserByIsPhoneVerifiedAsync(bool isPhoneVerified)
        {
            const string sql = "SELECT * FROM Users WHERE IsPhoneVerified = @IsPhoneVerified";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { IsPhoneVerified = isPhoneVerified });
        }
        public async Task<Users?> GetUserByIsLockedAsync(bool isLocked)
        {
            const string sql = "SELECT * FROM Users WHERE IsLocked = @IsLocked";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { IsLocked = isLocked });
        }
        public async Task<Users?> GetUserByLockedUntilAsync(DateTime? lockedUntil)
        {
            const string sql = "SELECT * FROM Users WHERE LockedUntil > @LockedUntil";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { LockedUntil = lockedUntil });
        }
        public async Task<Users?> VerifyOTPRequest (string email, string phoneNumber, string otp)
        {
            const string sql = @"
                SELECT * FROM Users 
                WHERE (Email = @Email OR PhoneNumber = @PhoneNumber) 
                AND (EmailOTP = @OTP AND EmailOTPExpiry > @Now OR PhoneOTP = @OTP AND PhoneOTPExpiry > @Now)";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(sql, new { Email = email, PhoneNumber = phoneNumber, OTP = otp, Now = DateTime.UtcNow });

        }

        public async Task<Users?> UpdatePhoneOTPAsync(string phoneNumber, string otp)
        {
            const string sql = @"
        UPDATE Users 
        SET PhoneOTP = @OTP, 
            PhoneOTPExpiry = DATEADD(minute, 10, GETUTCDATE())
        WHERE PhoneNumber = @PhoneNumber;
        
        SELECT * FROM Users WHERE PhoneNumber = @PhoneNumber";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Users>(
                sql, new { PhoneNumber = phoneNumber, OTP = otp });
        }
        public async Task<bool> HasExceededLoginAttemptsAsync(int userId)
        {
            const string sql = @"
        SELECT CASE 
            WHEN IsLocked = 1 AND LockedUntil > GETUTCDATE() THEN 1
            WHEN LoginAttempts >= 5 AND LastLoginAttempt > DATEADD(minute, -15, GETUTCDATE()) THEN 1
            ELSE 0
        END
        FROM Users WHERE Id = @UserId";

            using var connection = new SqlConnection(_connectionString);
            var result = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
            return result == 1;
        }

        public async Task ResetLoginAttemptsAsync(int userId)
        {
            const string sql = @"
        UPDATE Users 
        SET LoginAttempts = 0, 
            LastLoginAttempt = NULL,
            IsLocked = 0,
            LockedUntil = NULL
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