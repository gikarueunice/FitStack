namespace FitStack.ViewModels.Wellness
{
    public class WellnessViewModel
    {
        public WellnessMetrics Metrics { get; set; } = new();
        public List<MoodLog> MoodHistory { get; set; } = new();
        public List<WellnessTip> DailyTips { get; set; } = new();
        public HydrationData Hydration { get; set; } = new();
        public MeditationData Meditation { get; set; } = new();
    }
}
