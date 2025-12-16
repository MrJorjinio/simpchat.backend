using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simpchat.Application.Common.Pagination.Chat;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.Services;

using Simpchat.Application.Models.Files;
using Simpchat.Application.Models.Messages;
using Simpchat.Domain.Enums;
using System.Security.Claims;

namespace Simpchat.Web.Controllers
{
    [Route("api/chats")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;
        private readonly IChatBanService _chatBanService;

        public ChatController(IChatService chatService, IMessageService messageService, IChatBanService chatBanService)
        {
            _chatService = chatService;
            _messageService = messageService;
            _chatBanService = chatBanService;
        }

        [HttpPost("search")]
        [Authorize]
        public async Task<IActionResult> SearchByNameAsync(ChatSearchPageModel model)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _chatService.SearchAsync(model.searchTerm, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPost("add-user-permission")]
        [Authorize]
        public async Task<IActionResult> AddPermissionAsync(string permissionName, Guid chatId, Guid addingUserId)
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _chatService.AddUserPermissionAsync(chatId, addingUserId, permissionName, requesterId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyChatsAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _chatService.GetUserChatsAsync(userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpGet("{chatId}")]
        [Authorize]
        public async Task<IActionResult> GetByIdAsync(Guid chatId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _chatService.GetByIdAsync(chatId, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpGet("{chatId}/profile")]
        [Authorize]
        public async Task<IActionResult> GetProfileByIdAsync(Guid chatId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _chatService.GetProfileAsync(chatId, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPut("privacy-type")]
        [Authorize]
        public async Task<IActionResult> UpdatePrivacyTypeAsync(Guid chatId, ChatPrivacyTypes privacyType)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _chatService.UpdatePrivacyTypeAsync(chatId, privacyType, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPost("ban/{userId}")]
        [Authorize]
        public async Task<IActionResult> BanUserAsync(Guid chatId, Guid userId)
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _chatBanService.BanUserAsync(chatId, userId, requesterId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPost("unban/{userId}")]
        [Authorize]
        public async Task<IActionResult> UnbanUserAsync(Guid chatId, Guid userId)
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _chatBanService.DeleteAsync(chatId, userId, requesterId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpGet("{chatId}/banned-users")]
        [Authorize]
        public async Task<IActionResult> GetBannedUsersAsync(Guid chatId)
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _chatBanService.GetBannedUsersAsync(chatId, requesterId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }
    }
}
