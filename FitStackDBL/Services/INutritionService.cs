using FitStackDBL.Model;

namespace FitStackDBL.Services
{
    public interface INutritionService
    {
        Task<NutritionResult?> AnalyzeFoodImageAsync(byte[] imageBytes, string fileName);
        Task<NutritionResult?> AnalyzeFoodImageFromBase64Async(string base64Image);
    }
}
