using FitStackDBL.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace FitStack.Services
{
    public class ProfileImageServices : IProfileImageServices
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProfileImageServices> _logger;

        public ProfileImageServices(IWebHostEnvironment environment, ILogger<ProfileImageServices> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string?> UploadProfileImageAsync(IFormFile file, int userId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return null;

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return null;

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    return null;

                // Create user directory
                var userFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles", userId.ToString());
                if (!Directory.Exists(userFolder))
                    Directory.CreateDirectory(userFolder);

                // Generate unique filename
                var fileName = $"avatar_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                var filePath = Path.Combine(userFolder, fileName);
                var relativePath = $"/uploads/profiles/{userId}/{fileName}";

                // Process and save image
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Resize and crop to square
                using var image = await Image.LoadAsync(memoryStream);

                // Make it square (1:1 aspect ratio)
                var size = Math.Min(image.Width, image.Height);
                var x = (image.Width - size) / 2;
                var y = (image.Height - size) / 2;

                image.Mutate(i => i
                    .Crop(new Rectangle(x, y, size, size))
                    .Resize(300, 300)
                );

                // Save as JPEG
                var encoder = new JpegEncoder { Quality = 90 };
                using var outputStream = new FileStream(filePath, FileMode.Create);
                await image.SaveAsync(outputStream, encoder);

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile image for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> DeleteProfileImageAsync(int userId, string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return true;

                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile image for user {UserId}", userId);
                return false;
            }
        }

        public async Task<byte[]> ResizeImageAsync(byte[] imageBytes, int width, int height)
        {
            using var image = await Image.LoadAsync(new MemoryStream(imageBytes));
            image.Mutate(i => i.Resize(width, height));

            using var ms = new MemoryStream();
            await image.SaveAsync(ms, new JpegEncoder());
            return ms.ToArray();
        }

        public async Task<string> CropImageAsync(IFormFile file, int x, int y, int width, int height)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var image = await Image.LoadAsync(memoryStream);
            image.Mutate(i => i.Crop(new Rectangle(x, y, width, height)));

            var tempFile = Path.GetTempFileName() + ".jpg";
            using var outputStream = new FileStream(tempFile, FileMode.Create);
            await image.SaveAsync(outputStream, new JpegEncoder());

            return tempFile;
        }
    }
}