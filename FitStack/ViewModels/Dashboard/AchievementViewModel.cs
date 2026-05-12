namespace FitStack.ViewModels.Dashboard
{
    public class AchievementViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Badge { get; set; } = string.Empty;
        public DateTime EarnedAt { get; set; }
    }
}
