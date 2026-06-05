namespace FitStack.ViewModels.Wellness
{
    public class HydrationData
    {
        public int CurrentIntake { get; set; }
        public int Goal { get; set; } = 8;
        public List<HydrationLog> RecentLogs { get; set; } = new();
    }
    public class HydrationLog
    {
        public DateTime Time { get; set; }
        public int Amount { get; set; }
    }
}
