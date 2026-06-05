namespace FitStack.ViewModels.Wellness
{
    public class MoodLog
    {
        public DateTime Date { get; set; }
        public string Mood { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
