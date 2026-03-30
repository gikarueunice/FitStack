using System.Security.Cryptography;
using System.Text;

namespace FitStack.Helpers
{
    public static class PasswordHelper
    {
        public static bool IsValidPassword(string? password)
        {
            return !string.IsNullOrEmpty(password) && password.Length >= 6;
        }

        public static string HashPassword(string password)
        {
            // Simple SHA256 hash for compile-time safety. Replace with BCrypt in production.
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string? hashed)
        {
            if (hashed == null) return false;
            return HashPassword(password) == hashed;
        }
    }
}
