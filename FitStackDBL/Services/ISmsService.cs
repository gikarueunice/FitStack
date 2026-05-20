using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitStackDBL.Services
{
    public interface ISmsService
    {
         Task<bool> SendSmsAsync(string phoneNumber, string message);
        Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
        Task<bool> SendRegistrationOTPAsync(string phoneNumber, string otp);
        Task<bool> SendLoginCodeAsync(string phoneNumber, string code);
        Task<bool> SendTwoFactorCodeAsync(string phoneNumber, string code);
        Task<bool> SendWelcomeSmsAsync(string phoneNumber, string userName);
        Task<bool> SendWorkoutReminderAsync(string phoneNumber, string workoutName, DateTime time);
        Task<bool> SendPasswordResetCodeAsync(string phoneNumber, string code);

    }
}
