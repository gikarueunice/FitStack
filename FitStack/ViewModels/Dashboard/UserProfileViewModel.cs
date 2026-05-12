namespace FitStack.ViewModels.Dashboard
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public int NextLevelXP { get; set; }
        public int StreakDays { get; set; }
        public string JoinDate { get; set; } = string.Empty;
        public int TotalWorkouts { get; set; }
        public int TotalHours { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
    }
}
