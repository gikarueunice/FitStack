namespace FitStack.ViewModels.Wellness
{
    public class MeditationData
    {
        public int TodayMinutes { get; set; }
        public int WeeklyTotal { get; set; }
        public int MonthlyTotal { get; set; }
        public int Streak { get; set; }
        public List<MeditationSession> RecentSessions { get; set; } = new();
    }
    public class MeditationSession
    {
        public DateTime Date { get; set; }
        public int Duration { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
