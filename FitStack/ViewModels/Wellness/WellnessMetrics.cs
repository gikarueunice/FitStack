namespace FitStack.ViewModels.Wellness
{
    public class WellnessMetrics
    {
        public int HRV { get; set; }
        public int RestingHeartRate { get; set; }
        public int SleepScore { get; set; }
        public int SleepHours { get; set; }
        public int EnergyLevel { get; set; }
        public int StressLevel { get; set; }
        public string Mood { get; set; } = "Good";
        public int MeditationMinutes { get; set; }
        public int MindfulnessGoal { get; set; }
        public int WaterIntake { get; set; }
        public int WaterGoal { get; set; }
    }
}
