using Simpchat.Application.Errors;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.File;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.ApiResult;

using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Messages;
using Simpchat.Application.Models.Reactions;
using Simpchat.Application.Models.Users;
using Simpchat.Domain.Entities;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Features
{
    internal class ChatService : IChatService
    {
        private readonly IChatRepository _repo;
        private readonly IMessageRepository _messageRepo;
        private readonly IConversationRepository _conversationRepo;
        private readonly IGroupRepository _groupRepo;
        private readonly IChannelRepository _channelRepo;
        private readonly IUserRepository _userRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IFileStorageService _fileStorageService;
        private readonly IGroupService _groupService;
        private readonly IChatPermissionRepository _chatPermissionRepository;
        private readonly IChatUserPermissionRepository _chatUserPermissionRepository;
        private readonly IChatBanRepository _chatBanRepository;
        private readonly IChannelService _channelService;
        private readonly IConversationService _conversationService;
        private readonly IUserService _userService;
        private readonly IMessageReactionRepository _messageReactionRepo;
        private readonly IPresenceService _presenceService;


        public ChatService(
            IChatRepository chatRepository,
            IMessageRepository messageRepo,
            IConversationRepository conversationRepo,
            IGroupRepository groupRepo,
            IChannelRepository channelRepo,
            IUserRepository userRepo,
            INotificationRepository notificationRepo,
            IFileStorageService fileStorageService,
            IChannelService channelService,
            IConversationService conversationService,
            IUserService userService,
            IChatPermissionRepository chatPermissionRepository,
            IChatUserPermissionRepository chatUserPermissionRepository,
            IGroupService groupService,
            IChatBanRepository chatBanRepository,
            IMessageReactionRepository messageReactionRepo,
            IPresenceService presenceService)
        {
            _repo = chatRepository;
            _messageRepo = messageRepo;
            _conversationRepo = conversationRepo;
            _groupRepo = groupRepo;
            _channelRepo = channelRepo;
            _userRepo = userRepo;
            _notificationRepo = notificationRepo;
            _fileStorageService = fileStorageService;
            _channelService = channelService;
            _conversationService = conversationService;
            _userService = userService;
            _chatPermissionRepository = chatPermissionRepository;
            _chatUserPermissionRepository = chatUserPermissionRepository;
            _groupService = groupService;
            _chatBanRepository = chatBanRepository;
            _messageReactionRepo = messageReactionRepo;
            _presenceService = presenceService;
        }

        public async Task<Result<Guid>> AddUserPermissionAsync(Guid chatId, Guid userId, string permissionName, Guid requesterId)
        {
            var chat = await _repo.GetByIdAsync(chatId);

            if (chat is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.Chat.IdNotFound);
            }

            if (chat.Type == ChatTypes.Group)
            {
                var group = await _groupRepo.GetByIdAsync(chatId);
                if (group is null)
                {
                    return Result.Failure<Guid>(ApplicationErrors.Chat.IdNotFound);
                }

                var canGrantPermission = group.IsGroupOwner(requesterId) ||
                                        await _chatUserPermissionRepository.HasUserPermissionAsync(
                                            chatId, requesterId, nameof(ChatPermissionTypes.ManageUsers));

                if (!canGrantPermission)
                {
                    return Result.Failure<Guid>(ApplicationErrors.ChatPermission.Denied);
                }
            }
            else if (chat.Type == ChatTypes.Channel)
            {
                var channel = await _channelRepo.GetByIdAsync(chatId);
                if (channel is null)
                {
                    return Result.Failure<Guid>(ApplicationErrors.Chat.IdNotFound);
                }

                var canGrantPermission = channel.IsChannelOwner(requesterId) ||
                                        await _chatUserPermissionRepository.HasUserPermissionAsync(
                                            chatId, requesterId, nameof(ChatPermissionTypes.ManageUsers));

                if (!canGrantPermission)
                {
                    return Result.Failure<Guid>(ApplicationErrors.ChatPermission.Denied);
                }
            }

            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.User.IdNotFound);
            }

            var chatPermission = await _chatPermissionRepository.GetByNameAsync(permissionName);

            if (chatPermission is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.ChatPermission.NameNotFound);
            }

            var chatUserPermission = new ChatUserPermission
            {
                Id = Guid.NewGuid(),  // Generate GUID in C# to avoid duplicates
                ChatId = chatId,
                PermissionId = chatPermission.Id,
                UserId = userId
            };

            await _chatUserPermissionRepository.CreateAsync(chatUserPermission);

            return chatUserPermission.Id;
        }

        public async Task<Result<GetByIdChatDto>> GetByIdAsync(Guid chatId, Guid userId)
        {
            var chat = await _repo.GetByIdAsync(chatId);

            if (chat is null)
            {
                return Result.Failure<GetByIdChatDto>(ApplicationErrors.Chat.IdNotFound);
            }

            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure<GetByIdChatDto>(ApplicationErrors.User.IdNotFound);
            }

            var isBanned = await _chatBanRepository.IsUserBannedAsync(chatId, userId);
            if (isBanned)
            {
                return Result.Failure<GetByIdChatDto>(ApplicationErrors.ChatPermission.Denied);
            }

            var participantsCount = 0;
            var participantsOnline = 0;
            var avatarUrl = "";
            var name = "";

            if (chat.Type == ChatTypes.Conversation)
            {
                var conversation = await _conversationRepo.GetByIdAsync(chatId);

                var isParticipated = conversation.UserId1 == userId || conversation.UserId2 == userId;

                if (isParticipated is false)
                {
                    return Result.Failure<GetByIdChatDto>(ApplicationErrors.User.NotParticipatedInChat);
                }

                participantsCount = 2;

                if (_presenceService.IsUserOnline(conversation.User1.Id))
                {
                    participantsOnline++;
                }

                if (_presenceService.IsUserOnline(conversation.User2.Id))
                {
                    participantsOnline++;
                }

                avatarUrl = conversation.UserId1 == userId ? conversation.User2.AvatarUrl : conversation.User1.AvatarUrl;
                name = conversation.UserId1 == userId ? conversation.User2.Username : conversation.User1.Username;
            }
            else if (chat.Type == ChatTypes.Group)
            {
                var group = await _groupRepo.GetByIdAsync(chatId);

                bool isParticipated = group.Members.FirstOrDefault(m => m.UserId == userId) != null;

                participantsCount = group.Members.Count();

                foreach (var groupMember in group.Members)
                {
                    if (_presenceService.IsUserOnline(groupMember.User.Id))
                    {
                        participantsOnline++;
                    }
                }

                avatarUrl = group.AvatarUrl;
                name = group.Name;
            }
            else if (chat.Type == ChatTypes.Channel)
            {
                var channel = await _channelRepo.GetByIdAsync(chatId);

                bool isParticipated = channel.Subscribers.FirstOrDefault(m => m.UserId == userId) != null;

                participantsCount = channel.Subscribers.Count();

                foreach (var channelSubscriber in channel.Subscribers)
                {
                    if (_presenceService.IsUserOnline(channelSubscriber.User.Id))
                    {
                        participantsOnline++;
                    }
                }

                avatarUrl = channel.AvatarUrl;
                name = channel.Name;
            }


            var messagesModels = new List<GetByIdMessageDto>();

            foreach (var message in chat.Messages)
            {
                var messageReactions = await _messageReactionRepo.GetMessageReactionAsync(message.Id);
                var messageReactionModels = messageReactions is not null
                    ? messageReactions
                        .GroupBy(mr => mr.ReactionId)
                        .Select(g => new GetAllMessageReaction
                        {
                            Id = g.Key,
                            Count = g.Count(),
                            ImageUrl = g.First().Reaction.ImageUrl
                        }).ToList()
                    : new List<GetAllMessageReaction>();

                var notificationId = await _notificationRepo.GetIdAsync(message.Id, userId);

                var messageModel = new GetByIdMessageDto
                {
                    MessageId = message.Id,
                    Content = message.Content,
                    FileUrl = message.FileUrl,
                    ReplyId = message.ReplyId,
                    IsSeen = (await _notificationRepo.GetMessageSeenStatusAsync(message.Id)),
                    SenderAvatarUrl = message.Sender.AvatarUrl,
                    SenderUsername = message.Sender.Username,
                    SenderId = message.SenderId,
                    SentAt = message.SentAt,
                    IsNotificated = await _notificationRepo.CheckIsNotSeenAsync(message.Id, userId),
                    NotificationId = notificationId ?? Guid.Empty,
                    MessageReactions = messageReactionModels,
                    IsCurrentUser = message.SenderId == userId
                };

                messagesModels.Add(messageModel);
            }

            var notificationsCount = await _notificationRepo.GetUserChatNotificationsCountAsync(userId, chatId);

            var model = new GetByIdChatDto
            {
                Id = chat.Id,
                AvatarUrl = avatarUrl,
                Messages = messagesModels,
                Name = name,
                NotificationsCount = notificationsCount,
                ParticipantsCount = participantsCount,
                ParticipantsOnline = participantsOnline,
                Type = chat.Type
            };

            return model;
        }

        public async Task<Result<GetByIdChatProfile>> GetProfileAsync(Guid chatId, Guid userId)
        {
            var chat = await _repo.GetByIdAsync(chatId);

            if (chat is null)
            {
                return Result.Failure<GetByIdChatProfile>(ApplicationErrors.Chat.IdNotFound);
            }

            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure<GetByIdChatProfile>(ApplicationErrors.User.IdNotFound);
            }

            var participantsCount = 0;
            var participantsOnline = 0;
            var members = new List<ChatMemberDto>();
            var avatarUrl = "";
            var name = "";
            var description = "";
            var createdAt = DateTimeOffset.UtcNow;
            ChatPrivacyTypes? privacy = null;
            Guid? createdById = null;

            if (chat.Type == ChatTypes.Conversation)
            {
                var conversation = await _conversationRepo.GetByIdAsync(chatId);

                var isParticipated = conversation.UserId1 == userId || conversation.UserId2 == userId;

                if (isParticipated is false)
                {
                    return Result.Failure<GetByIdChatProfile>(ApplicationErrors.User.NotParticipatedInChat);
                }

                participantsCount = 2;

                if (_presenceService.IsUserOnline(conversation.User1.Id))
                {
                    participantsOnline++;
                }

                if (_presenceService.IsUserOnline(conversation.User2.Id))
                {
                    participantsOnline++;
                }

                avatarUrl = conversation.UserId1 == userId ? conversation.User2.AvatarUrl : conversation.User1.AvatarUrl;
                name = conversation.UserId1 == userId ? conversation.User2.Username : conversation.User1.Username;
                description = conversation.UserId1 == userId ? conversation.User2.Description : conversation.User1.Description;
                createdAt = conversation.CreatedAt;

                members.Add(new ChatMemberDto
                {
                    Id = Guid.NewGuid(),
                    UserId = conversation.User1.Id,
                    User = UserResponseDto.ConvertFromDomainObject(conversation.User1, _presenceService),
                    JoinedAt = conversation.CreatedAt.ToString("o"),
                    Role = "member"
                });
                members.Add(new ChatMemberDto
                {
                    Id = Guid.NewGuid(),
                    UserId = conversation.User2.Id,
                    User = UserResponseDto.ConvertFromDomainObject(conversation.User2, _presenceService),
                    JoinedAt = conversation.CreatedAt.ToString("o"),
                    Role = "member"
                });
            }
            else if (chat.Type == ChatTypes.Group)
            {
                var group = await _groupRepo.GetByIdAsync(chatId);

                bool isParticipated = group.Members.FirstOrDefault(m => m.UserId == userId) != null;

                participantsCount = group.Members.Count();
                createdById = group.CreatedById;
                createdAt = chat.CreatedAt;
                privacy = chat.PrivacyType;

                foreach (var groupMember in group.Members)
                {
                    if (_presenceService.IsUserOnline(groupMember.User.Id))
                    {
                        participantsOnline++;
                    }

                    // Determine role: owner = admin, others = member (can be upgraded via permissions)
                    var role = groupMember.UserId == group.CreatedById ? "admin" : "member";

                    members.Add(new ChatMemberDto
                    {
                        Id = groupMember.Id,
                        UserId = groupMember.UserId,
                        User = UserResponseDto.ConvertFromDomainObject(groupMember.User, _presenceService),
                        JoinedAt = groupMember.JoinedAt.ToString("o"),
                        Role = role
                    });
                }

                avatarUrl = group.AvatarUrl;
                name = group.Name;
                description = group.Description;
            }
            else if (chat.Type == ChatTypes.Channel)
            {
                var channel = await _channelRepo.GetByIdAsync(chatId);

                bool isParticipated = channel.Subscribers.FirstOrDefault(m => m.UserId == userId) != null;

                participantsCount = channel.Subscribers.Count();
                createdById = channel.CreatedById;
                createdAt = chat.CreatedAt;
                privacy = chat.PrivacyType;

                foreach (var channelSubscriber in channel.Subscribers)
                {
                    if (_presenceService.IsUserOnline(channelSubscriber.User.Id))
                    {
                        participantsOnline++;
                    }

                    // Determine role: owner = admin, others = member
                    var role = channelSubscriber.UserId == channel.CreatedById ? "admin" : "member";

                    members.Add(new ChatMemberDto
                    {
                        Id = channelSubscriber.Id,
                        UserId = channelSubscriber.UserId,
                        User = UserResponseDto.ConvertFromDomainObject(channelSubscriber.User, _presenceService),
                        JoinedAt = channelSubscriber.SubscribedAt.ToString("o"),
                        Role = role
                    });
                }

                avatarUrl = channel.AvatarUrl;
                name = channel.Name;
                description = channel.Description;
            }

            var model = new GetByIdChatProfile
            {
                Id = chatId,
                Type = chat.Type,
                Privacy = privacy,
                CreatedAt = createdAt,
                Description = description,
                AvatarUrl = avatarUrl,
                Name = name,
                ParticipantsCount = participantsCount,
                ParticipantsOnline = participantsOnline,
                Members = members
            };

            return model;
        }

        public async Task<Result<List<UserChatResponseDto>>> GetUserChatsAsync(Guid userId)
        {
            var conversationsApiResult = await _conversationService.GetUserConversationsAsync(userId);
            var groupsApiResult = await _groupService.GetUserParticipatedAsync(userId);
            var channelResult = await _channelService.GetUserSubscribedAsync(userId);

            var merged = new List<UserChatResponseDto>();

            merged.AddRange(conversationsApiResult.Value);
            merged.AddRange(groupsApiResult.Value);
            merged.AddRange(channelResult.Value);

            merged = merged.OrderByDescending(m => (DateTimeOffset?)m.LastMessage.SentAt ?? DateTimeOffset.MinValue).ToList();

            return merged;
        }

        public async Task<Result<List<SearchChatResponseDto>>> SearchAsync(string term, Guid userId)
        {
            var filteredUsersResult = await _userService.SearchAsync(term, userId);
            var filteredGroupsResult = await _groupService.SearchAsync(term);
            var filteredChannelsResult = await _channelService.SearchAsync(term);

            var merged = new List<SearchChatResponseDto>();

            if (filteredUsersResult.IsSuccess is false)
            {
                return Result.Failure<List<SearchChatResponseDto>>(filteredChannelsResult.Error);
            }

            if (filteredGroupsResult.IsSuccess is false)
            {
                return Result.Failure<List<SearchChatResponseDto>>(filteredGroupsResult.Error);
            }

            if (filteredChannelsResult.IsSuccess is false)
            {
                return Result.Failure<List<SearchChatResponseDto>>(filteredChannelsResult.Error);
            }

            merged.AddRange(filteredUsersResult.Value);
            merged.AddRange(filteredGroupsResult.Value);
            merged.AddRange(filteredChannelsResult.Value);

            merged = merged.OrderBy(m => m.EntityId).ToList();

            return merged;
        }

        public async Task<Result> UpdatePrivacyTypeAsync(Guid chatId, ChatPrivacyTypes chatPrivacyType, Guid userId)
        {
            var chat = await _repo.GetByIdAsync(chatId);

            if (chat is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            chat.PrivacyType = chatPrivacyType;

            await _repo.UpdateAsync(chat);

            return Result.Success();
        }
    }
}
