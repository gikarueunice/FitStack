using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitStackDBL.Services
{
    public interface IProfileImageServices
    {
        Task<string?> UploadProfileImageAsync(IFormFile file, int userId);
        Task<bool> DeleteProfileImageAsync(int userId, string imagePath);
        Task<byte[]> ResizeImageAsync(byte[] imageBytes, int width, int height);
        Task<string> CropImageAsync(IFormFile file, int x, int y, int width, int height);
    }
}
