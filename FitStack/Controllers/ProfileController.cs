using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitStack.Services;
using FitStackDBL.Services;
using System.Security.Claims;

namespace FitStack.Controllers
{
    [Authorize]
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IProfileImageServices _profileImageServices;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IUserService userService,
            IProfileImageServices profileImageServices,
            IWebHostEnvironment environment,
            ILogger<ProfileController> logger)
        {
            _userService = userService;
            _profileImageServices = profileImageServices;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile profileImage)
        {
            try
            {
                Console.WriteLine("=== UPLOAD AVATAR DEBUG ===");
                Console.WriteLine($"Profile image is null: {profileImage == null}");

                if (profileImage != null)
                {
                    Console.WriteLine($"File name: {profileImage.FileName}");
                    Console.WriteLine($"File size: {profileImage.Length} bytes");
                    Console.WriteLine($"Content type: {profileImage.ContentType}");
                }

                var userId = GetCurrentUserId();
                Console.WriteLine($"Current user ID: {userId}");

                if (userId == 0)
                {
                    Console.WriteLine("User not authenticated");
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                if (profileImage == null || profileImage.Length == 0)
                {
                    Console.WriteLine("No image selected");
                    return BadRequest(new { success = false, message = "No image selected" });
                }

                // Validate file size (max 5MB)
                if (profileImage.Length > 5 * 1024 * 1024)
                {
                    Console.WriteLine($"File too large: {profileImage.Length} bytes");
                    return BadRequest(new { success = false, message = "Image size must be less than 5MB" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(profileImage.ContentType.ToLower()))
                {
                    Console.WriteLine($"Invalid file type: {profileImage.ContentType}");
                    return BadRequest(new { success = false, message = "Only JPEG, PNG, and WEBP images are supported" });
                }

                // Get current user to delete old image
                var user = await _userService.GetUserByIdAsync(userId);
                Console.WriteLine($"User found: {user != null}");

                if (user != null && !string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    Console.WriteLine($"Deleting old image: {user.ProfilePicturePath}");
                    await _profileImageServices.DeleteProfileImageAsync(userId, user.ProfilePicturePath);
                }

                // Upload new image
                Console.WriteLine("Uploading new image...");
                var imagePath = await _profileImageServices.UploadProfileImageAsync(profileImage, userId);
                Console.WriteLine($"Image path: {imagePath}");

                if (string.IsNullOrEmpty(imagePath))
                {
                    Console.WriteLine("Failed to upload image");
                    return BadRequest(new { success = false, message = "Failed to upload image" });
                }

                // Update database
                Console.WriteLine("Updating database...");
                await _userService.UpdateProfilePictureAsync(userId, imagePath, imagePath);

                Console.WriteLine("Upload successful!");
                return Ok(new { success = true, imageUrl = imagePath });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                _logger.LogError(ex, "Error uploading avatar");
                return StatusCode(500, new { success = false, message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized();

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound();

                user.FullName = request.FullName ?? user.FullName;
                user.Email = request.Email ?? user.Email;
                user.PhoneNumber = request.Phone ?? user.PhoneNumber;

                if (!string.IsNullOrEmpty(request.Dob))
                    user.DateOfBirth = DateTime.Parse(request.Dob);

                await _userService.UpdateUserAsync(user);

                return Ok(new { success = true, message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return 0;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { success = true, message = "API is working" });
        }
    }

    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Dob { get; set; }
        public string? Bio { get; set; }
    }
}