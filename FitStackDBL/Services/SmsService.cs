using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitStackDBL.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly IConfiguration _configuration;

        public SmsService(ILogger<SmsService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            // For development, just log the SMS
            _logger.LogInformation("SMS to {PhoneNumber}: {Message}", phoneNumber, message);

            // In production, integrate with SMS provider like Twilio, Vonage, etc.
            // Example with Twilio:
            /*
            TwilioClient.Init(_configuration["Twilio:AccountSid"], _configuration["Twilio:AuthToken"]);
            var message = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_configuration["Twilio:PhoneNumber"]),
                to: new PhoneNumber(phoneNumber)
            );
            */

            await Task.CompletedTask;
        }

        public async Task SendVerificationCodeAsync(string phoneNumber, string code)
        {
            var message = $"🔐 Your FitTrack verification code is: {code}. Valid for 10 minutes. Do not share this code with anyone.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendLoginCodeAsync(string phoneNumber, string code)
        {
            var message = $"🔑 Your FitTrack login code is: {code}. Valid for 10 minutes. If you didn't request this, please ignore.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendTwoFactorCodeAsync(string phoneNumber, string code)
        {
            var message = $"🔒 Your FitTrack 2FA code is: {code}. Valid for 10 minutes.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendWelcomeSmsAsync(string phoneNumber, string userName)
        {
            var message = $"🎉 Welcome to FitTrack, {userName}! Start your fitness journey today. Download our app for the best experience!";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendWorkoutReminderAsync(string phoneNumber, string workoutName, DateTime time)
        {
            var message = $"💪 Reminder: Your {workoutName} workout is scheduled for {time:hh:mm tt}. Get ready to crush your goals!";
            await SendSmsAsync(phoneNumber, message);
        }
    }
}
