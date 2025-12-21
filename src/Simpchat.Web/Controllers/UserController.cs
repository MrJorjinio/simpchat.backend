using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.Services;

using Simpchat.Application.Models.Files;
using Simpchat.Application.Models.Users;
using Simpchat.Application.Validators;
using Simpchat.Domain.Enums;
using System.Security.Claims;

namespace Simpchat.Web.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMeAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _userService.GetByIdAsync(userId, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _userService.GetByIdAsync(id, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpGet("search/{username}")]
        [Authorize]
        public async Task<IActionResult> SearchAsync(string username)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _userService.SearchAsync(username, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPut("last-seen")]
        [Authorize]
        public async Task<IActionResult> SetLastSeenAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _userService.SetLastSeenAsync(userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMeAsync([FromForm]UpdateUserDto model, IFormFile? file)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Validate avatar file if present (images only)
            if (file != null)
            {
                var validationResult = FileValidator.ValidateFile(file, imageOnly: true);
                if (!validationResult.IsSuccess)
                {
                    return validationResult.ToApiResult().ToActionResult();
                }
            }

            var fileUploadRequest = new UploadFileRequest();

            if (file is not null)
            {
                fileUploadRequest = new UploadFileRequest
                {
                    Content = file.OpenReadStream(),
                    ContentType = file.ContentType,
                    FileName = file.Name
                };
            }

            var response = await _userService.UpdateAsync(userId, model, fileUploadRequest);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid userId, [FromForm] UpdateUserDto model, IFormFile? file)
        {
            // Validate avatar file if present (images only)
            if (file != null)
            {
                var validationResult = FileValidator.ValidateFile(file, imageOnly: true);
                if (!validationResult.IsSuccess)
                {
                    return validationResult.ToApiResult().ToActionResult();
                }
            }

            var fileUploadRequest = new UploadFileRequest();

            if (file is not null)
            {
                fileUploadRequest = new UploadFileRequest
                {
                    Content = file.OpenReadStream(),
                    ContentType = file.ContentType,
                    FileName = file.Name
                };
            }

            var response = await _userService.UpdateAsync(userId, model, fileUploadRequest);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpDelete("{userId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(Guid userId)
        {
            var response = await _userService.DeleteAsync(userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _userService.GetAllAsync();
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }
    }
}
