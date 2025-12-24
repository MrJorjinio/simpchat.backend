using FluentValidation;
using Simpchat.Application.Errors;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.File;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.ApiResult;

using Simpchat.Application.Models.Files;
using Simpchat.Application.Models.Messages;
using Simpchat.Domain.Entities;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Features
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IChatRepository _chatRepo;
        private readonly IFileStorageService _fileStorageService;
        private readonly IConversationRepository _conversationRepo;
        private readonly IValidator<PostMessageDto> _postMessageValidator;
        private readonly IValidator<UpdateMessageDto> _updateMessageValidator;
        private readonly IChatBanRepository _chatBanRepository;
        private readonly IUserBanRepository _userBanRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IChatUserPermissionRepository _permissionRepository;
        private readonly IChatHubService _chatHubService;
        private const string BucketName = "messages-files";

        public MessageService(
            IMessageRepository repo,
            IUserRepository userRepo,
            IFileStorageService fileStorageService,
            IChatRepository chatRepo,
            IConversationRepository conversationRepo,
            IValidator<PostMessageDto> postMessageValidator,
            IValidator<UpdateMessageDto> updateMessageValidator,
            IChatBanRepository chatBanRepository,
            IUserBanRepository userBanRepository,
            IGroupRepository groupRepository,
            IChannelRepository channelRepository,
            INotificationRepository notificationRepository,
            IChatUserPermissionRepository permissionRepository,
            IChatHubService chatHubService)
        {
            _repo = repo;
            _userRepo = userRepo;
            _fileStorageService = fileStorageService;
            _chatRepo = chatRepo;
            _conversationRepo = conversationRepo;
            _postMessageValidator = postMessageValidator;
            _updateMessageValidator = updateMessageValidator;
            _chatBanRepository = chatBanRepository;
            _userBanRepository = userBanRepository;
            _groupRepository = groupRepository;
            _channelRepository = channelRepository;
            _notificationRepository = notificationRepository;
            _permissionRepository = permissionRepository;
            _chatHubService = chatHubService;
        }

        public async Task<Result<Guid>> SendMessageAsync(PostMessageDto postMessageDto, UploadFileRequest? uploadFileRequest)
        {
            var validationResult = await _postMessageValidator.ValidateAsync(postMessageDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure<Guid>(ApplicationErrors.Validation.Failed, errors);
            }

            var sender = await _userRepo.GetByIdAsync(postMessageDto.SenderId);
            if (sender is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.User.IdNotFound);
            }

            string? fileUrl = null;
            if (uploadFileRequest is not null)
            {
                if (uploadFileRequest.Content != null &&
                uploadFileRequest.FileName != null &&
                uploadFileRequest.ContentType != null)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{uploadFileRequest.FileName}";
                    fileUrl = await _fileStorageService.UploadFileAsync(
                        BucketName,
                        uniqueFileName,
                        uploadFileRequest.Content,
                        uploadFileRequest.ContentType
                    );
                }
            }

            if (postMessageDto.ReplyId is not null)
            {
                if (await _repo.GetByIdAsync((Guid)postMessageDto.ReplyId) is null)
                {
                    return Result.Failure<Guid>(ApplicationErrors.Message.IdNotFound);
                }
            }

            Guid chatId;
            if (postMessageDto.ChatId != null)
            {
                chatId = (Guid)postMessageDto.ChatId;
            }
            else
            {
                // This is a DM - check user-to-user ban before proceeding
                var receiverId = (Guid)postMessageDto.ReceiverId;

                // Check if the receiver has blocked the sender
                if (await _userBanRepository.IsUserBannedAsync(receiverId, postMessageDto.SenderId))
                {
                    return Result.Failure<Guid>(ApplicationErrors.UserBan.UserBanned);
                }

                // Check if the sender has blocked the receiver (they need to unblock first)
                if (await _userBanRepository.IsUserBannedAsync(postMessageDto.SenderId, receiverId))
                {
                    return Result.Failure<Guid>(ApplicationErrors.UserBan.CannotMessageBannedUser);
                }

                var conversationBetweenId = await _conversationRepo.GetConversationBetweenAsync(postMessageDto.SenderId, receiverId);
                if (conversationBetweenId != null)
                {
                    chatId = (Guid)conversationBetweenId;
                }
                else
                {
                    var newChat = new Chat
                    {
                        Id = Guid.NewGuid(),
                        Type = ChatTypes.Conversation,
                    };

                    await _chatRepo.CreateAsync(newChat);

                    var newConversation = new Conversation
                    {
                        Id = newChat.Id,
                        UserId1 = postMessageDto.SenderId,
                        UserId2 = receiverId
                    };

                    await _conversationRepo.CreateAsync(newConversation);
                    chatId = newChat.Id;

                    // Notify the receiver about the new conversation via SignalR
                    await _chatHubService.NotifyNewConversationAsync(
                        receiverId,
                        newChat.Id,
                        postMessageDto.SenderId,
                        sender.Username,
                        sender.AvatarUrl
                    );

                    // Also add the sender to the chat group
                    await _chatHubService.AddUserToChatGroupAsync(postMessageDto.SenderId, newChat.Id);

                    // Get receiver info and notify the sender about the conversation creation
                    // so they can switch from temp chat to real chat
                    var receiver = await _userRepo.GetByIdAsync(receiverId);
                    if (receiver != null)
                    {
                        await _chatHubService.NotifyConversationCreatedAsync(
                            postMessageDto.SenderId,
                            newChat.Id,
                            receiverId,
                            receiver.Username,
                            receiver.AvatarUrl
                        );
                    }
                }
            }

            var chat = await _chatRepo.GetByIdAsync(chatId);
            if (chat is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.Chat.IdNotFound);
            }

            var isBanned = await _chatBanRepository.IsUserBannedAsync(chatId, postMessageDto.SenderId);
            if (isBanned)
            {
                return Result.Failure<Guid>(ApplicationErrors.ChatPermission.Denied);
            }

            // Check SendMessage permission for groups and channels
            if (chat.Type == ChatTypes.Group)
            {
                var group = await _groupRepository.GetByIdAsync(chatId);
                if (group != null && !group.IsGroupOwner(postMessageDto.SenderId))
                {
                    // Non-owner needs SendMessage permission
                    var hasPermission = await _permissionRepository.HasUserPermissionAsync(
                        chatId, postMessageDto.SenderId, nameof(ChatPermissionTypes.SendMessage));
                    if (!hasPermission)
                    {
                        return Result.Failure<Guid>(ApplicationErrors.ChatPermission.Denied);
                    }
                }
            }
            else if (chat.Type == ChatTypes.Channel)
            {
                var channel = await _channelRepository.GetByIdAsync(chatId);
                if (channel != null && !channel.IsChannelOwner(postMessageDto.SenderId))
                {
                    // Non-owner needs SendMessage permission
                    var hasPermission = await _permissionRepository.HasUserPermissionAsync(
                        chatId, postMessageDto.SenderId, nameof(ChatPermissionTypes.SendMessage));
                    if (!hasPermission)
                    {
                        return Result.Failure<Guid>(ApplicationErrors.ChatPermission.Denied);
                    }
                }
            }
            else if (chat.Type == ChatTypes.Conversation)
            {
                // For existing conversations, check user-to-user ban
                var conversation = await _conversationRepo.GetByIdAsync(chatId);
                if (conversation != null)
                {
                    // Determine the other user in the conversation
                    var otherUserId = conversation.UserId1 == postMessageDto.SenderId
                        ? conversation.UserId2
                        : conversation.UserId1;

                    // Check if the other user has blocked the sender
                    if (await _userBanRepository.IsUserBannedAsync(otherUserId, postMessageDto.SenderId))
                    {
                        return Result.Failure<Guid>(ApplicationErrors.UserBan.UserBanned);
                    }

                    // Check if the sender has blocked the other user (they need to unblock first)
                    if (await _userBanRepository.IsUserBannedAsync(postMessageDto.SenderId, otherUserId))
                    {
                        return Result.Failure<Guid>(ApplicationErrors.UserBan.CannotMessageBannedUser);
                    }
                }
            }

            var message = new Message
            {
                ChatId = chatId,
                SenderId = postMessageDto.SenderId,
                Content = postMessageDto.Content,
                FileUrl = fileUrl,
                ReplyId = postMessageDto.ReplyId,
                SentAt = DateTime.UtcNow
            };

            await _repo.CreateAsync(message);

            await CreateNotificationsAsync(chatId, message, chat.Type);

            return message.Id;
        }

        public async Task<Result> UpdateAsync(Guid messageId, UpdateMessageDto updateMessageDto, UploadFileRequest? uploadFileRequest, Guid userId)
        {
            var validationResult = await _updateMessageValidator.ValidateAsync(updateMessageDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure<Guid>(ApplicationErrors.Validation.Failed, errors);
            }

            var message = await _repo.GetByIdAsync(messageId);

            if (message is null)
            {
                return Result.Failure(ApplicationErrors.Message.IdNotFound);
            }

            // Allow edit if: sender OR has ManageMessages permission OR is owner
            if (!message.IsMessageSender(userId))
            {
                var canManage = await CanManageMessageAsync(message.ChatId, userId);
                if (!canManage)
                {
                    return Result.Failure(ApplicationErrors.ChatPermission.Denied);
                }
            }

            string? fileUrl = null;
            if (uploadFileRequest is not null &&
                uploadFileRequest.Content != null &&
                uploadFileRequest.FileName != null &&
                uploadFileRequest.ContentType != null)
            {
                var uniqueFileName = $"{Guid.NewGuid()}_{uploadFileRequest.FileName}";
                fileUrl = await _fileStorageService.UploadFileAsync(
                    BucketName,
                    uniqueFileName,
                    uploadFileRequest.Content,
                    uploadFileRequest.ContentType
                );
            }

            if (updateMessageDto.ReplyId is not null)
            {
                if (await _repo.GetByIdAsync((Guid)updateMessageDto.ReplyId) is null)
                {
                    return Result.Failure(ApplicationErrors.Message.IdNotFound);
                }
            }

            if (fileUrl is null)
            {
                fileUrl = message.FileUrl;
            }

            message.FileUrl = fileUrl;
            message.Content = updateMessageDto.Content;
            message.ReplyId = updateMessageDto.ReplyId;

            await _repo.UpdateAsync(message);

            return Result.Success();
        }

        public async Task<Result> DeleteAsync(Guid messageId, Guid userId)
        {
            var message = await _repo.GetByIdAsync(messageId);

            if (message is null)
            {
                return Result.Failure(ApplicationErrors.Message.IdNotFound);
            }

            // Allow delete if: sender OR has ManageMessages permission OR is owner
            if (!message.IsMessageSender(userId))
            {
                var canManage = await CanManageMessageAsync(message.ChatId, userId);
                if (!canManage)
                {
                    return Result.Failure(ApplicationErrors.ChatPermission.Denied);
                }
            }

            await _repo.DeleteAsync(message);

            return Result.Success();
        }

        private async Task<bool> CanManageMessageAsync(Guid chatId, Guid userId)
        {
            var chat = await _chatRepo.GetByIdAsync(chatId);
            if (chat == null) return false;

            // Check if user is owner
            if (chat.Type == ChatTypes.Group)
            {
                var group = await _groupRepository.GetByIdAsync(chatId);
                if (group != null && group.IsGroupOwner(userId))
                    return true;
            }
            else if (chat.Type == ChatTypes.Channel)
            {
                var channel = await _channelRepository.GetByIdAsync(chatId);
                if (channel != null && channel.IsChannelOwner(userId))
                    return true;
            }

            // Check if user has ManageMessages permission
            return await _permissionRepository.HasUserPermissionAsync(
                chatId, userId, nameof(ChatPermissionTypes.ManageMessages));
        }

        private async Task CreateNotificationsAsync(Guid chatId, Message message, ChatTypes chatType)
        {
            var recipientIds = new List<Guid>();

            if (chatType == ChatTypes.Group)
            {
                var group = await _groupRepository.GetByIdAsync(chatId);
                if (group != null)
                {
                    recipientIds = group.Members
                        .Where(m => m.UserId != message.SenderId)
                        .Select(m => m.UserId)
                        .ToList();
                }
            }
            else if (chatType == ChatTypes.Channel)
            {
                var channel = await _channelRepository.GetByIdAsync(chatId);
                if (channel != null)
                {
                    recipientIds = channel.Subscribers
                        .Where(s => s.UserId != message.SenderId)
                        .Select(s => s.UserId)
                        .ToList();
                }
            }
            else if (chatType == ChatTypes.Conversation)
            {
                var conversation = await _conversationRepo.GetByIdAsync(chatId);
                if (conversation != null)
                {
                    if (conversation.UserId1 != message.SenderId)
                        recipientIds.Add(conversation.UserId1);
                    if (conversation.UserId2 != message.SenderId)
                        recipientIds.Add(conversation.UserId2);
                }
            }

            if (recipientIds.Any())
            {
                // Batch create all notifications at once instead of one by one
                var notifications = recipientIds.Select(recipientId => new Notification
                {
                    MessageId = message.Id,
                    ReceiverId = recipientId,
                    IsSeen = false
                }).ToList();

                await _notificationRepository.CreateBatchAsync(notifications);
            }
        }

        private const int MaxPinnedMessages = 50;

        public async Task<Result> PinMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _repo.GetByIdAsync(messageId);
            if (message is null)
            {
                return Result.Failure(ApplicationErrors.Message.IdNotFound);
            }

            if (message.IsPinned)
            {
                return Result.Failure(ApplicationErrors.MessagePinning.AlreadyPinned);
            }

            // Check if user can pin messages
            var canPin = await CanPinMessageAsync(message.ChatId, userId);
            if (!canPin)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            // Check pin limit
            var pinnedCount = await _repo.GetPinnedMessagesCountAsync(message.ChatId);
            if (pinnedCount >= MaxPinnedMessages)
            {
                return Result.Failure(ApplicationErrors.MessagePinning.PinLimitReached);
            }

            message.IsPinned = true;
            message.PinnedAt = DateTimeOffset.UtcNow;
            message.PinnedById = userId;

            await _repo.UpdateAsync(message);

            return Result.Success();
        }

        public async Task<Result> UnpinMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _repo.GetByIdAsync(messageId);
            if (message is null)
            {
                return Result.Failure(ApplicationErrors.Message.IdNotFound);
            }

            if (!message.IsPinned)
            {
                return Result.Failure(ApplicationErrors.MessagePinning.NotPinned);
            }

            // Check if user can unpin messages
            var canPin = await CanPinMessageAsync(message.ChatId, userId);
            if (!canPin)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            message.IsPinned = false;
            message.PinnedAt = null;
            message.PinnedById = null;

            await _repo.UpdateAsync(message);

            return Result.Success();
        }

        public async Task<Result<List<PinnedMessageDto>>> GetPinnedMessagesAsync(Guid chatId, Guid userId)
        {
            var chat = await _chatRepo.GetByIdAsync(chatId);
            if (chat is null)
            {
                return Result.Failure<List<PinnedMessageDto>>(ApplicationErrors.Chat.IdNotFound);
            }

            // Verify user is a participant
            var isParticipant = await IsUserParticipantAsync(chatId, userId, chat.Type);
            if (!isParticipant)
            {
                return Result.Failure<List<PinnedMessageDto>>(ApplicationErrors.ChatPermission.Denied);
            }

            var pinnedMessages = await _repo.GetPinnedMessagesAsync(chatId);

            var dtos = pinnedMessages.Select(m => new PinnedMessageDto
            {
                MessageId = m.Id,
                Content = m.Content,
                FileUrl = m.FileUrl,
                SenderId = m.SenderId,
                SenderUsername = m.Sender?.Username ?? string.Empty,
                SenderAvatarUrl = m.Sender?.AvatarUrl,
                SentAt = m.SentAt,
                PinnedAt = m.PinnedAt,
                PinnedById = m.PinnedById,
                PinnedByUsername = m.PinnedBy?.Username,
                MessageReactions = new List<Models.Reactions.MessageReactionDto>()
            }).ToList();

            return Result.Success(dtos);
        }

        private async Task<bool> CanPinMessageAsync(Guid chatId, Guid userId)
        {
            var chat = await _chatRepo.GetByIdAsync(chatId);
            if (chat == null) return false;

            // For conversations (DMs), both participants can pin
            if (chat.Type == ChatTypes.Conversation)
            {
                var conversation = await _conversationRepo.GetByIdAsync(chatId);
                if (conversation != null)
                {
                    return conversation.UserId1 == userId || conversation.UserId2 == userId;
                }
                return false;
            }

            // For groups/channels, check if owner OR has PinMessages permission
            if (chat.Type == ChatTypes.Group)
            {
                var group = await _groupRepository.GetByIdAsync(chatId);
                if (group != null && group.IsGroupOwner(userId))
                    return true;
            }
            else if (chat.Type == ChatTypes.Channel)
            {
                var channel = await _channelRepository.GetByIdAsync(chatId);
                if (channel != null && channel.IsChannelOwner(userId))
                    return true;
            }

            // Check if user has PinMessages permission
            return await _permissionRepository.HasUserPermissionAsync(
                chatId, userId, nameof(ChatPermissionTypes.PinMessages));
        }

        private async Task<bool> IsUserParticipantAsync(Guid chatId, Guid userId, ChatTypes chatType)
        {
            if (chatType == ChatTypes.Conversation)
            {
                var conversation = await _conversationRepo.GetByIdAsync(chatId);
                return conversation != null &&
                       (conversation.UserId1 == userId || conversation.UserId2 == userId);
            }
            else if (chatType == ChatTypes.Group)
            {
                var group = await _groupRepository.GetByIdAsync(chatId);
                return group != null && group.IsGroupMember(userId);
            }
            else if (chatType == ChatTypes.Channel)
            {
                var channel = await _channelRepository.GetByIdAsync(chatId);
                return channel != null && channel.IsChannelSubscriber(userId);
            }

            return false;
        }

        public async Task<Result<List<Guid>>> MarkMessagesAsSeenAsync(Guid chatId, Guid userId)
        {
            var chat = await _chatRepo.GetByIdAsync(chatId);
            if (chat is null)
            {
                return Result.Failure<List<Guid>>(ApplicationErrors.Chat.IdNotFound);
            }

            // Verify user is a participant
            var isParticipant = await IsUserParticipantAsync(chatId, userId, chat.Type);
            if (!isParticipant)
            {
                return Result.Failure<List<Guid>>(ApplicationErrors.ChatPermission.Denied);
            }

            // Get unseen messages before marking them
            var unseenMessages = await _repo.GetUnseenMessagesInChatAsync(chatId, userId);
            var messageIds = unseenMessages.Select(m => m.Id).ToList();

            if (messageIds.Any())
            {
                // Mark messages as seen - SignalR will broadcast the update
                await _repo.MarkMessagesAsSeenAsync(chatId, userId);

                // Mark notifications as seen in a single batch update (no loop)
                await _notificationRepository.MarkSeenByMessageIdsAsync(messageIds, userId);
            }

            return Result.Success(messageIds);
        }
    }
}
