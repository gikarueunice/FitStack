using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitStackDBL.Model
{
    public class VerifyOTPRequest
    {
        public int UserId { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
