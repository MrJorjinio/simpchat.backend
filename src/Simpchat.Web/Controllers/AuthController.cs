using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.Auth;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.Users;
using System.Security.Claims;

namespace Simpchat.Web.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterUserDto registerUserDto)
        {
            var response = await _authService.RegisterAsync(registerUserDto);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginUserDto loginUserDto)
        {
            var response = await _authService.LoginAsync(loginUserDto);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPut("update-password")]
        [Authorize]
        public async Task<IActionResult> UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _authService.UpdatePasswordAsync(userId, updatePasswordDto);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPut("forgot-password")]
        [Authorize]
        public async Task<IActionResult> ForgotPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _authService.ResetPasswordAsync(userId, resetPasswordDto);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }
    }
}
