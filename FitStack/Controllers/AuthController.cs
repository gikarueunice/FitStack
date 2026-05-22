using FitStackDBL;
using FitStackDBL.Services;
using FitStackDBL.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using FitStack.ViewModels.Auth;
using BCrypt.Net;

namespace FitStack.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IOTPService _otpService;
        private readonly ISmsService _smsService;
        private readonly ILogger<AuthController> _logger;
        private static Dictionary<int, string> _pendingRegistrations = new();
        private static Dictionary<string, string> _pendingOtps = new();

        public AuthController(
            IUserService userService,
            IEmailService emailService,
            IOTPService otpService,
            ISmsService smsService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _emailService = emailService;
            _otpService = otpService;
            _smsService = smsService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            var model = new RegisterViewModel();
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            try
            {
                // Check if email exists
                var existingUser = await _userService.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Email already registered" });
                }

                // Hash password
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var user = new Users
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber ?? string.Empty,
                    PasswordHash = passwordHash,
                    Salt = string.Empty,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Height = model.Height,
                    Weight = model.Weight,
                    FitnessGoal = model.FitnessGoal,
                    ActivityLevel = model.ActivityLevel,
                    SelectedPlan = model.SelectedPlan,
                    SubscribeToNewsletter = model.SubscribeToNewsletter,
                    IsEmailVerified = false,
                    EmailVerificationToken = Guid.NewGuid().ToString(),
                    IsPhoneVerified = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // ONLY CREATE USER ONCE!
                var userId = await _userService.CreateUserAsync(user);

                // Generate OTP (6 digits)
                var otp = new Random().Next(100000, 999999).ToString();

                // DEBUG: Log the OTP
                _logger.LogInformation($"=== OTP GENERATED ===");
                _logger.LogInformation($"User ID: {userId}");
                _logger.LogInformation($"Generated OTP: {otp}");
                _logger.LogInformation($"Email: {user.Email}");
                _logger.LogInformation($"===================");

                // Also output to console for immediate visibility
                Console.WriteLine($"\n\n*** VERIFICATION CODE FOR USER {userId}: {otp} ***\n\n");

                // Store OTP temporarily
                _pendingRegistrations[userId] = otp;

                // DEBUG: Verify it was stored
                _logger.LogInformation($"OTP stored successfully. Pending count: {_pendingRegistrations.Count}");
                _logger.LogInformation($"Stored OTP for ID {userId}: {_pendingRegistrations[userId]}");

                // Send OTP via email
                await _emailService.SendRegistrationOTPAsync(user.Email, otp, user.FullName);

                // Send OTP via SMS if phone provided
                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    await _smsService.SendRegistrationOTPAsync(model.PhoneNumber, otp);
                }

                string target = model.Email;
                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    target += $" or {model.PhoneNumber}";
                }

                TempData["VerificationEmail"] = user.Email;
                TempData["VerificationPhone"] = user.PhoneNumber;
                TempData["VerificationUserId"] = userId;

                return RedirectToAction("VerifyRegistration", new { userId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return Json(new { success = false, message = "Registration failed. Please try again." });
            }
        }
        [HttpGet]
        [Route("auth/VerifyRegistrationOTP")]
        //[Route("auth/verify/{userId?}")]
        //[Route("auth/verify-registration/{userId?}")]
        //[Route("auth/verify-otp/{userId?}")]
        [Route("auth/verify-registration-otp")]
        public async Task<IActionResult> VerifyRegistration(int? userId)
        {
            // Try to get userId from multiple sources
            int finalUserId = 0;

            // 1. From route parameter
            if (userId.HasValue && userId.Value > 0)
            {
                finalUserId = userId.Value;
            }

            // 2. From query string
            if (finalUserId == 0 && Request.Query.ContainsKey("userId"))
            {
                int.TryParse(Request.Query["userId"], out finalUserId);
            }

            // 3. From TempData (in case of redirect)
            if (finalUserId == 0 && TempData["VerificationUserId"] != null)
            {
                finalUserId = Convert.ToInt32(TempData["VerificationUserId"]);
            }

            // 4. From session (if you're using session)
            if (finalUserId == 0 && HttpContext.Session.GetInt32("PendingUserId") != null)
            {
                finalUserId = HttpContext.Session.GetInt32("PendingUserId").Value;
            }

            // If still no userId, show error
            if (finalUserId <= 0)
            {
                TempData["Error"] = "Invalid verification request. Please register again.";
                return RedirectToAction("Register");
            }

            var user = await _userService.GetUserByIdAsync(finalUserId);

            if (user == null)
            {
                TempData["Error"] = "User not found. Please register again.";
                return RedirectToAction("Register");
            }

            // Check if already verified
            if (user.IsEmailVerified)
            {
                TempData["Success"] = "Your account is already verified! Please login.";
                return RedirectToAction("Login");
            }

            var model = new VerifyRegistrationViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            // Store in TempData for backup
            TempData["VerificationUserId"] = user.Id;

            return View("VerifyRegistrationPage", model);
        }

        // Alternative: Also accept userId from query string
        [HttpGet]
        public async Task<IActionResult> Verify(int? userId)
        {
            if (userId == null || userId <= 0)
            {
                // Try to get from TempData
                if (TempData["VerificationUserId"] != null)
                {
                    userId = Convert.ToInt32(TempData["VerificationUserId"]);
                }
                else
                {
                    TempData["Error"] = "Invalid verification request.";
                    return RedirectToAction("Register");
                }
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Register");
            }

            var model = new VerifyRegistrationViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            return View("VerifyRegistrationPage", model);
        }

        [HttpPost]
        [Route("auth/VerifyRegistrationOTP")]
        [Route("auth/verify-registration-otp")]
        public async Task<IActionResult> VerifyRegistrationOTP([FromBody] VerifyOTPRequest request)
        {
            try
            {
                Console.WriteLine($"=== OTP VERIFICATION RECEIVED ===");
                Console.WriteLine($"UserId: {request?.UserId}");
                Console.WriteLine($"Code: {request?.Code}");

                if (request == null)
                {
                    return Json(new { success = false, message = "Invalid request" });
                }

                // Check if pending registrations contains the userId
                if (!_pendingRegistrations.ContainsKey(request.UserId))
                {
                    Console.WriteLine($"UserId {request.UserId} not found in pending registrations");
                    return Json(new { success = false, message = "Invalid or expired verification code" });
                }

                var storedOtp = _pendingRegistrations[request.UserId];
                Console.WriteLine($"Stored OTP: {storedOtp}, Entered: {request.Code}");

                if (storedOtp != request.Code)
                {
                    Console.WriteLine($"OTP mismatch!");
                    return Json(new { success = false, message = "Invalid verification code" });
                }

                // Get user and mark as verified
                var user = await _userService.GetUserByIdAsync(request.UserId);
                if (user != null)
                {
                    user.IsEmailVerified = true;
                    if (!string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        user.IsPhoneVerified = true;
                    }
                    await _userService.UpdateUserAsync(user);

                    // Remove from pending
                    _pendingRegistrations.Remove(request.UserId);

                    // Sign in the user
                    await SignInUserAsync(user, false);

                    return Json(new
                    {
                        success = true,
                        message = "Account verified successfully!",
                        redirect = Url.Action("Index", "Dashboard")
                    });
                }

                return Json(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                _logger.LogError(ex, "OTP verification error");
                return Json(new { success = false, message = "Verification failed: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResendRegistrationOTP([FromBody] ResendOTPRequest request)
        {
            try
            {
                if (!_pendingRegistrations.ContainsKey(request.UserId))
                {
                    return Json(new { success = false, message = "Invalid request" });
                }

                var user = await _userService.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Generate new OTP
                var newOtp = new Random().Next(100000, 999999).ToString();
                _pendingRegistrations[request.UserId] = newOtp;

                // Resend OTP
                await _emailService.SendRegistrationOTPAsync(user.Email, newOtp, user.FullName);

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    await _smsService.SendRegistrationOTPAsync(user.PhoneNumber, newOtp);
                }

                return Json(new { success = true, message = "New verification code sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resend OTP error");
                return Json(new { success = false, message = "Failed to resend code" });
            }
        }



        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            // Clear any existing messages
            TempData.Remove("Error");
            TempData.Remove("Success");
            TempData.Remove("Warning");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Email))
                    TempData["Error"] = "Please enter your email address.";
                else if (string.IsNullOrEmpty(model.Password))
                    TempData["Error"] = "Please enter your password.";
                else
                    TempData["Error"] = "Please correct the errors in the form.";

                return View(model);
            }

            try
            {
                var user = await _userService.GetUserByEmailAsync(model.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", model.Email);
                    TempData["Error"] = "No account found with this email address. Please check your email or <a href='/auth/register'>create a new account</a>.";
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    TempData["Error"] = "Your account has been deactivated. Please contact support for assistance.";
                    return View(model);
                }

                if (!user.IsEmailVerified)
                {
                    var otp = new Random().Next(100000, 999999).ToString();

                    _pendingRegistrations[user.Id] = otp;

                    await _emailService.SendRegistrationOTPAsync(
                        user.Email,
                        otp,
                        user.FullName);

                    TempData["Warning"] =
                    "Your account isn't verified yet. A new OTP code has been sent to your email.";

                    return RedirectToAction(
                        "VerifyRegistration",
                        new { userId = user.Id }
                        );
                }

                // Verify password
                // Verify password
                bool isPasswordValid;

                try
                {
                    if (!string.IsNullOrEmpty(user.Salt))
                    {
                        // old users
                        var oldHash = BCrypt.Net.BCrypt.HashPassword(
                            model.Password,
                            user.Salt
                        );

                        isPasswordValid = oldHash == user.PasswordHash;
                    }
                    else
                    {
                        // new users
                        isPasswordValid = BCrypt.Net.BCrypt.Verify(
                            model.Password,
                            user.PasswordHash
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Password verification error for {Email}",
                        model.Email);

                    TempData["Error"] = "Password verification error";
                    return View(model);
                }

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Failed login attempt for {Email} - Incorrect password", model.Email);
                    TempData["Error"] = "Incorrect password. Please try again. <a href='/auth/forgot-password'>Forgot your password?</a>";
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                // Check for multiple failed attempts (you can implement this in your user service)
                if (await _userService.HasExceededLoginAttemptsAsync(user.Id))
                {
                    TempData["Error"] = "Too many failed login attempts. Your account has been temporarily locked. Please try again in 15 minutes or reset your password.";
                    return View(model);
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user);

                // Clear any failed login attempts
                await _userService.ResetLoginAttemptsAsync(user.Id);

                // Sign in the user
                await SignInUserAsync(user, model.RememberMe);

                TempData["Success"] = $"Welcome back, {user.FullName}! 👋";

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Dashboard");
            }
            catch (UserLockedException ex)
            {
                _logger.LogWarning(ex, "Locked account login attempt for {Email}", model.Email);
                TempData["Error"] = "Your account has been locked due to multiple failed attempts. Please reset your password or contact support.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error during login for {Email}",
                    model.Email);

                TempData["Error"] =
                    ex.Message;

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResendVerification(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Email address is required.";
                return RedirectToAction("Login");
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null || user.IsEmailVerified)
            {
                TempData["Error"] = "Unable to resend verification. Please check your email or register a new account.";
                return RedirectToAction("Login");
            }

            try
            {
                var verificationLink = Url.Action(nameof(VerifyEmail), "Auth",
                    new { token = user.EmailVerificationToken }, Request.Scheme);
                await _emailService.SendVerificationEmailAsync(user.Email, verificationLink, user.FullName);
                TempData["Success"] = "Verification email has been resent. Please check your inbox (and spam folder).";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend verification email");
                TempData["Error"] = "Unable to send verification email at this time. Please try again later.";
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Invalid verification token.";
                return RedirectToAction("Login");
            }

            var user = await _userService.GetUserByVerificationTokenAsync(token);
            if (user == null)
            {
                TempData["Error"] = "Invalid or expired verification token. Please request a new verification email.";
                return RedirectToAction("Login");
            }

            if (user.IsEmailVerified)
            {
                TempData["Success"] = "Your email is already verified. You can now login.";
                return RedirectToAction("Login");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            await _userService.UpdateUserAsync(user);

            TempData["Success"] = "Email verified successfully! 🎉 You can now login and start your fitness journey.";
            return RedirectToAction("Login");
        }
        [HttpGet]


        [HttpGet]
        public IActionResult VerifyPhone(int phoneNumber)
        {
            var model = new PhoneVerificationViewModel { PhoneNumber = phoneNumber };
            return View(model);
        }

        //[HttpPost]
        //public async Task<IActionResult> SendPhoneOTP([FromBody] PhoneOTPRequest request)
        //{
        //    try
        //    {
        //        await _otpService.GeneratePhoneOTPAsync(request.PhoneNumber);
        //        return Json(new { success = true, message = "Verification code sent" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to send phone OTP");
        //        return Json(new { success = false, message = "Failed to send code" });
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> VerifyPhoneOTP([FromBody] PhoneOTPVerifyRequest request)
        {
            try
            {
                var isValid = await _otpService.VerifyPhoneOTPAsync(request.PhoneNumber, request.Code);

                if (isValid)
                {
                    return Json(new { success = true, message = "Phone verified successfully" });
                }

                return Json(new { success = false, message = "Invalid or expired code" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify phone OTP");
                return Json(new { success = false, message = "Verification failed" });
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Email))
                    TempData["Error"] = "Please enter your email address.";
                return View(model);
            }

            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (user != null && user.IsEmailVerified)
            {
                try
                {
                    // Generate password reset token
                    user.PasswordResetToken = Guid.NewGuid().ToString();
                    user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
                    await _userService.UpdateUserAsync(user);

                    // Send reset email
                    var resetLink = Url.Action(nameof(ResetPassword), "Auth",
                        new { token = user.PasswordResetToken }, Request.Scheme);
                    await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink, user.FullName);

                    TempData["Success"] = "Password reset instructions have been sent to your email address. The link will expire in 24 hours.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send password reset email");
                    TempData["Error"] = "Unable to send password reset email at this time. Please try again later.";
                }
            }
            else if (user != null && !user.IsEmailVerified)
            {
                TempData["Warning"] = "Your email is not verified. Please verify your email first before resetting your password. <a href='/auth/resend-verification?email=" + model.Email + "'>Resend verification email</a>";
            }
            else
            {
                // Don't reveal that email doesn't exist for security
                TempData["Info"] = "If an account exists with this email and is verified, you will receive password reset instructions.";
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Invalid password reset token.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                TempData["Error"] = string.Join(" ", errors);
                return View(model);
            }

            var user = await _userService.GetUserByPasswordResetTokenAsync(model.Token);
            if (user == null)
            {
                TempData["Error"] = "Invalid or expired password reset token. Please request a new password reset.";
                return RedirectToAction("ForgotPassword");
            }

            if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                TempData["Error"] = "Password reset link has expired. Please request a new one.";
                return RedirectToAction("ForgotPassword");
            }

            // Validate password strength
            if (!IsPasswordStrong(model.Password))
            {
                TempData["Error"] = "Password does not meet security requirements. It must be at least 8 characters and contain uppercase, lowercase, number, and special character.";
                return View(model);
            }

            try
            {
                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                user.Salt = string.Empty;
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                await _userService.UpdateUserAsync(user);

                TempData["Success"] = "Password reset successfully! You can now login with your new password.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {Email}", user.Email);
                TempData["Error"] = "An error occurred while resetting your password. Please try again.";
                return View(model);
            }
        }

        private bool IsPasswordStrong(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => "@$!%*?&".Contains(ch));
        }

        private async Task SignInUserAsync(Users user, bool isPersistent = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName),
                new Claim("EmailVerified", user.IsEmailVerified.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private async Task SendVerificationEmailAsync(string email, string token)
        {
            var verificationLink = Url.Action(nameof(VerifyEmail), "Account", new { token }, Request.Scheme);
            // Implement email sending logic here
            await _emailService.SendEmailAsync(email, "Verify Your Email",
                $"Please verify your email by clicking <a href='{verificationLink}'>here</a>");
        }

        private async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var resetLink = Url.Action(nameof(ResetPassword), "Account", new { token }, Request.Scheme);
            // Implement email sending logic here
            await _emailService.SendEmailAsync(email, "Reset Your Password",
                $"Please reset your password by clicking <a href='{resetLink}'>here</a>");
        }
        [HttpPost]
        public async Task<IActionResult> SendPhoneOTP([FromBody] PhoneVerificationRequest request)
        {
            try
            {
                var user = await _userService.GetUserByPhoneNumberAsync(request.PhoneNumber);
                if (user != null && user.IsPhoneVerified)
                {
                    return Json(new { success = false, message = "Phone number already verified" });
                }

                await _otpService.GenerateOTPAsync(request.PhoneNumber, OTPType.PhoneVerification);
                return Json(new { success = true, message = "OTP sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending phone OTP");
                return Json(new { success = false, message = "Failed to send OTP" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyPhoneOTP([FromBody] PhoneVerificationRequest request)
        {
            try
            {
                var isValid = await _otpService.VerifyOTPAsync(request.PhoneNumber, request.Code, OTPType.PhoneVerification);

                if (isValid)
                {
                    var user = await _userService.GetUserByPhoneNumberAsync(request.PhoneNumber);
                    if (user != null)
                    {
                        user.IsPhoneVerified = true;
                        await _userService.UpdateUserAsync(user);
                    }
                    return Json(new { success = true, message = "Phone verified successfully" });
                }

                return Json(new { success = false, message = "Invalid or expired OTP" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying phone OTP");
                return Json(new { success = false, message = "Failed to verify OTP" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendLoginOTP([FromBody] LoginOTPRequest request)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(request.Identifier);
                if (user == null)
                {
                    user = await _userService.GetUserByPhoneNumberAsync(request.Identifier);
                }

                if (user == null)
                {
                    return Json(new { success = false, message = "No account found" });
                }

                // Send OTP via email or SMS based on identifier type
                if (request.Identifier.Contains("@"))
                {
                    await _otpService.GenerateOTPAsync(request.Identifier, OTPType.Login);
                }
                else
                {
                    await _otpService.GenerateOTPAsync(request.Identifier, OTPType.Login);
                }

                return Json(new { success = true, message = "Login code sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending login OTP");
                return Json(new { success = false, message = "Failed to send login code" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyLoginOTP([FromBody] LoginOTPVerifyRequest request)
        {
            try
            {
                var isValid = await _otpService.VerifyOTPAsync(request.Identifier, request.Code, OTPType.Login);

                if (isValid)
                {
                    var user = await _userService.GetUserByEmailAsync(request.Identifier);
                    if (user == null)
                    {
                        user = await _userService.GetUserByPhoneNumberAsync(request.Identifier);
                    }

                    if (user != null)
                    {
                        await SignInUserAsync(user, false);
                        return Json(new { success = true, redirect = "/dashboard" });
                    }
                }

                return Json(new { success = false, message = "Invalid or expired code" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying login OTP");
                return Json(new { success = false, message = "Failed to verify code" });
            }
        }
        [HttpGet]
        [Route("auth/test-api")]
        public IActionResult TestApi()
        {
            return Json(new { success = true, message = "API is working!" });
        }

        public class PhoneVerificationRequest
        {
            public string PhoneNumber { get; set; } = string.Empty;
            public string? Code { get; set; }
        }

        public class LoginOTPRequest
        {
            public string Identifier { get; set; } = string.Empty;
        }

        public class LoginOTPVerifyRequest
        {
            public string Identifier { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }

        //[HttpGet]
        //public IActionResult VerifyRegistration(int userId)
        //{
        //    // Get user info from TempData or query string
        //    var email = TempData["VerificationEmail"]?.ToString() ?? string.Empty;
        //    var phone = TempData["VerificationPhone"]?.ToString();

        //    // If not in TempData, try to get from database
        //    if (string.IsNullOrEmpty(email))
        //    {
        //        // You might want to fetch from database here
        //        // For now, just use the userId from query string
        //    }

        //    var model = new VerifyRegistrationViewModel
        //    {
        //        UserId = userId,
        //        Email = email,
        //        PhoneNumber = phone
        //    };

        //    return View(model);
        //}

    }

    [Serializable]
    internal class UserLockedException : Exception
    {
        public UserLockedException()
        {
        }

        public UserLockedException(string? message) : base(message)
        {
        }

        public UserLockedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    [Serializable]
    internal class DuplicateEmailException : Exception
    {
        public DuplicateEmailException()
        {
        }

        public DuplicateEmailException(string? message) : base(message)
        {
        }

        public DuplicateEmailException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
