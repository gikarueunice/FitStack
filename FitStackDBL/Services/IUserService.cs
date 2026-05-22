using FitStackDBL.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FitStackDBL.Services
{
    public interface IUserService
    {
        Task<Users?> GetUserByIdAsync(int id);
        Task<Users?> GetUserByEmailAsync(string email);
        Task<Users?> GetUserByPhoneNumberAsync(string phoneNumber);
        Task<Users?> GetUserByVerificationTokenAsync(string token);
        Task<Users?> GetUserByPasswordResetTokenAsync(string token);
        Task<int> CreateUserAsync(Users user);
        Task UpdateUserAsync(Users user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> HasExceededLoginAttemptsAsync(int userId);
        Task ResetLoginAttemptsAsync(int userId);
        Task IncrementLoginAttemptsAsync(int userId);
        Task<Users> UpdatePhoneOTPAsync(string phoneNumber, string otp);
        
    }
}
