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
    public class ResetPasswordByEmailValidator : AbstractValidator<ResetPasswordByEmailDto>
    {
        public ResetPasswordByEmailValidator()
        {
            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(RegisterUserConfig.PasswordMinLength)
                    .WithMessage($"Password must be at least {RegisterUserConfig.PasswordMinLength} characters")
                .MaximumLength(RegisterUserConfig.PasswordMaxLength)
                    .WithMessage($"Password max length is {RegisterUserConfig.PasswordMaxLength}");

            RuleFor(u => u.Otp)
                .NotEmpty().WithMessage("OTP code is required.");
        }
    }
}
