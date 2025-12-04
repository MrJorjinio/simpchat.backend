using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.Services;

using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Files;
using Simpchat.Application.Models.Messages;
using System.Security.Claims;

namespace Simpchat.Web.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IMessageReactionService _messageReactionService;

        public MessageController(IMessageService messageService, IMessageReactionService messageReactionService)
        {
            _messageService = messageService;
            _messageReactionService = messageReactionService;  // FIX: Added missing injection!
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessageAsync([FromForm] PostMessageDto messagePostDto, IFormFile? file)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            UploadFileRequest? fileUploadRequest = null;

            if (file != null)
            {
                fileUploadRequest = new UploadFileRequest
                {
                    Content = file.OpenReadStream(),
                    FileName = file.FileName,
                    ContentType = file.ContentType
                };
            }

            var messagePostRequest = new PostMessageDto
            {
                ChatId = messagePostDto.ChatId,
                Content = messagePostDto.Content,
                ReceiverId = messagePostDto.ReceiverId,
                ReplyId = messagePostDto.ReplyId,
                SenderId = userId
            };

            var response = await _messageService.SendMessageAsync(messagePostRequest, fileUploadRequest);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPut("{messageId}")]
        [Authorize]
        public async Task<IActionResult> UpdateAsync(Guid messageId, [FromForm] UpdateMessageDto updateMessageDto, IFormFile? file)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            UploadFileRequest? fileUploadRequest = null;

            if (file != null)
            {
                fileUploadRequest = new UploadFileRequest
                {
                    Content = file.OpenReadStream(),
                    FileName = file.FileName,
                    ContentType = file.ContentType
                };
            }

            var response = await _messageService.UpdateAsync(messageId, updateMessageDto, fileUploadRequest, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpDelete("{messageId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAsync(Guid messageId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _messageService.DeleteAsync(messageId, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPost("reaction")]
        [Authorize]
        public async Task<IActionResult> PutReactionAsync(Guid messageId, Guid reactionId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _messageReactionService.CreateAsync(messageId, reactionId, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpDelete("reaction")]
        [Authorize]
        public async Task<IActionResult> DeleteReactionAsync(Guid messageId, Guid reactionId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _messageReactionService.DeleteAsync(messageId, userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }
    }
}
