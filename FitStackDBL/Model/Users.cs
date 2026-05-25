using System;
using System.Collections.Generic;
using System.Text;

namespace FitStackDBL.Model
{
    public class Users
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public required string PhoneNumber { get; set; } 
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Salt { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public int? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? FitnessGoal { get; set; }
        public string? ActivityLevel { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? EmailVerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public int? EmailOTP { get; set; }
        public string? PhoneOTP { get; set; }
        public string? EmailOTPExpiry { get; set; }
        public string? PhoneOTPExpiry { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedUntil { get; set; }
        public string? SelectedPlan { get; set; }
        public bool SubscribeToNewsletter { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public UserProfile? Profile { get; set; }
        public string? Password { get; internal set; }
        public bool IsPhoneVerified { get; set; }
        public int LoginAttempts { get; set; }
    }
}
