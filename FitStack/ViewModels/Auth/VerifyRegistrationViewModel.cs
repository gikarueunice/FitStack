using System.ComponentModel.DataAnnotations;

namespace FitStack.ViewModels.Auth
{
    public class VerifyRegistrationViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // Initialize with empty string

        public string? PhoneNumber { get; set; } // Nullable
        public int LoginAttempts { get; set; }

        // Add validation method
        public bool IsValid()
        {
            return UserId > 0 && !string.IsNullOrEmpty(Email);
        }
    }
}
