using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace FitStackDBL.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendVerificationEmailAsync(string to, string verificationLink, string userName);
        Task SendPasswordResetEmailAsync(string to, string resetLink, string userName);
        Task SendWelcomeEmailAsync(string to, string userName);
        Task SendWorkoutReminderAsync(string to, string userName, string workoutType, DateTime scheduledTime);
        Task SendWeeklyProgressReportAsync(string to, string userName, int workoutsCompleted, int totalCalories, int streakDays);
        Task SendGoalAchievedEmailAsync(string to, string userName, string goalName);
        Task SendNewsletterEmailAsync(string to, string userName, List<NewsletterArticle> articles);
        Task SendEmailAsync(string to, string subject, string body);
        Task SendOTPEmailAsync(string to, string otp, string purpose);
        Task Send2FADisableEmailAsync(string to, string userName);
        Task Send2FAEnableEmailAsync(string to, string userName, string backupCodes);
    }
    public class NewsletterArticle
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
