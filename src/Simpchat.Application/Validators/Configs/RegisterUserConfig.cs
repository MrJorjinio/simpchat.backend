using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Validators.Configs
{
    internal static class RegisterUserConfig
    {
        public const int NameMaxLength = 30;
        public const int NameMinLength = 5;
        public const int PasswordMinLength = 5;
        public const int PasswordMaxLength = 128;
    }
}
