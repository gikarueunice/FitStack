namespace FitStack.ViewModels.Analysis
{
    public class FoodAnalysisViewModel
    {
        public IFormFile? FoodImage { get; set; }
        public string? ImageBase64 { get; set; }
        public NutritionResult? NutritionResult { get; set; }
        public bool IsAnalyzing { get; set; }
        public string? ErrorMessage { get; set; }
    }
    public class NutritionResult
    {
        public string MealName { get; set; } = string.Empty;
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }
        public int Fiber { get; set; }
        public int Sugar { get; set; }
        public double ConfidenceScore { get; set; }
        public int HealthScore { get; set; }
        public string? Rationale { get; set; }
        public List<FoodComponent>? Components { get; set; }
    }

    public class FoodComponent
    {
        public string Name { get; set; } = string.Empty;
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }
    }
}
