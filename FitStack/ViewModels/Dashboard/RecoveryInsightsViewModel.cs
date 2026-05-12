namespace FitStack.ViewModels.Dashboard
{
    public class RecoveryInsightsViewModel
    {
        public string ReadinessScore { get; set; } = string.Empty;
        public string RecoveryStatus { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public int SleepQuality { get; set; }
        public int MuscleSoreness { get; set; }
        public int Fatigue { get; set; }
        public bool RecommendedRestDay { get; set; }
    }
}
