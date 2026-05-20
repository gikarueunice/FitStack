namespace FitStack.ViewModels.Auth
{
    public class VerifyRegistrationViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
