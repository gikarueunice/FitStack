namespace FitStack.ViewModels.Dashboard
{
    public class WeeklyProgressViewModel
    {
        public List<string> Days { get; set; } = new();
        public List<int> WorkoutMinutes { get; set; } = new();
        public List<int> CaloriesData { get; set; } = new();
        public int TotalWorkouts { get; set; }
        public int TotalCalories { get; set; }
        public int AverageHeartRate { get; set; }
        public int Improvement { get; set; }
    }
}
