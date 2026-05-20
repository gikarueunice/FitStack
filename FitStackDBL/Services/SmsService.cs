using FitStackDBL.Model;
using FitStackDBL.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace FitStack.Services
{
    public class SmsService : ISmsService
    {
        private readonly TwilioSettings _twilioSettings;
        private readonly ILogger<SmsService> _logger;
        private readonly bool _isTestMode;

        public SmsService(IOptions<TwilioSettings> twilioSettings, ILogger<SmsService> logger)
        {
            _twilioSettings = twilioSettings.Value;
            _logger = logger;

            // Check if we're in test mode (no valid credentials)
            _isTestMode = string.IsNullOrEmpty(_twilioSettings.AccountSid) ||
                          _twilioSettings.AccountSid == "ACf8b667a139aad03c436b2fe084372e33";

            if (!_isTestMode)
            {
                // Initialize Twilio client
                TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
                _logger.LogInformation("Twilio client initialized successfully");
            }
            else
            {
                _logger.LogWarning("Twilio is running in test mode. No actual SMS will be sent.");
            }
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                _logger.LogWarning("Cannot send SMS: Phone number is empty");
                return false;
            }

            // Format phone number (ensure it has country code)
            var formattedNumber = FormatPhoneNumber(phoneNumber);

            try
            {
                if (_isTestMode)
                {
                    // Test mode - just log the message
                    _logger.LogInformation("[TEST MODE] SMS to {PhoneNumber}: {Message}", formattedNumber, message);
                    return true;
                }

                // Send actual SMS using Twilio
                var messageResource = await MessageResource.CreateAsync(
                    to: new PhoneNumber(formattedNumber),
                    from: string.IsNullOrEmpty(_twilioSettings.MessagingServiceSid)
                        ? new PhoneNumber(_twilioSettings.FromPhoneNumber)
                        : null,
                    messagingServiceSid: _twilioSettings.MessagingServiceSid,
                    body: message
                );

                _logger.LogInformation("SMS sent successfully to {PhoneNumber}. SID: {Sid}", formattedNumber, messageResource.Sid);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", formattedNumber);
                return false;
            }
        }

        public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
        {
            var message = $"🔐 Your FitTrack verification code is: {code}. Valid for 10 minutes. Do not share this code with anyone.";
            return await SendSmsAsync(phoneNumber, message);
        }

        public async Task<bool> SendRegistrationOTPAsync(string phoneNumber, string otp)
        {
            var message = $"🎉 Welcome to FitTrack! Your registration verification code is: {otp}. Enter this code to complete your registration. Valid for 10 minutes.";
            return await SendSmsAsync(phoneNumber, message);
        }

        public async Task<bool> SendLoginCodeAsync(string phoneNumber, string code)
        {
            var message = $"🔑 Your FitTrack login code is: {code}. Valid for 10 minutes. If you didn't request this, please ignore this message.";
            return await SendSmsAsync(phoneNumber, message);
        }

        public async Task<bool> SendTwoFactorCodeAsync(string phoneNumber, string code)
        {
            var message = $"🔒 Your FitTrack two-factor authentication code is: {code}. Valid for 10 minutes. Never share this code with anyone.";
            return await SendSmsAsync(phoneNumber, message);
        }

        public async Task<bool> SendWelcomeSmsAsync(string phoneNumber, string userName)
        {
            var message = $"🎉 Welcome to FitTrack, {userName}! We're excited to help you achieve your fitness goals. Log in to start your journey: https://yourdomain.com/login";
            return await SendSmsAsync(phoneNumber, message);
        }

        public async Task<bool> SendWorkoutReminderAsync(string phoneNumber, string workoutName, DateTime time)
        {
            var message = $"💪 Reminder: Your {workoutName} workout is scheduled for {time:hh:mm tt} today. Get ready to crush your goals!";
            return await SendSmsAsync(phoneNumber, message);
        }

        public async Task<bool> SendPasswordResetCodeAsync(string phoneNumber, string code)
        {
            var message = $"🔐 Your FitTrack password reset code is: {code}. Valid for 10 minutes. If you didn't request this, please ignore.";
            return await SendSmsAsync(phoneNumber, message);
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove any non-digit characters
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Ensure the number starts with '+' for E.164 format
            if (!phoneNumber.StartsWith("+") && digits.Length >= 10)
            {
                // Check if it's a US number (starts with 1)
                if (digits.Length == 11 && digits.StartsWith("1"))
                {
                    return "+" + digits;
                }
                // Add default country code for US if not present (you can make this configurable)
                else if (digits.Length == 10)
                {
                    return "+1" + digits;
                }
                return "+" + digits;
            }

            return phoneNumber;
        }
    }
}