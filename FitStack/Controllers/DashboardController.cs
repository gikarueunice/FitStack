using FitStack.ViewModels;
using FitStack.ViewModels.Dashboard;
using FitStackDBL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace FitStack.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUserService _userService;

        public DashboardController(IUserService userService)
        {
            _userService = userService;
        }
        public IActionResult Index()
        {
            var model = GetDashboardData();

            return View(model);
        }
        private async Task SetUserDataAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    ViewBag.UserId = user.Id;
                    ViewBag.UserName = user.FullName;
                    ViewBag.UserEmail = user.Email;
                    ViewBag.UserAvatar = user.ProfilePictureUrl ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=80&h=80&fit=crop";
                    ViewBag.UserLevel = user.Level ?? 1;
                    ViewBag.UserXP = user.XP ?? 0;
                    ViewBag.JoinDate = user.CreatedAt.ToString("MMM yyyy");
                }
            }
        }
        public async Task<IActionResult> Goals()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "My Goals";
            return View();
        }

        public async Task<IActionResult> Schedules()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "Schedules";
            return View();
        }

        public async Task<IActionResult> WorkOutPlan()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "Workout Plan";
            return View();
        }

        public async Task<IActionResult> DietPlan()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "Diet Plan";
            return View();
        }

        public async Task<IActionResult> Progress()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "Progress";
            return View();
        }

        public async Task<IActionResult> Achievements()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "Achievements";
            return View();
        }

        public async Task<IActionResult> Statistics()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "Statistics";
            return View();
        }

        public async Task<IActionResult> Community()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "Community";
            return View();
        }

        public async Task<IActionResult> Wellness()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "Wellness";
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "Profile";
            return View();
        }

        public async Task<IActionResult> Settings()
        {
            await SetUserDataAsync();
            ViewData["Title"] = "Settings";
            return View();
        }
        
        public IActionResult Social()
        {
            return View();
        }
        private async Task LoadUserDataAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    ViewBag.UserId = user.Id;
                    ViewBag.UserName = user.FullName;
                    ViewBag.UserEmail = user.Email;
                    ViewBag.UserAvatar = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : "";
                    ViewBag.UserInitial = user.FullName?.FirstOrDefault().ToString().ToUpper() ?? "U";
                    ViewBag.UserLevel = user.Level;
                    ViewBag.UserXP = user.XP;
                }
            }
        }
        private DashboardViewModel GetDashboardData()
        {
            // This would come from your database
            return new DashboardViewModel
            {
                UserProfile = new UserProfileViewModel
                {
                    Id = 1,
                    FullName = "Alex Johnson",
                    Email = "alex@example.com",
                    ProfilePictureUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100&h=100&fit=crop",
                    Level = 12,
                    XP = 2450,
                    NextLevelXP = 3000,
                    StreakDays = 45,
                    JoinDate = "Jan 2024",
                    TotalWorkouts = 128,
                    TotalHours = 86,
                    Followers = 234,
                    Following = 156
                },
                DailyStats = new DailyStatsViewModel
                {
                    Steps = 7842,
                    StepGoal = 10000,
                    CaloriesBurned = 520,
                    CaloriesGoal = 600,
                    ActiveMinutes = 45,
                    ActiveMinutesGoal = 60,
                    DistanceKm = 6.2,
                    DistanceGoal = 8,
                    WaterIntake = 5,
                    WaterGoal = 8,
                    SleepHours = 7,
                    SleepGoal = 8
                },
                ActiveGoals = new List<GoalViewModel>
                {
                    new() { Id = 1, Title = "Weekly Workouts", Description = "Complete 5 workouts this week", Current = 3, Target = 5, Unit = "workouts", Icon = "🏋️", Color = "#4F46E5", Progress = 60 },
                    new() { Id = 2, Title = "Daily Steps", Description = "Reach 10,000 steps daily", Current = 7842, Target = 10000, Unit = "steps", Icon = "👟", Color = "#10B981", Progress = 78 },
                    new() { Id = 3, Title = "Weight Loss", Description = "Lose 5 kg", Current = 2.5, Target = 5, Unit = "kg", Icon = "⚖️", Color = "#F59E0B", Progress = 50 }
                },
                RecentActivities = new List<ActivityViewModel>
                {
                    new() { Id = 1, Type = "Running", Title = "Morning Run", Duration = 30, Calories = 320, Timestamp = DateTime.Now.AddHours(-2), Icon = "🏃" },
                    new() { Id = 2, Type = "Strength", Title = "Upper Body Workout", Duration = 45, Calories = 280, Timestamp = DateTime.Now.AddDays(-1), Icon = "💪" },
                    new() { Id = 3, Type = "Yoga", Title = "Evening Stretch", Duration = 20, Calories = 95, Timestamp = DateTime.Now.AddDays(-1).AddHours(-3), Icon = "🧘" }
                },
                RecentAchievements = new List<AchievementViewModel>
                {
                    new() { Id = 1, Name = "100 Workouts", Description = "Completed 100 total workouts", Icon = "🎯", Badge = "gold", EarnedAt = DateTime.Now.AddDays(-5) },
                    new() { Id = 2, Name = "30 Day Streak", Description = "30 consecutive days active", Icon = "🔥", Badge = "silver", EarnedAt = DateTime.Now.AddDays(-10) },
                    new() { Id = 3, Name = "Early Bird", Description = "10 workouts before 6 AM", Icon = "🌅", Badge = "bronze", EarnedAt = DateTime.Now.AddDays(-15) }
                },
                WorkoutPlans = new List<WorkoutPlanViewModel>
                {
                    new() { Id = 1, Name = "Full Body HIIT", Difficulty = "Hard", Duration = 30, CaloriesEstimate = 350, ImageUrl = "", IsToday = true },
                    new() { Id = 2, Name = "Upper Body Strength", Difficulty = "Medium", Duration = 45, CaloriesEstimate = 300, ImageUrl = "", IsToday = false },
                    new() { Id = 3, Name = "Core Crusher", Difficulty = "Medium", Duration = 20, CaloriesEstimate = 180, ImageUrl = "", IsToday = false }
                },
                DietPlans = new List<DietPlanViewModel>
                {
                    new() { Id = 1, Meal = "Breakfast", Name = "Protein Oatmeal", Calories = 350, Protein = 25, Carbs = 45, Fats = 10, Icon = "🥣" },
                    new() { Id = 2, Meal = "Lunch", Name = "Grilled Chicken Salad", Calories = 450, Protein = 40, Carbs = 20, Fats = 15, Icon = "🥗" },
                    new() { Id = 3, Meal = "Dinner", Name = "Salmon with Quinoa", Calories = 550, Protein = 45, Carbs = 35, Fats = 20, Icon = "🐟" }
                },
                WellnessMetrics = new WellnessMetricsViewModel
                {
                    HRV = 65,
                    RestingHeartRate = 62,
                    SleepScore = 85,
                    EnergyLevel = 75,
                    StressLevel = 30,
                    Mood = "Great",
                    MeditationMinutes = 15,
                    MindfulnessGoal = 20
                },
                SocialFeed = new List<SocialPostViewModel>
                {
                    new() { Id = 1, UserName = "Sarah Parker", UserAvatar = "https://randomuser.me/api/portraits/women/1.jpg", Content = "Just completed a 10k run! 🏃‍♀️ New personal best!", Likes = 45, Comments = 12, Timestamp = DateTime.Now.AddHours(-3), IsLiked = false },
                    new() { Id = 2, UserName = "Mike Chen", UserAvatar = "https://randomuser.me/api/portraits/men/2.jpg", Content = "30 days of consistency! Feeling stronger than ever 💪", Likes = 89, Comments = 23, Timestamp = DateTime.Now.AddDays(-1), IsLiked = true }
                },
                ActiveChallenges = new List<ChallengeViewModel>
                {
                    new() { Id = 1, Name = "Summer Shape Up", Description = "Complete 20 workouts in 30 days", Icon = "🏆", Participants = 1234, YourProgress = 12, Target = 20, EndDate = DateTime.Now.AddDays(18), IsJoined = true },
                    new() { Id = 2, Name = "10K Steps Daily", Description = "Reach 10,000 steps every day", Icon = "👟", Participants = 856, YourProgress = 7, Target = 30, EndDate = DateTime.Now.AddDays(23), IsJoined = true }
                },
                PersonalizedRecommendations = new List<RecommendationViewModel>
                {
                    new() { Id = 1, Type = "Workout", Title = "5K Training Plan", Description = "Perfect for improving your running endurance", ImageUrl = "", Duration = "4 weeks", Difficulty = "Intermediate" },
                    new() { Id = 2, Type = "Nutrition", Title = "High Protein Meals", Description = "Recipes to support muscle growth", ImageUrl = "", Duration = "15 min read", Difficulty = "Easy" }
                },
                RecoveryInsights = new RecoveryInsightsViewModel
                {
                    ReadinessScore = "78%",
                    RecoveryStatus = "Moderate",
                    Recommendation = "Light activity recommended today. Focus on mobility and hydration.",
                    SleepQuality = 85,
                    MuscleSoreness = 40,
                    Fatigue = 35,
                    RecommendedRestDay = false
                }
            };
        }
    }
}
