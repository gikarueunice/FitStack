namespace FitStack.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public UserProfileViewModel UserProfile { get; set; } = new();
        public DailyStatsViewModel DailyStats { get; set; } = new();
        public WeeklyProgressViewModel WeeklyProgress { get; set; } = new();
        public List<GoalViewModel> ActiveGoals { get; set; } = new();
        public List<ActivityViewModel> RecentActivities { get; set; } = new();
        public List<AchievementViewModel> RecentAchievements { get; set; } = new();
        public List<WorkoutPlanViewModel> WorkoutPlans { get; set; } = new();
        public List<DietPlanViewModel> DietPlans { get; set; } = new();
        public WellnessMetricsViewModel WellnessMetrics { get; set; } = new();
        public List<SocialPostViewModel> SocialFeed { get; set; } = new();
        public List<ChallengeViewModel> ActiveChallenges { get; set; } = new();
        public List<RecommendationViewModel> PersonalizedRecommendations { get; set; } = new();
        public RecoveryInsightsViewModel RecoveryInsights { get; set; } = new();
    }
}
