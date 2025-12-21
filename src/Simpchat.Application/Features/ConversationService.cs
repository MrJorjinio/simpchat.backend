using Simpchat.Application.Errors;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.ApiResult;

using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Messages;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Features
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _conversationRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMessageRepository _messageRepo;
        private readonly IPresenceService _presenceService;

        public ConversationService(
            IConversationRepository conversationRepo,
            INotificationRepository notificationRepo,
            IUserRepository userRepo,
            IMessageRepository messageRepo,
            IPresenceService presenceService)
        {
            _conversationRepo = conversationRepo;
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
            _messageRepo = messageRepo;
            _presenceService = presenceService;
        }

        public async Task<Result> DeleteAsync(Guid conversationId, Guid userId)
        {
            var conversation = await _conversationRepo.GetByIdAsync(conversationId);

            if (conversation is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            // Authorization check: only participants can delete the conversation
            if (conversation.UserId1 != userId && conversation.UserId2 != userId)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            await _conversationRepo.DeleteAsync(conversation);

            return Result.Success();
        }

        public async Task<Result<List<UserChatResponseDto>>> GetUserConversationsAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure<List<UserChatResponseDto>>(ApplicationErrors.User.IdNotFound);
            }

            var conversations = await _conversationRepo.GetUserConversationsAsync(userId);

            var modeledConversations = new List<UserChatResponseDto>();

            foreach (var conversation in conversations)
            {
                var notificationsCount = await _notificationRepo.GetUserChatNotificationsCountAsync(userId, conversation.Id);
                var lastMessage = await _messageRepo.GetLastMessageAsync(conversation.Id);
                var lastUserSendedMessage = await _messageRepo.GetUserLastSendedMessageAsync(userId, conversation.Id);

                // Get the other user's ID to check online status
                var otherUserId = conversation.UserId1 == userId ? conversation.UserId2 : conversation.UserId1;
                var isOnline = _presenceService.IsUserOnline(otherUserId);

                var modeledConversation = new UserChatResponseDto
                {
                    AvatarUrl = conversation.UserId1 == userId ? conversation.User2.AvatarUrl : conversation.User1.AvatarUrl,
                    Name = conversation.UserId1 == userId ? conversation.User2.Username : conversation.User1.Username,
                    Id = conversation.Id,
                    Type = ChatTypes.Conversation,
                    LastMessage = new LastMessageResponseDto
                    {
                        Content = lastMessage?.Content,
                        FileUrl = lastMessage?.FileUrl,
                        SenderId = lastMessage?.SenderId,
                        SenderUsername = lastMessage?.Sender.Username,
                        SentAt = lastMessage?.SentAt,
                        ReplyId = lastMessage?.ReplyId,
                        IsSeen = lastMessage?.IsSeen ?? false
                    },
                    NotificationsCount = notificationsCount,
                    UserLastMessage = lastUserSendedMessage?.SentAt,
                    IsOnline = isOnline,
                    ParticipantsCount = 2 // DMs always have 2 participants
                };

                modeledConversations.Add(modeledConversation);
            }

            modeledConversations = modeledConversations.OrderByDescending(mc => (DateTimeOffset?)mc.LastMessage.SentAt ?? DateTimeOffset.MinValue).ToList();

            return modeledConversations;
        }
    }
}
