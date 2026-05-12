namespace FitStack.ViewModels.Dashboard
{
    public class GoalViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Current { get; set; }
        public int Target { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public double Progress { get; set; }
    }
}
