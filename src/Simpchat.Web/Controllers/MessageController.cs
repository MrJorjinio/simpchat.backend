using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Files;
using Simpchat.Application.Models.Messages;
using Simpchat.Application.Validators;
using Simpchat.Web.Hubs;
using System.Security.Claims;

namespace Simpchat.Web.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IMessageReactionService _messageReactionService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IMessageRepository _messageRepository;
        private readonly IPresenceService _presenceService;
        private readonly IUserService _userService;
        private readonly IChatService _chatService;

        public MessageController(
            IMessageService messageService,
            IMessageReactionService messageReactionService,
            IHubContext<ChatHub> hubContext,
            IMessageRepository messageRepository,
            IPresenceService presenceService,
            IUserService userService,
            IChatService chatService)
        {
            _messageService = messageService;
            _messageReactionService = messageReactionService;
            _hubContext = hubContext;
            _messageRepository = messageRepository;
            _presenceService = presenceService;
            _userService = userService;
            _chatService = chatService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessageAsync([FromForm] PostMessageDto messagePostDto, IFormFile? file)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Ensure at least content or file is provided
            if (string.IsNullOrWhiteSpace(messagePostDto.Content) && file == null)
            {
                return BadRequest(new { success = false, error = "Message must have either content or a file attachment" });
            }

            // Validate file if present
            if (file != null)
            {
                var validationResult = FileValidator.ValidateFile(file, imageOnly: false);
                if (!validationResult.IsSuccess)
                {
                    return validationResult.ToApiResult().ToActionResult();
                }
            }

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

            // If successful and has a file, broadcast via SignalR
            if (response.IsSuccess && file != null)
            {
                try
                {
                    // Get the created message to retrieve the fileUrl
                    var message = await _messageRepository.GetByIdAsync(response.Value);

                    if (message != null && messagePostDto.ChatId.HasValue)
                    {
                        // Get sender details for the broadcast
                        var senderResult = await _userService.GetByIdAsync(userId, userId);
                        var senderUsername = senderResult.IsSuccess ? senderResult.Value.Username : "Unknown";
                        var senderAvatarUrl = senderResult.IsSuccess ? senderResult.Value.AvatarUrl : null;

                        // Broadcast to chat group
                        await _hubContext.Clients.Group($"chat_{messagePostDto.ChatId.Value}")
                            .SendAsync("ReceiveMessage", new
                            {
                                messageId = message.Id,
                                chatId = message.ChatId,
                                senderId = userId,
                                senderUsername = senderUsername,
                                senderAvatarUrl = senderAvatarUrl,
                                content = message.Content,
                                fileUrl = message.FileUrl,
                                replyId = message.ReplyId,
                                sentAt = message.SentAt
                            });

                        // Broadcast notification to recipients
                        await BroadcastFileNotificationAsync(userId, messagePostDto, message);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the request
                    // The file was uploaded successfully, SignalR broadcast is best-effort
                    Console.WriteLine($"Failed to broadcast file message via SignalR: {ex.Message}");
                }
            }

            var apiResponse = response.ToApiResult();
            return apiResponse.ToActionResult();
        }

        [HttpPut("{messageId}")]
        [Authorize]
        public async Task<IActionResult> UpdateAsync(Guid messageId, [FromForm] UpdateMessageDto updateMessageDto, IFormFile? file)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Validate file if present
            if (file != null)
            {
                var validationResult = FileValidator.ValidateFile(file, imageOnly: false);
                if (!validationResult.IsSuccess)
                {
                    return validationResult.ToApiResult().ToActionResult();
                }
            }

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

        // Helper method to broadcast file notifications
        private async Task BroadcastFileNotificationAsync(Guid senderId, PostMessageDto request, Domain.Entities.Message message)
        {
            try
            {
                var recipientIds = new List<Guid>();
                string chatName = "";
                string chatAvatar = "";

                if (request.ReceiverId.HasValue && request.ReceiverId.Value != Guid.Empty)
                {
                    // Direct conversation
                    recipientIds.Add(request.ReceiverId.Value);

                    // For conversation, chat name/avatar is sender's info
                    var senderResult = await _userService.GetByIdAsync(senderId, senderId);
                    if (senderResult.IsSuccess)
                    {
                        chatName = senderResult.Value.Username;
                        chatAvatar = senderResult.Value.AvatarUrl;
                    }
                }
                else if (request.ChatId.HasValue)
                {
                    // Group or channel chat
                    var profileResult = await _chatService.GetProfileAsync(request.ChatId.Value, senderId);
                    if (profileResult.IsSuccess)
                    {
                        var profile = profileResult.Value;
                        chatName = profile.Name;
                        chatAvatar = profile.AvatarUrl;
                        recipientIds = profile.Members
                            .Where(m => m.UserId != senderId)
                            .Select(m => m.UserId)
                            .ToList();
                    }
                }

                if (recipientIds.Any())
                {
                    // Get sender details
                    var senderResult = await _userService.GetByIdAsync(senderId, senderId);
                    if (senderResult.IsSuccess)
                    {
                        var sender = senderResult.Value;
                        var notificationPayload = new
                        {
                            messageId = message.Id,
                            chatId = request.ChatId ?? Guid.Empty,
                            chatName,
                            chatAvatar,
                            senderName = sender.Username,
                            content = message.Content,
                            fileUrl = message.FileUrl,
                            sentTime = message.SentAt
                        };

                        // Broadcast to each recipient's connections
                        foreach (var recipientId in recipientIds)
                        {
                            var connections = _presenceService.GetUserConnections(recipientId);
                            foreach (var connectionId in connections)
                            {
                                try
                                {
                                    await _hubContext.Clients.Client(connectionId)
                                        .SendAsync("NewNotification", notificationPayload);
                                }
                                catch
                                {
                                    // Ignore individual connection failures
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to broadcast file notification: {ex.Message}");
            }
        }
    }
}
