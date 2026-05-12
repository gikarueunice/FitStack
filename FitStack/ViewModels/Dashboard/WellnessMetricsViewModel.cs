namespace FitStack.ViewModels.Dashboard
{
    public class WellnessMetricsViewModel
    {
        public int HRV { get; set; }
        public int RestingHeartRate { get; set; }
        public int SleepScore { get; set; }
        public int EnergyLevel { get; set; }
        public int StressLevel { get; set; }
        public string Mood { get; set; } = string.Empty;
        public int MeditationMinutes { get; set; }
        public int MindfulnessGoal { get; set; }
    }
}
