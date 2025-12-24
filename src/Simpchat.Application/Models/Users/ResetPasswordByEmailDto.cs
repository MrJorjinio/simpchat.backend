using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Models.Users
{
    public class ResetPasswordByEmailDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Otp { get; set; }
    }
}
