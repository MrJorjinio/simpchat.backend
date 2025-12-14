using FluentValidation;
using Simpchat.Application.Models.Users;
using Simpchat.Application.Validators.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Validators
{
    internal class RegisterUserValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserValidator()
        {
            RuleFor(u => u.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(RegisterUserConfig.NameMaxLength)
                    .WithMessage($"Username max length is {RegisterUserConfig.NameMaxLength}")
                .MinimumLength(RegisterUserConfig.NameMinLength)
                    .WithMessage($"Username min length is {RegisterUserConfig.NameMinLength}")
                .Matches(@"^[A-Za-z0-9._\-]+$").WithMessage("Username may contain letters, digits, dots, underscores and hyphens only.");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(RegisterUserConfig.PasswordMinLength)
                    .WithMessage($"Password must be at least {RegisterUserConfig.PasswordMinLength} characters")
                .MaximumLength(RegisterUserConfig.PasswordMaxLength)
                    .WithMessage($"Password max length is {RegisterUserConfig.PasswordMaxLength}")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"\d").WithMessage("Password must contain at least one digit")
                .Matches(@"[@$!%*?&#^()_\-+=\[\]{}|\\:;""'<>,.\/]").WithMessage("Password must contain at least one special character");
        }
    }
}
