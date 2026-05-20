using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitStackDBL.Model
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromPhoneNumber { get; set; } = string.Empty;
        public string? MessagingServiceSid { get; set; }
    }
}
