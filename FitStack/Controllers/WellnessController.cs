using FitStack.ViewModels.Wellness;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitStack.Controllers
{
    [Authorize]
    public class WellnessController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            await LoadUserDataAsync();
            ViewData["Title"] = "Wellness";

            var model = GetWellnessData();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LogMood([FromBody] MoodEntry entry)
        {
            // Save mood to database
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> LogWater([FromBody] WaterEntry entry)
        {
            // Save water intake to database
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> LogMeditation([FromBody] MeditationEntry entry)
        {
            // Save meditation session to database
            return Json(new { success = true });
        }

        private WellnessViewModel GetWellnessData()
        {
            return new WellnessViewModel
            {
                Metrics = new WellnessMetrics
                {
                    HRV = 65,
                    RestingHeartRate = 62,
                    SleepScore = 85,
                    SleepHours = 7,
                    EnergyLevel = 75,
                    StressLevel = 30,
                    Mood = "Great",
                    MeditationMinutes = 15,
                    MindfulnessGoal = 20,
                    WaterIntake = 5,
                    WaterGoal = 8
                },
                MoodHistory = new List<MoodLog>
                {
                    new() { Date = DateTime.Now.AddDays(-6), Mood = "Happy", Emoji = "😊", Note = "Great workout today!" },
                    new() { Date = DateTime.Now.AddDays(-5), Mood = "Tired", Emoji = "😴", Note = "Need more sleep" },
                    new() { Date = DateTime.Now.AddDays(-4), Mood = "Happy", Emoji = "😊", Note = "Good energy" },
                    new() { Date = DateTime.Now.AddDays(-3), Mood = "Stressed", Emoji = "😰", Note = "Work pressure" },
                    new() { Date = DateTime.Now.AddDays(-2), Mood = "Calm", Emoji = "😌", Note = "Meditation helped" },
                    new() { Date = DateTime.Now.AddDays(-1), Mood = "Happy", Emoji = "😊", Note = "Great day!" },
                    new() { Date = DateTime.Now, Mood = "Great", Emoji = "😄", Note = "Feeling fantastic!" }
                },
                DailyTips = new List<WellnessTip>
                {
                    new() { Title = "Take a Break", Description = "Step away from your screen for 5 minutes every hour.", Icon = "🧘", Category = "Mindfulness" },
                    new() { Title = "Stay Hydrated", Description = "Drink water before you feel thirsty.", Icon = "💧", Category = "Hydration" },
                    new() { Title = "Deep Breathing", Description = "Try 4-7-8 breathing technique to reduce stress.", Icon = "🌬️", Category = "Breathing" },
                    new() { Title = "Quality Sleep", Description = "Aim for 7-8 hours of uninterrupted sleep.", Icon = "😴", Category = "Sleep" }
                }
            };
        }
    }

    public class MoodEntry
    {
        public string Mood { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }

    public class WaterEntry
    {
        public int Amount { get; set; }
    }

    public class MeditationEntry
    {
        public int Duration { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}