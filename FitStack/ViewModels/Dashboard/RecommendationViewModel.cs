namespace FitStack.ViewModels.Dashboard
{
    public class RecommendationViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
    }
}
