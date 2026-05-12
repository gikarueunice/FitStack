namespace FitStack.ViewModels.Dashboard
{
    public class ChallengeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int Participants { get; set; }
        public int YourProgress { get; set; }
        public int Target { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsJoined { get; set; }
    }
}
