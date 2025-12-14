using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.Services;

using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Files;
using System.Security.Claims;

namespace Simpchat.Web.Controllers
{
    [Route("api/channels")]
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly IChannelService _channelService;
        private readonly IChatService _chatService;
        

        public ChannelController(IChannelService channelService, IChatService chatService)
        {
            _channelService = channelService;
            _chatService = chatService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAsync([FromForm]PostChatDto model, IFormFile? file)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

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


            model.OwnerId = userId;

            var response = await _channelService.CreateAsync(model, fileUploadRequest);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();

        }

        [HttpPost("add-member")]
        [Authorize]
        public async Task<IActionResult> AddMemberAsync(Guid chatId, Guid addingUserId)
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _channelService.AddSubscriberAsync(chatId, addingUserId, requesterId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPost("join")]
        [Authorize]
        public async Task<IActionResult> JoinAsync(Guid channelId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _channelService.JoinChannelAsync(channelId, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPost("leave")]
        [Authorize]
        public async Task<IActionResult> LeaveAsync(Guid chatId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var requesterId = userId;

            var response = await _channelService.DeleteSubscriberAsync(userId, chatId, requesterId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteAsync(Guid chatId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _channelService.DeleteAsync(chatId, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateAsync(Guid chatId, [FromForm]UpdateChatDto updateChatDto, IFormFile? file)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

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

            var response = await _channelService.UpdateAsync(chatId, updateChatDto, fileUploadRequest, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchAsync(string searchTerm)
        {
            var response = await _channelService.SearchAsync(searchTerm);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }
    }
}
