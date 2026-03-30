using System;
using System.Collections.Generic;
using System.Text;

namespace FitStackDBL.Model
{
    public class Users
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        // Stored/hash password
        public string? Password { get; set; }
        // Optional fields used by the web app
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public string? PhoneNumber{ get; set; }
    }
}
