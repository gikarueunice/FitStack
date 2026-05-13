using FitStackDBL.Model;
using FitStackDBL.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;


namespace FitStackDBL.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
      

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
                  {
            _emailSettings = emailSettings.Value;
            _logger = logger;
  
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Validate email settings
                if (string.IsNullOrEmpty(_emailSettings.SmtpServer))
                    throw new InvalidOperationException("SMTP server is not configured");

                if (string.IsNullOrEmpty(_emailSettings.SenderEmail))
                    throw new InvalidOperationException("Sender email is not configured");

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = _emailSettings.UseDefaultCredentials,
                    Credentials = string.IsNullOrEmpty(_emailSettings.Username)
                        ? null
                        : new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string to, string verificationLink, string userName)
        {
            var subject = "Verify Your Email Address - FitTrack";
            var body = GetVerificationEmailTemplate(userName, verificationLink);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink, string userName)
        {
            var subject = "Reset Your Password - FitTrack";
            var body = GetPasswordResetEmailTemplate(userName, resetLink);
            await SendEmailAsync(to, subject, body);
        }
        public Task SendOTPEmailAsync(string to, string otp, string purpose)
        {
            var subject = $"Your {purpose} OTP - FitTrack";
            var body = $@"
            <!DOCTYPE html>
            <html>
            <head><meta charset='UTF-8'></head>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4F46E5;'>Your OTP Code</h2>
                    <p>Your One-Time Password (OTP) for <strong>{purpose}</strong> is:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <div style='font-size: 32px; font-weight: bold; letter-spacing: 5px; background: #F3F4F6; padding: 20px; border-radius: 10px;'>
                            {otp}
                        </div>
                    </div>
                    <p>This code is valid for <strong>10 minutes</strong>.</p>
                    <p style='color: #EF4444;'>⚠️ Never share this code with anyone, including FitTrack support.</p>
                </div>
            </body>
            </html>";
            return SendEmailAsync(to, subject, body);
        }
        public Task Send2FADisableEmailAsync(string to, string userName)
        {
            var subject = "Two-Factor Authentication Disabled - FitTrack";
            var body = $@"
            <!DOCTYPE html>
            <html>
            <head><meta charset='UTF-8'></head>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4F46E5;'>2FA Disabled</h2>
                    <p>Hi <strong>{userName}</strong>,</p>
                    <p>Two-Factor Authentication has been disabled on your account.</p>
                    <p>If you didn't make this change, please contact support immediately.</p>
                </div>
            </body>
            </html>";
            return SendEmailAsync(to, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string to, string userName)
        {
            var subject = "Welcome to FitTrack! 🎉";
            var body = GetWelcomeEmailTemplate(userName);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendWorkoutReminderAsync(string to, string userName, string workoutType, DateTime scheduledTime)
        {
            var subject = $"Reminder: {workoutType} Workout Today! 💪";
            var body = GetWorkoutReminderTemplate(userName, workoutType, scheduledTime);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendWeeklyProgressReportAsync(string to, string userName, int workoutsCompleted, int totalCalories, int streakDays)
        {
            var subject = "Your Weekly Progress Report - FitTrack 📊";
            var body = GetWeeklyReportTemplate(userName, workoutsCompleted, totalCalories, streakDays);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendGoalAchievedEmailAsync(string to, string userName, string goalName)
        {
            var subject = $"🎉 Congratulations! You've Achieved Your Goal: {goalName}";
            var body = GetGoalAchievedTemplate(userName, goalName);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendNewsletterEmailAsync(string to, string userName, List<NewsletterArticle> articles)
        {
            var subject = "FitTrack Weekly Newsletter - Tips & Insights 📰";
            var body = GetNewsletterTemplate(userName, articles);
            await SendEmailAsync(to, subject, body);
        }

        #region Email Templates

        private string GetVerificationEmailTemplate(string userName, string verificationLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Verify Your Email</title>
                    <style>
                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                            line-height: 1.6;
                            color: #1F2937;
                            background-color: #F3F4F6;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background: #FFFFFF;
                            border-radius: 24px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
                            padding: 40px 30px;
                            text-align: center;
                        }}
                        .logo {{
                            display: inline-flex;
                            align-items: center;
                            gap: 10px;
                            color: white;
                            font-size: 28px;
                            font-weight: bold;
                            text-decoration: none;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 14px 32px;
                            background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 12px;
                            font-weight: 600;
                            margin: 20px 0;
                        }}
                        .alert {{
                            background: #FEF3C7;
                            border-left: 4px solid #F59E0B;
                            padding: 16px;
                            margin: 20px 0;
                            border-radius: 8px;
                        }}
                        .footer {{
                            padding: 30px;
                            text-align: center;
                            background: #F9FAFB;
                            border-top: 1px solid #E5E7EB;
                            color: #6B7280;
                            font-size: 14px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>
                                <svg width='32' height='32' viewBox='0 0 32 32' fill='none'>
                                    <path d='M16 2L4 8V16C4 23.5 9.5 30.5 16 32C22.5 30.5 28 23.5 28 16V8L16 2Z' fill='white' stroke='white' stroke-width='2'/>
                                    <path d='M12 16L15 19L21 13' stroke='#4F46E5' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round'/>
                                </svg>
                                <span>FitTrack</span>
                            </div>
                        </div>
                        <div class='content'>
                            <h1>Verify Your Email Address</h1>
                            <p>Hi <strong>{userName}</strong>,</p>
                            <p>Thanks for joining FitTrack! We're excited to help you on your fitness journey. To get started, please verify your email address by clicking the button below:</p>
                            <div style='text-align: center;'>
                                <a href='{verificationLink}' class='button'>Verify Email Address</a>
                            </div>
                            <div class='alert'>
                                <strong>🔒 Security Tip:</strong> This verification link will expire in 24 hours. If you didn't create this account, please ignore this email.
                            </div>
                            <p>If the button doesn't work, copy and paste this link into your browser:</p>
                            <p style='background: #F3F4F6; padding: 12px; border-radius: 8px; word-break: break-all;'>{verificationLink}</p>
                        </div>
                        <div class='footer'>
                            <p>© 2026 FitTrack. All rights reserved.</p>
                            <p>Made with ❤️ to help you achieve your fitness goals</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetPasswordResetEmailTemplate(string userName, string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <title>Reset Your Password</title>
                    <style>
                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                            line-height: 1.6;
                            color: #1F2937;
                            background-color: #F3F4F6;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background: #FFFFFF;
                            border-radius: 24px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
                            padding: 40px 30px;
                            text-align: center;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 14px 32px;
                            background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 12px;
                            font-weight: 600;
                            margin: 20px 0;
                        }}
                        .warning {{
                            background: #FEF2F2;
                            border-left: 4px solid #EF4444;
                            padding: 16px;
                            margin: 20px 0;
                            border-radius: 8px;
                        }}
                        .footer {{
                            padding: 30px;
                            text-align: center;
                            background: #F9FAFB;
                            border-top: 1px solid #E5E7EB;
                            color: #6B7280;
                            font-size: 14px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>
                                <svg width='32' height='32' viewBox='0 0 32 32' fill='none'>
                                    <path d='M16 2L4 8V16C4 23.5 9.5 30.5 16 32C22.5 30.5 28 23.5 28 16V8L16 2Z' fill='white' stroke='white' stroke-width='2'/>
                                    <path d='M12 16L15 19L21 13' stroke='#4F46E5' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round'/>
                                </svg>
                                <span>FitTrack</span>
                            </div>
                        </div>
                        <div class='content'>
                            <h1>Reset Your Password</h1>
                            <p>Hi <strong>{userName}</strong>,</p>
                            <p>We received a request to reset your password. Click the button below to create a new password:</p>
                            <div style='text-align: center;'>
                                <a href='{resetLink}' class='button'>Reset Password</a>
                            </div>
                            <div class='warning'>
                                <strong>⚠️ Important:</strong> This password reset link will expire in 24 hours. If you didn't request this, please ignore this email and your password will remain unchanged.
                            </div>
                            <p>If the button doesn't work, copy and paste this link into your browser:</p>
                            <p style='background: #F3F4F6; padding: 12px; border-radius: 8px; word-break: break-all;'>{resetLink}</p>
                        </div>
                        <div class='footer'>
                            <p>© 2026 FitTrack. All rights reserved.</p>
                            <p>Need help? Contact our support team at support@fittrack.com</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetWelcomeEmailTemplate(string userName)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <title>Welcome to FitTrack!</title>
                    <style>
                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                            line-height: 1.6;
                            color: #1F2937;
                            background-color: #F3F4F6;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background: #FFFFFF;
                            border-radius: 24px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
                            padding: 40px 30px;
                            text-align: center;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .features {{
                            display: grid;
                            grid-template-columns: repeat(3, 1fr);
                            gap: 20px;
                            margin: 30px 0;
                        }}
                        .feature {{
                            text-align: center;
                        }}
                        .feature-icon {{
                            font-size: 32px;
                            margin-bottom: 10px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 14px 32px;
                            background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 12px;
                            font-weight: 600;
                            margin: 20px 0;
                        }}
                        .footer {{
                            padding: 30px;
                            text-align: center;
                            background: #F9FAFB;
                            border-top: 1px solid #E5E7EB;
                            color: #6B7280;
                            font-size: 14px;
                        }}
                        @media (max-width: 480px) {{
                            .features {{
                                grid-template-columns: 1fr;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>
                                <svg width='32' height='32' viewBox='0 0 32 32' fill='none'>
                                    <path d='M16 2L4 8V16C4 23.5 9.5 30.5 16 32C22.5 30.5 28 23.5 28 16V8L16 2Z' fill='white' stroke='white' stroke-width='2'/>
                                    <path d='M12 16L15 19L21 13' stroke='#4F46E5' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round'/>
                                </svg>
                                <span>FitTrack</span>
                            </div>
                        </div>
                        <div class='content'>
                            <h1>Welcome to FitTrack, {userName}! 🎉</h1>
                            <p>We're thrilled to have you join our fitness community. Get ready to transform your health and achieve your goals!</p>
                            
                            <div class='features'>
                                <div class='feature'>
                                    <div class='feature-icon'>📊</div>
                                    <h3>Track Progress</h3>
                                    <p>Log workouts and watch your improvement</p>
                                </div>
                                <div class='feature'>
                                    <div class='feature-icon'>🎯</div>
                                    <h3>Set Goals</h3>
                                    <p>Define and achieve your fitness targets</p>
                                </div>
                                <div class='feature'>
                                    <div class='feature-icon'>🏆</div>
                                    <h3>Earn Badges</h3>
                                    <p>Get rewarded for consistency</p>
                                </div>
                            </div>
                            
                            <div style='text-align: center;'>
                                <a href='https://yourdomain.com/dashboard' class='button'>Start Your Journey</a>
                            </div>
                            
                            <h3>Here's how to get started:</h3>
                            <ol>
                                <li>Complete your profile with fitness goals</li>
                                <li>Log your first workout</li>
                                <li>Set up daily reminders</li>
                                <li>Join community challenges</li>
                            </ol>
                            
                            <p>Need help? Check out our Getting Started Guide or contact our support team.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2026 FitTrack. All rights reserved.</p>
                            <p>Follow us on social media for daily tips and motivation!</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetWorkoutReminderTemplate(string userName, string workoutType, DateTime scheduledTime)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <title>Workout Reminder</title>
                    <style>
                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                            line-height: 1.6;
                            color: #1F2937;
                            background-color: #F3F4F6;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background: #FFFFFF;
                            border-radius: 24px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #10B981 0%, #059669 100%);
                            padding: 40px 30px;
                            text-align: center;
                            color: white;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .workout-card {{
                            background: linear-gradient(135deg, #FEF3C7 0%, #FDE68A 100%);
                            padding: 24px;
                            border-radius: 16px;
                            text-align: center;
                            margin: 20px 0;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 14px 32px;
                            background: linear-gradient(135deg, #10B981 0%, #059669 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 12px;
                            font-weight: 600;
                            margin: 20px 0;
                        }}
                        .footer {{
                            padding: 30px;
                            text-align: center;
                            background: #F9FAFB;
                            border-top: 1px solid #E5E7EB;
                            color: #6B7280;
                            font-size: 14px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>⏰ Workout Reminder</h2>
                        </div>
                        <div class='content'>
                            <h1>Time to Move, {userName}! 💪</h1>
                            <div class='workout-card'>
                                <div style='font-size: 48px;'>🏋️</div>
                                <h2>{workoutType}</h2>
                                <p><strong>Scheduled for:</strong> {scheduledTime:dddd, MMMM d, yyyy at h:mm tt}</p>
                            </div>
                            <p>Don't skip your workout today! Every step brings you closer to your goals.</p>
                            <div style='text-align: center;'>
                                <a href='https://yourdomain.com/workout/log' class='button'>Log Your Workout</a>
                            </div>
                            <p>Need motivation? Here are some tips for your {workoutType.ToLower()} workout:</p>
                            <ul>
                                <li>Warm up for 5-10 minutes</li>
                                <li>Stay hydrated throughout</li>
                                <li>Focus on proper form</li>
                                <li>Cool down and stretch after</li>
                            </ul>
                        </div>
                        <div class='footer'>
                            <p>© 2026 FitTrack. All rights reserved.</p>
                            <p><a href='https://yourdomain.com/settings/notifications'>Manage notifications</a></p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetWeeklyReportTemplate(string userName, int workoutsCompleted, int totalCalories, int streakDays)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <title>Weekly Progress Report</title>
                    <style>
                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                            line-height: 1.6;
                            color: #1F2937;
                            background-color: #F3F4F6;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background: #FFFFFF;
                            border-radius: 24px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
                            padding: 40px 30px;
                            text-align: center;
                            color: white;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .stats-grid {{
                            display: grid;
                            grid-template-columns: repeat(3, 1fr);
                            gap: 20px;
                            margin: 30px 0;
                        }}
                        .stat-card {{
                            text-align: center;
                            padding: 20px;
                            background: #F9FAFB;
                            border-radius: 12px;
                        }}
                        .stat-number {{
                            font-size: 32px;
                            font-weight: bold;
                            color: #4F46E5;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 14px 32px;
                            background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 12px;
                            font-weight: 600;
                            margin: 20px 0;
                        }}
                        .footer {{
                            padding: 30px;
                            text-align: center;
                            background: #F9FAFB;
                            border-top: 1px solid #E5E7EB;
                            color: #6B7280;
                            font-size: 14px;
                        }}
                        @media (max-width: 480px) {{
                            .stats-grid {{
                                grid-template-columns: 1fr;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>📊 Your Weekly Report</h2>
                        </div>
                        <div class='content'>
                            <h1>Great Job This Week, {userName}! 👏</h1>
                            <p>Here's how you performed this week:</p>
                            
                            <div class='stats-grid'>
                                <div class='stat-card'>
                                    <div style='font-size: 32px;'>🏋️</div>
                                    <div class='stat-number'>{workoutsCompleted}</div>
                                    <div>Workouts Completed</div>
                                </div>
                                <div class='stat-card'>
                                    <div style='font-size: 32px;'>🔥</div>
                                    <div class='stat-number'>{totalCalories}</div>
                                    <div>Calories Burned</div>
                                </div>
                                <div class='stat-card'>
                                    <div style='font-size: 32px;'>⚡</div>
                                    <div class='stat-number'>{streakDays}</div>
                                    <div>Day Streak</div>
                                </div>
                            </div>
                            
                            {(streakDays >= 7 ? @"
                            <div style='background: #ECFDF5; padding: 16px; border-radius: 12px; margin: 20px 0;'>
                                <strong>🎉 Amazing streak!</strong> You've been consistent. Keep it up!
                            </div>" : "")}
                            
                            <div style='text-align: center;'>
                                <a href='https://yourdomain.com/dashboard' class='button'>View Full Report</a>
                            </div>
                            
                            <h3>Next Week's Goals:</h3>
                            <ul>
                                <li>Complete {Math.Min(workoutsCompleted + 1, 7)} workouts</li>
                                <li>Maintain your {streakDays}-day streak</li>
                                <li>Try a new workout type</li>
                            </ul>
                        </div>
                        <div class='footer'>
                            <p>© 2026 FitTrack. All rights reserved.</p>
                            <p>Keep pushing forward! 💪</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetGoalAchievedTemplate(string userName, string goalName)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <title>Goal Achieved! 🎉</title>
                    <style>
                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                            line-height: 1.6;
                            color: #1F2937;
                            background-color: #F3F4F6;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background: #FFFFFF;
                            border-radius: 24px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #F59E0B 0%, #D97706 100%);
                            padding: 40px 30px;
                            text-align: center;
                            color: white;
                        }}
                        .content {{
                            padding: 40px 30px;
                            text-align: center;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 14px 32px;
                            background: linear-gradient(135deg, #F59E0B 0%, #D97706 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 12px;
                            font-weight: 600;
                            margin: 20px 0;
                        }}
                        .footer {{
                            padding: 30px;
                            text-align: center;
                            background: #F9FAFB;
                            border-top: 1px solid #E5E7EB;
                            color: #6B7280;
                            font-size: 14px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>🏆 Congratulations!</h2>
                        </div>
                        <div class='content'>
                            <div style='font-size: 64px;'>🎉</div>
                            <h1>You Did It, {userName}!</h1>
                            <p>Congratulations on achieving your goal:</p>
                            <div style='background: #FEF3C7; padding: 20px; border-radius: 12px; margin: 20px 0;'>
                                <strong style='font-size: 20px;'>{goalName}</strong>
                            </div>
                            <p>Your dedication and hard work have paid off! This is a significant milestone in your fitness journey.</p>
                            
                            <div style='text-align: center;'>
                                <a href='https://yourdomain.com/dashboard' class='button'>Set Your Next Goal</a>
                            </div>
                            
                            <p>Share your achievement with friends and inspire others!</p>
                        </div>
                        <div class='footer'>
                            <p>© 2026 FitTrack. All rights reserved.</p>
                            <p>Keep reaching for new heights! 🚀</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetNewsletterTemplate(string userName, List<NewsletterArticle> articles)
        {
            var articlesHtml = "";
            foreach (var article in articles)
            {
                articlesHtml += $@"
                    <div style='margin-bottom: 30px; padding-bottom: 20px; border-bottom: 1px solid #E5E7EB;'>
                        {(string.IsNullOrEmpty(article.ImageUrl) ? "" : $"<img src='{article.ImageUrl}' style='width: 100%; border-radius: 12px; margin-bottom: 16px;' />")}
                        <h3 style='margin-bottom: 8px;'>{article.Title}</h3>
                        <p style='color: #6B7280;'>{article.Description}</p>
                        <a href='{article.Link}' style='color: #4F46E5; text-decoration: none; font-weight: 500;'>Read more →</a>
                    </div>";
            }

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <title>FitTrack Weekly Newsletter</title>
                    <style>
                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                            line-height: 1.6;
                            color: #1F2937;
                            background-color: #F3F4F6;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background: #FFFFFF;
                            border-radius: 24px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
                            padding: 40px 30px;
                            text-align: center;
                            color: white;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 12px 24px;
                            background: #4F46E5;
                            color: white;
                            text-decoration: none;
                            border-radius: 8px;
                            font-weight: 500;
                        }}
                        .footer {{
                            padding: 30px;
                            text-align: center;
                            background: #F9FAFB;
                            border-top: 1px solid #E5E7EB;
                            color: #6B7280;
                            font-size: 14px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>📰 FitTrack Weekly</h2>
                            <p>Your weekly dose of fitness inspiration</p>
                        </div>
                        <div class='content'>
                            <h1>Hello, {userName}!</h1>
                            <p>Here are this week's top articles and tips to help you stay on track:</p>
                            
                            {articlesHtml}
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='https://yourdomain.com/blog' class='button'>Read More Articles</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>© 2026 FitTrack. All rights reserved.</p>
                            <p>You received this email because you subscribed to our newsletter.</p>
                            <p><a href='https://yourdomain.com/unsubscribe'>Unsubscribe</a></p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }

        public Task Send2FAEnableEmailAsync(string to, string userName, string backupCodes)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}