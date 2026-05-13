using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitStackDBL.Services
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendVerificationCodeAsync(string phoneNumber, string code);
        Task SendLoginCodeAsync(string phoneNumber, string code);
        Task SendTwoFactorCodeAsync(string phoneNumber, string code);
        Task SendWelcomeSmsAsync(string phoneNumber, string userName);
        Task SendWorkoutReminderAsync(string phoneNumber, string workoutName, DateTime time);
    }
}
