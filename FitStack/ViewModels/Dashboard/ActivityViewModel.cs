namespace FitStack.ViewModels.Dashboard
{
    public class ActivityViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int Calories { get; set; }
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = string.Empty;
    }
}
