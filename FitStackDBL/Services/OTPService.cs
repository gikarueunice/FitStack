using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FitStackDBL.Services
{
    public class OTPService : IOTPService
    {

        private readonly IMemoryCache _cache;
        private readonly ILogger<OTPService> _logger;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IUserService _userService;
        private readonly Random _random = new();

        public OTPService(IConfiguration configuration,
            IMemoryCache cache,
            ILogger<OTPService> logger,
            IEmailService emailService,
            ISmsService smsService,
            IUserService userService)
        {

            configuration = configuration;
            _cache = cache;
            _logger = logger;
            _emailService = emailService;
            _smsService = smsService;
            _userService = userService;
        }

        public async Task<string> GenerateOTPAsync(string identifier, OTPType type)
        {
            // Generate 6-digit OTP
            string otp = _random.Next(100000, 999999).ToString();

            // Store in cache with 10-minute expiration
            var cacheKey = GetCacheKey(identifier, type);
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            _cache.Set(cacheKey, new OTPData { Code = otp, Attempts = 0, GeneratedAt = DateTime.UtcNow }, options);

            // Send OTP based on type
            await SendOTPAsync(identifier, otp, type);

            _logger.LogInformation("OTP generated for {Identifier} of type {Type}", identifier, type);
            return otp;
        }

        public async Task<bool> VerifyOTPAsync(string identifier, string code, OTPType type)
        {
            var cacheKey = GetCacheKey(identifier, type);

            if (!_cache.TryGetValue(cacheKey, out OTPData? otpData) || otpData == null)
            {
                _logger.LogWarning("OTP not found or expired for {Identifier}", identifier);
                return false;
            }

            // Check attempts (max 3 attempts)
            if (otpData.Attempts >= 3)
            {
                _cache.Remove(cacheKey);
                _logger.LogWarning("OTP attempts exceeded for {Identifier}", identifier);
                return false;
            }

            otpData.Attempts++;
            _cache.Set(cacheKey, otpData);

            if (otpData.Code == code)
            {
                _cache.Remove(cacheKey);
                _logger.LogInformation("OTP verified successfully for {Identifier}", identifier);
                return true;
            }

            _logger.LogWarning("Invalid OTP attempt for {Identifier}", identifier);
            return false;
        }

        public async Task<bool> IsOTPValidAsync(string identifier, OTPType type)
        {
            var cacheKey = GetCacheKey(identifier, type);
            return _cache.TryGetValue(cacheKey, out _);
        }

        public async Task ResendOTPAsync(string identifier, OTPType type)
        {
            var cacheKey = GetCacheKey(identifier, type);
            _cache.Remove(cacheKey);
            await GenerateOTPAsync(identifier, type);
        }

        public async Task<string> GenerateBackupCodesAsync(int userId, int count = 10)
        {
            var codes = new List<string>();
            for (int i = 0; i < count; i++)
            {
                // Generate 8-character backup code
                var code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(6))
                    .Replace("+", "0")
                    .Replace("/", "1")
                    .Replace("=", "2")
                    .Substring(0, 8)
                    .ToUpper();
                codes.Add(code);
            }

            var backupCodesKey = $"backup_codes_{userId}";
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            };
            _cache.Set(backupCodesKey, codes, options);

            return string.Join("\n", codes);
        }

        public async Task<bool> VerifyBackupCodeAsync(int userId, string backupCode)
        {
            var backupCodesKey = $"backup_codes_{userId}";
            if (_cache.TryGetValue(backupCodesKey, out List<string>? codes) && codes != null)
            {
                if (codes.Contains(backupCode))
                {
                    codes.Remove(backupCode);
                    _cache.Set(backupCodesKey, codes);
                    return true;
                }
            }
            return false;
        }

        public async Task Enable2FAAsync(int userId)
        {
            var key = $"2fa_enabled_{userId}";
            _cache.Set(key, true, TimeSpan.FromDays(365));
        }

        public async Task Disable2FAAsync(int userId)
        {
            var key = $"2fa_enabled_{userId}";
            _cache.Remove(key);
        }

        public async Task<bool> Is2FAEnabledAsync(int userId)
        {
            var key = $"2fa_enabled_{userId}";
            return _cache.TryGetValue(key, out _);
        }

        private async Task SendOTPAsync(string identifier, string otp, OTPType type)
        {
            switch (type)
            {
                case OTPType.PhoneVerification:
                    await _smsService.SendVerificationCodeAsync(identifier, otp);
                    break;
                case OTPType.EmailVerification:
                    await _emailService.SendOTPEmailAsync(identifier, otp, "Email Verification");
                    break;
                case OTPType.Login:
                    await _smsService.SendLoginCodeAsync(identifier, otp);
                    break;
                case OTPType.PasswordReset:
                    await _emailService.SendOTPEmailAsync(identifier, otp, "Password Reset");
                    break;
                case OTPType.TwoFactorAuth:
                    await _smsService.SendTwoFactorCodeAsync(identifier, otp);
                    break;
            }
        }

        public async Task<string> GenerateAndSendPhoneOTPAsync(string phoneNumber, OTPType type)
        {
            var otp = _random.Next(100000, 999999).ToString();

            // Store in database or cache
            await _userService.UpdatePhoneOTPAsync(phoneNumber, otp);

            // Send via Twilio SMS
            bool smsSent = false;

            switch (type)
            {
                case OTPType.PhoneVerification:
                    smsSent = await _smsService.SendVerificationCodeAsync(phoneNumber, otp);
                    break;
                case OTPType.Registration:
                    smsSent = await _smsService.SendRegistrationOTPAsync(phoneNumber, otp);
                    break;
                case OTPType.Login:
                    smsSent = await _smsService.SendLoginCodeAsync(phoneNumber, otp);
                    break;
                case OTPType.TwoFactorAuth:
                    smsSent = await _smsService.SendTwoFactorCodeAsync(phoneNumber, otp);
                    break;
                case OTPType.PasswordReset:
                    smsSent = await _smsService.SendPasswordResetCodeAsync(phoneNumber, otp);
                    break;
            }

            if (!smsSent)
            {
                _logger.LogWarning("Failed to send SMS OTP to {PhoneNumber}", phoneNumber);
            }

            return otp;
        }
        private string GetCacheKey(string identifier, OTPType type)
        {
            return $"otp_{type}_{identifier}";
        }

        public async Task GeneratePhoneOTPAsync(string phoneNumber)
        {
            await GenerateOTPAsync(
                phoneNumber,
                OTPType.PhoneVerification
            );
        }

        public async Task<bool> VerifyPhoneOTPAsync(
            string phoneNumber,
            string code)
        {
            return await VerifyOTPAsync(
                phoneNumber,
                code,
                OTPType.PhoneVerification
            );
        }
    }

    public class OTPData
    {
        public string Code { get; set; } = string.Empty;
        public int Attempts { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
