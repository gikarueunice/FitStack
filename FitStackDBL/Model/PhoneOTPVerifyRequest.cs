using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitStackDBL.Model
{
    public class PhoneOTPVerifyRequest
    {
        public string PhoneNumber { get; set; }
        public required String Code { get; set; }
    }
}
