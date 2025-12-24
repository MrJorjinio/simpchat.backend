using FluentValidation;
using Simpchat.Application.Errors;
using Simpchat.Application.Interfaces.Auth;
using Simpchat.Application.Interfaces.Email;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.Users;
using Simpchat.Domain.Entities;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;
using System.Text.RegularExpressions;

namespace Simpchat.Application.Features
{
    public class AuthService : IAuthService
    {

        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IGlobalRoleRepository _globalRoleRepo;
        private readonly IOtpService _otpService;
        private readonly IValidator<RegisterUserDto> _registerValidator;
        private readonly IValidator<LoginUserDto> _loginUserDto;
        private readonly IValidator<ResetPasswordDto> _resetPasswordValidator;
        private readonly IValidator<UpdatePasswordDto> _updatePasswordValidator;
        private readonly IValidator<ResetPasswordByEmailDto> _resetPasswordByEmailValidator;
        private const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public AuthService(
            IJwtTokenGenerator jwtTokenGenerator,
            IPasswordHasher passwordHasher,
            IUserRepository userRepo,
            IGlobalRoleRepository globalRoleRepo,
            IOtpService otpService,
            IValidator<RegisterUserDto> registerValidator,
            IValidator<LoginUserDto> loginUserDto,
            IValidator<ResetPasswordDto> resetPasswordValidator,
            IValidator<UpdatePasswordDto> updatePasswordValidator,
            IValidator<ResetPasswordByEmailDto> resetPasswordByEmailValidator)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = passwordHasher;
            _userRepo = userRepo;
            _globalRoleRepo = globalRoleRepo;
            _otpService = otpService;
            _registerValidator = registerValidator;
            _loginUserDto = loginUserDto;
            _resetPasswordValidator = resetPasswordValidator;
            _updatePasswordValidator = updatePasswordValidator;
            _resetPasswordByEmailValidator = resetPasswordByEmailValidator;
        }

        public async Task<Result<string>> LoginAsync(LoginUserDto loginUserDto)
        {
            var validationResult = await _loginUserDto.ValidateAsync(loginUserDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure<string>(ApplicationErrors.Validation.Failed, errors);
            }

            bool isEmail = Regex.IsMatch(loginUserDto.Credential, EmailRegex);

            var user = new User();

            if (isEmail is true)
            {
                user = await _userRepo.GetByEmailAsync(loginUserDto.Credential);

                if (user is null)
                {
                    return Result.Failure<string>(ApplicationErrors.User.EmailNotFound);
                }

                if (await _passwordHasher.VerifyAsync(user.PasswordHash, loginUserDto.Password, user.Salt) is false)
                {
                    return Result.Failure<string>(ApplicationErrors.User.WrongPasswordOrEmail);
                }
            }
            else
            {
                user = await _userRepo.GetByUsernameAsync(loginUserDto.Credential);

                if (user is null)
                {
                    return Result.Failure<string>(ApplicationErrors.User.UsernameNotFound);
                }

                if (await _passwordHasher.VerifyAsync(user.PasswordHash, loginUserDto.Password, user.Salt) is false)
                {
                    return Result.Failure<string>(ApplicationErrors.User.WrongPasswordOrUsername);
                }
            }

            string jwtToken = await _jwtTokenGenerator.GenerateJwtTokenAsync(user.Id, user.Role);
            return jwtToken;
        }

        public async Task<Result<Guid>> RegisterAsync(RegisterUserDto registerUserDto)
        {
            var validationResult = await _registerValidator.ValidateAsync(registerUserDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure<Guid>(ApplicationErrors.Validation.Failed, errors);
            }

            if (await _userRepo.GetByUsernameAsync(registerUserDto.Username) is not null)
            {
                return Result.Failure<Guid>(ApplicationErrors.User.UsernameAlreadyExists);
            }

            if (await _userRepo.GetByEmailAsync(registerUserDto.Email) is not null)
            {
                return Result.Failure<Guid>(ApplicationErrors.User.EmailAlreadyExists);
            }

            var emailOtpCodeResult = await _otpService.ValidateEmailOtpAsync(registerUserDto.Email, registerUserDto.OtpCode);

            if (emailOtpCodeResult.IsSuccess is false)
            {
                return Result.Failure<Guid>(emailOtpCodeResult.Error);
            }

            if (emailOtpCodeResult.Value is false)
            {
                return Result.Failure<Guid>(ApplicationErrors.Otp.Wrong);
            }

            string salt = await _passwordHasher.GenerateSaltAsync();
            string passwordHash = await _passwordHasher.EncryptAsync(registerUserDto.Password, salt);

            var role = await _globalRoleRepo.GetByNameAsync(Enum.GetName(GlobalRoleTypes.User));

            if (role is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.GlobalRole.NameNotFound);
            }

            var user = new User()
            {
                Username = registerUserDto.Username,
                Email = registerUserDto.Email,
                PasswordHash = passwordHash,
                Salt = salt,
                Description = string.Empty,
                HwoCanAddType = HwoCanAddYouTypes.WithConversations,
                RoleId = role.Id
            };

            await _userRepo.CreateAsync(user);

            return user.Id;
        }

        public async Task<Result> ResetPasswordAsync(Guid userId, ResetPasswordDto resetPasswordDto)
        {
            var validationResult = await _resetPasswordValidator.ValidateAsync(resetPasswordDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure(ApplicationErrors.Validation.Failed, errors);
            }

            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure(ApplicationErrors.User.IdNotFound);
            }

            var userOtpCodeResult = await _otpService.ValidateUserOtpAsync(userId, resetPasswordDto.Otp);

            if (userOtpCodeResult.IsSuccess is false)
            {
                return Result.Failure(userOtpCodeResult.Error);
            }

            var newPasswordHash = await _passwordHasher.EncryptAsync(resetPasswordDto.Password, user.Salt);
            user.PasswordHash = newPasswordHash;

            await _userRepo.UpdateAsync(user);

            return Result.Success();
        }

        public async Task<Result> UpdatePasswordAsync(Guid userId, UpdatePasswordDto updatePasswordDto)
        {
            var validationResult = await _updatePasswordValidator.ValidateAsync(updatePasswordDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure(ApplicationErrors.Validation.Failed, errors);
            }

            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure(ApplicationErrors.User.IdNotFound);
            }

            if (await _passwordHasher.VerifyAsync(user.PasswordHash, updatePasswordDto.CurrentPassword, user.Salt) is false)
            {
                return Result.Failure(ApplicationErrors.User.WrongPassword);
            }

            var newPasswordHash = await _passwordHasher.EncryptAsync(updatePasswordDto.NewPassword, user.Salt);
            user.PasswordHash = newPasswordHash;

            await _userRepo.UpdateAsync(user);

            return Result.Success();
        }

        public async Task<Result> ResetPasswordByEmailAsync(ResetPasswordByEmailDto resetPasswordByEmailDto)
        {
            var validationResult = await _resetPasswordByEmailValidator.ValidateAsync(resetPasswordByEmailDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure(ApplicationErrors.Validation.Failed, errors);
            }

            var user = await _userRepo.GetByEmailAsync(resetPasswordByEmailDto.Email);

            if (user is null)
            {
                return Result.Failure(ApplicationErrors.User.EmailNotFound);
            }

            var emailOtpCodeResult = await _otpService.ValidateEmailOtpAsync(resetPasswordByEmailDto.Email, resetPasswordByEmailDto.Otp);

            if (emailOtpCodeResult.IsSuccess is false)
            {
                return Result.Failure(emailOtpCodeResult.Error);
            }

            if (emailOtpCodeResult.Value is false)
            {
                return Result.Failure(ApplicationErrors.Otp.Wrong);
            }

            var newPasswordHash = await _passwordHasher.EncryptAsync(resetPasswordByEmailDto.Password, user.Salt);
            user.PasswordHash = newPasswordHash;

            await _userRepo.UpdateAsync(user);

            return Result.Success();
        }
    }
}
