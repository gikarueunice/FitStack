namespace FitStack.ViewModels.Dashboard
{
    public class DailyStatsViewModel
    {
        public int Steps { get; set; }
        public int StepGoal { get; set; }
        public int CaloriesBurned { get; set; }
        public int CaloriesGoal { get; set; }
        public int ActiveMinutes { get; set; }
        public int ActiveMinutesGoal { get; set; }
        public double DistanceKm { get; set; }
        public double DistanceGoal { get; set; }
        public int WaterIntake { get; set; }
        public int WaterGoal { get; set; }
        public int SleepHours { get; set; }
        public int SleepGoal { get; set; }
    }
}
