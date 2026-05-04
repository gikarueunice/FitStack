using System;
using System.Collections.Generic;
using System.Text;

namespace FitStackDBL.Model
{
    public class UserProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? PreferredWorkoutTime { get; set; }
        public string? FitnessInterests { get; set; } // JSON array or comma-separated
        public bool ShareProgress { get; set; }
        public bool ReceiveReminders { get; set; }
        public string? ReminderTime { get; set; }
        public Users? User { get; set; }
    }
}
