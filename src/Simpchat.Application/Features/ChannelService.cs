using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Simpchat.Application.Errors;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.File;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Files;
using Simpchat.Application.Models.Messages;
using Simpchat.Domain.Entities;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Features
{
    public class ChannelService : IChannelService
    {
        private readonly IChannelRepository _repo;
        private readonly IChannelSubscriberRepository _channelSubscriberRepo;
        private readonly IUserRepository _userRepo;
        private readonly IChatRepository _chatRepo;
        private readonly IChatPermissionRepository _chatPermissionRepository;
        private readonly IChatUserPermissionRepository _chatUserPermissionRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationRepository _notificationRepo;
        private readonly IMessageRepository _messageRepo;
        private readonly IValidator<UpdateChatDto> _updateValidator;
        private readonly IValidator<PostChatDto> _createValidator;
        private const string BucketName = "channels-avatars";

        public ChannelService(
            IChannelRepository repo,
            IUserRepository userRepo,
            IChatRepository chatRepo,
            IFileStorageService fileStorageService,
            INotificationRepository notificationRepository,
            IMessageRepository messageRepository,
            IValidator<UpdateChatDto> updateValidator,
            IValidator<PostChatDto> createValidator,
            IChannelSubscriberRepository channelSubscriberRepo,
            IChatPermissionRepository chatPermissionRepository,
            IChatUserPermissionRepository chatUserPermissionRepository)
        {
            _repo = repo;
            _userRepo = userRepo;
            _chatRepo = chatRepo;
            _fileStorageService = fileStorageService;
            _notificationRepo = notificationRepository;
            _messageRepo = messageRepository;
            _updateValidator = updateValidator;
            _createValidator = createValidator;
            _channelSubscriberRepo = channelSubscriberRepo;
            _chatPermissionRepository = chatPermissionRepository;
            _chatUserPermissionRepository = chatUserPermissionRepository;
        }

        public async Task<Result> AddSubscriberAsync(Guid channelId, Guid userId)
        {
            var channel = await _repo.GetByIdAsync(channelId);

            if (channel is null)
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);

            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
                return Result.Failure(ApplicationErrors.User.IdNotFound);

            if (channel.IsChannelSubscriber(userId))
            {
                return Result.Failure(ApplicationErrors.User.NotParticipatedInChat);
            }

            var channelSubscriber = new ChannelSubscriber
            {
                Id = Guid.NewGuid(),  // FIX: Explicitly generate Id
                UserId = user.Id,
                ChannelId = channelId,
            };

            await _channelSubscriberRepo.CreateAsync(channelSubscriber);

            return Result.Success();
        }

        public async Task<Result> JoinChannelAsync(Guid channelId, Guid userId)
        {
            var channel = await _repo.GetByIdAsync(channelId);

            if (channel is null)
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);

            var chat = await _chatRepo.GetByIdAsync(channelId);

            if (chat is null)
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);

            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
                return Result.Failure(ApplicationErrors.User.IdNotFound);

            if (chat.PrivacyType == ChatPrivacyTypes.Private)
                return Result.Failure(new Error("Channel.Private", "Cannot join private channel"));

            if (channel.IsChannelSubscriber(userId))
                return Result.Failure(ApplicationErrors.User.NotParticipatedInChat);

            var channelSubscriber = new ChannelSubscriber
            {
                Id = Guid.NewGuid(),  // FIX: Explicitly generate Id
                UserId = user.Id,
                ChannelId = channelId,
            };

            await _channelSubscriberRepo.CreateAsync(channelSubscriber);

            return Result.Success();
        }

        public async Task<Result<Guid>> CreateAsync(PostChatDto chatPostDto, UploadFileRequest? avatar)
        {
            var validationResult = await _createValidator.ValidateAsync(chatPostDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure<Guid>(ApplicationErrors.Validation.Failed, errors);
            }

            var user = await _userRepo.GetByIdAsync(chatPostDto.OwnerId);

            if (user is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.User.IdNotFound);
            }

            var chat = new Chat
            {
                Type = ChatTypes.Channel,
                PrivacyType = chatPostDto.PrivacyType
            };

            var chatId = await _chatRepo.CreateAsync(chat);

            var channel = new Channel
            {
                Id = chatId,
                CreatedById = user.Id,
                Description = chatPostDto.Description,
                Name = chatPostDto.Name,
                Subscribers = new List<ChannelSubscriber>
                {
                    new ChannelSubscriber{
                        Id = Guid.NewGuid(),  // FIX: Explicitly generate Id
                        UserId = user.Id
                    }
                }
            };

            if (avatar is not null)
            {
                if (avatar.FileName != null && avatar.Content != null && avatar.ContentType != null)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{avatar.FileName}";
                    channel.AvatarUrl = await _fileStorageService.UploadFileAsync(BucketName, uniqueFileName, avatar.Content, avatar.ContentType);
                }
            }

            await _repo.CreateAsync(channel);

            return chatId;
        }

        public async Task<Result> DeleteAsync(Guid channelId, Guid userId)
        {
            var channel = await _repo.GetByIdAsync(channelId);

            if (channel is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            var canDelete = channel.IsChannelOwner(userId) ||
                            await _chatUserPermissionRepository.HasUserPermissionAsync(
                                channelId, userId, nameof(ChatPermissionTypes.ManageChatInfo));

            if (!canDelete)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            await _repo.DeleteAsync(channel);

            return Result.Success();
        }

        public async Task<Result> DeleteSubscriberAsync(Guid userId, Guid channelId, Guid requesterId)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure(ApplicationErrors.User.IdNotFound);
            }

            var chat = await _chatRepo.GetByIdAsync(channelId);

            if (chat is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            if (chat.Type != ChatTypes.Channel)
            {
                return Result.Failure(ApplicationErrors.Chat.NotValidChatType);
            }

            var channel = await _repo.GetByIdAsync(channelId);
            if (channel is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            if (userId != requesterId)
            {
                var canManage = channel.IsChannelOwner(requesterId) ||
                                await _chatUserPermissionRepository.HasUserPermissionAsync(
                                    channelId, requesterId, nameof(ChatPermissionTypes.ManageUsers));

                if (!canManage)
                {
                    return Result.Failure(ApplicationErrors.ChatPermission.Denied);
                }
            }

            var channelSubscriber = new ChannelSubscriber
            {
                Id = Guid.NewGuid(),  // FIX: Explicitly generate Id
                ChannelId = channelId,
                UserId = userId
            };

            await _channelSubscriberRepo.DeleteAsync(channelSubscriber);

            return Result.Success();
        }

        public async Task<Result<List<SearchChatResponseDto>?>> SearchAsync(string searchTerm)
        {
            var results = await _repo.SearchAsync(searchTerm);

            var modeledResults = new List<SearchChatResponseDto>();

            // Use the included Chat entity to avoid N+1 queries
            foreach (var channel in results)
            {
                // Only return public channels in search results
                if (channel.Chat != null && channel.Chat.PrivacyType == ChatPrivacyTypes.Public)
                {
                    modeledResults.Add(new SearchChatResponseDto
                    {
                        EntityId = channel.Id,
                        ChatId = channel.Id,
                        AvatarUrl = channel.AvatarUrl,
                        DisplayName = channel.Name,
                        ChatType = ChatTypes.Channel
                    });
                }
            }

            return modeledResults;
        }

        public async Task<Result> UpdateAsync(Guid channelId, UpdateChatDto updateChatDto, UploadFileRequest? avatar, Guid userId)
        {
            var validationResult = await _updateValidator.ValidateAsync(updateChatDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure<Guid>(ApplicationErrors.Validation.Failed, errors);
            }

            var channel = await _repo.GetByIdAsync(channelId);

            if (channel is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            var canUpdate = channel.IsChannelOwner(userId) ||
                            await _chatUserPermissionRepository.HasUserPermissionAsync(
                                channelId, userId, nameof(ChatPermissionTypes.ManageChatInfo));

            if (!canUpdate)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            channel.Name = updateChatDto.Name;
            channel.Description = updateChatDto.Description;

            if (avatar is not null)
            {
                if (avatar.FileName != null && avatar.Content != null && avatar.ContentType != null)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{avatar.FileName}";
                    channel.AvatarUrl = await _fileStorageService.UploadFileAsync(BucketName, uniqueFileName, avatar.Content,avatar.ContentType);
                }
            }

            await _repo.UpdateAsync(channel);

            return Result.Success();
        }

        public async Task<Result<List<UserChatResponseDto>>> GetUserSubscribedAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure<List<UserChatResponseDto>>(ApplicationErrors.User.IdNotFound);
            }

            var channels = await _repo.GetUserSubscribedChannelsAsync(userId);

            var modeledChannels = new List<UserChatResponseDto>();

            foreach (var channel in channels)
            {
                var notificationsCount = await _notificationRepo.GetUserChatNotificationsCountAsync(userId, channel.Id);
                var lastMessage = await _messageRepo.GetLastMessageAsync(channel.Id);
                var lastUserSendedMessage = await _messageRepo.GetUserLastSendedMessageAsync(userId, channel.Id);

                var modeledChannel = new UserChatResponseDto
                {
                    Id = channel.Id,
                    AvatarUrl = channel.AvatarUrl,
                    LastMessage = new LastMessageResponseDto
                    {
                        Content = lastMessage?.Content,
                        FileUrl = lastMessage?.FileUrl,
                        SenderUsername = lastMessage?.Sender.Username,
                        SentAt = lastMessage?.SentAt
                    },
                    Name = channel.Name,
                    NotificationsCount = notificationsCount,
                    Type = ChatTypes.Channel,
                    UserLastMessage = lastUserSendedMessage?.SentAt
                };

                modeledChannels.Add(modeledChannel);
            }

            modeledChannels = modeledChannels.OrderByDescending(mc => (DateTimeOffset?)mc.LastMessage.SentAt ?? DateTimeOffset.MinValue).ToList();

            return modeledChannels;
        }
    }
}
