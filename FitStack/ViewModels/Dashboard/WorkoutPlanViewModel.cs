namespace FitStack.ViewModels.Dashboard
{
    public class WorkoutPlanViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int CaloriesEstimate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsToday { get; set; }
    }
}
