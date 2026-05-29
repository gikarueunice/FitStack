using FitStack.Services;
using FitStack.ViewModels.Analysis;
using FitStackDBL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitStack.Controllers
{
    [Authorize]
    public class FoodAnalysisController : Controller
    {
        private readonly INutritionService _nutritionService;
        private readonly ILogger<FoodAnalysisController> _logger;

        public FoodAnalysisController(INutritionService nutritionService, ILogger<FoodAnalysisController> logger)
        {
            _nutritionService = nutritionService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new FoodAnalysisViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Analyze(IFormFile foodImage)
        {
            if (foodImage == null || foodImage.Length == 0)
            {
                return Json(new { success = false, message = "Please select an image to analyze" });
            }

            // Check file size (max 5MB)
            if (foodImage.Length > 5 * 1024 * 1024)
            {
                return Json(new { success = false, message = "Image size must be less than 5MB" });
            }

            // Check file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(foodImage.ContentType))
            {
                return Json(new { success = false, message = "Only JPEG, PNG, and WEBP images are supported" });
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await foodImage.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                var result = await _nutritionService.AnalyzeFoodImageAsync(imageBytes, foodImage.FileName);

                if (result != null)
                {
                    // Save to user's food log (optional)
                    //await SaveToFoodLog(result);

                    return Json(new { success = true, data = result });
                }

                return Json(new { success = false, message = "Could not analyze the image. Please try again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing food image");
                return Json(new { success = false, message = "An error occurred during analysis" });
            }
        }

        private async Task SaveToFoodLog(NutritionResult result)
        {
            // TODO: Save to database
            // This would save the analyzed food to the user's daily food log
            await Task.CompletedTask;
        }
    }
}