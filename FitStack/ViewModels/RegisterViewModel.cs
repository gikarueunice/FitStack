using System.ComponentModel.DataAnnotations;

namespace FitStack.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;


        [Required]
        [Phone]
        [Display(Name = "Mobile Phone")]
        [RegularExpression(@"^\+?[1-9][0-9]{7,14}$",
        ErrorMessage = "Enter a valid phone number (e.g., +1234567890)")]
        public string? PhoneNumber { get; set; }
       


        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [MinimumAge(13, ErrorMessage = "You must be at least 13 years old to register")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [Display(Name = "Height (cm)")]
        [Range(100, 250, ErrorMessage = "Height must be between 100cm and 250cm")]
        public int? Height { get; set; }

        [Display(Name = "Weight (kg)")]
        [Range(30, 250, ErrorMessage = "Weight must be between 30kg and 250kg")]
        public decimal? Weight { get; set; }

        [Display(Name = "Fitness Goal")]
        public string? FitnessGoal { get; set; }

        [Display(Name = "Activity Level")]
        public string? ActivityLevel { get; set; }

        [Required(ErrorMessage = "You must accept the terms and conditions")]
        [Display(Name = "I agree to the Terms and Conditions and Privacy Policy")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTerms { get; set; }

        [Display(Name = "Subscribe to newsletter")]
        public bool SubscribeToNewsletter { get; set; }

        // For plan selection from pricing page
        public string? SelectedPlan { get; set; }
    }

    // Custom validation attribute for minimum age
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Null values are handled by Required attribute
            }

            if (value is DateTime dateOfBirth)
            {
                var age = DateTime.Today.Year - dateOfBirth.Year;
                if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

                if (age < _minimumAge)
                {
                    return new ValidationResult($"You must be at least {_minimumAge} years old.");
                }
            }

            return ValidationResult.Success;
        }
    }
}