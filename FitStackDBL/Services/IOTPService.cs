using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitStackDBL.Services
{
    public interface IOTPService
    {
        Task<string> GenerateOTPAsync(string identifier, OTPType type);
        Task<bool> VerifyOTPAsync(string identifier, string code, OTPType type);
        Task<bool> IsOTPValidAsync(string identifier, OTPType type);
        Task ResendOTPAsync(string identifier, OTPType type);
        Task<string> GenerateBackupCodesAsync(int userId, int count = 10);
        Task<bool> VerifyBackupCodeAsync(int userId, string backupCode);
        Task Disable2FAAsync(int userId);
        Task Enable2FAAsync(int userId);
        Task<bool> Is2FAEnabledAsync(int userId);
        Task GeneratePhoneOTPAsync(object phoneNumber);
        Task<bool> VerifyPhoneOTPAsync(object phoneNumber, object code);
    }
    public enum OTPType
    {
        PhoneVerification,
        EmailVerification,
        Login,
        PasswordReset,
        TwoFactorAuth,
        TransactionConfirmation
    }

    public class OTPResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Code { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
