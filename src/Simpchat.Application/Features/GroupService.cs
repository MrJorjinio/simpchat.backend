using FluentValidation;
using Simpchat.Application.Common.Pagination;
using Simpchat.Application.Errors;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.File;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.ApiResult;

using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Files;
using Simpchat.Application.Models.Messages;
using Simpchat.Domain.Entities;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Features
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IChatRepository _chatRepo;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationRepository _notificationRepo;
        private readonly IMessageRepository _messageRepo;
        private readonly IValidator<UpdateChatDto> _updateValidator;
        private readonly IValidator<PostChatDto> _createValidator;
        private readonly IChatUserPermissionRepository _chatUserPermissionRepository;
        private readonly IChatPermissionRepository _chatPermissionRepository;
        private readonly IChatBanRepository _chatBanRepo;
        private readonly IChatHubService _chatHubService;
        private readonly IRealTimeNotificationService _realTimeNotificationService;
        private const string BucketName = "groups-avatars";

        // Default permissions for new group members
        private static readonly string[] DefaultGroupPermissions = new[]
        {
            nameof(ChatPermissionTypes.SendMessage),
            nameof(ChatPermissionTypes.AddUsers)
        };

        public GroupService(
            IGroupRepository repo,
            IUserRepository userRepo,
            IChatRepository chatRepo,
            IFileStorageService fileStorageService,
            INotificationRepository notificationRepository,
            IMessageRepository messageRepository,
            IValidator<UpdateChatDto> updateValidator,
            IValidator<PostChatDto> createValidator,
            IChatUserPermissionRepository chatUserPermissionRepository,
            IChatPermissionRepository chatPermissionRepository,
            IChatBanRepository chatBanRepository,
            IChatHubService chatHubService,
            IRealTimeNotificationService realTimeNotificationService)
        {
            _repo = repo;
            _userRepo = userRepo;
            _chatRepo = chatRepo;
            _fileStorageService = fileStorageService;
            _notificationRepo = notificationRepository;
            _messageRepo = messageRepository;
            _updateValidator = updateValidator;
            _createValidator = createValidator;
            _chatUserPermissionRepository = chatUserPermissionRepository;
            _chatPermissionRepository = chatPermissionRepository;
            _chatBanRepo = chatBanRepository;
            _chatHubService = chatHubService;
            _realTimeNotificationService = realTimeNotificationService;
        }

        public async Task<Result> AddMemberAsync(Guid groupId, Guid userId, Guid requesterId)
        {
            var group = await _repo.GetByIdAsync(groupId);

            if (group is null)
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);

            // Check if requester is a member of the group
            if (!group.IsGroupMember(requesterId))
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            // Check if requester has permission to add users (owner or has AddUsers permission)
            var canAddUsers = group.IsGroupOwner(requesterId) ||
                              await _chatUserPermissionRepository.HasUserPermissionAsync(
                                  groupId, requesterId, nameof(ChatPermissionTypes.AddUsers));

            if (!canAddUsers)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
                return Result.Failure(ApplicationErrors.User.IdNotFound);

            // Check if user is banned from this group
            if (await _chatBanRepo.IsUserBannedAsync(groupId, userId))
            {
                return Result.Failure(ApplicationErrors.ChatBan.UserBanned);
            }

            if (group.IsGroupMember(userId))
            {
                return Result.Failure(ApplicationErrors.User.NotParticipatedInChat);
            }

            await _repo.AddMemberAsync(userId, groupId);

            // Grant default permissions to the new member
            await GrantDefaultPermissionsAsync(groupId, userId);

            // Notify the user via SignalR that they've been added to the group
            await _chatHubService.NotifyUserAddedToChatAsync(
                userId,
                groupId,
                group.Name,
                "group",
                group.AvatarUrl
            );

            return Result.Success();
        }

        public async Task<Result> JoinGroupAsync(Guid groupId, Guid userId)
        {
            var group = await _repo.GetByIdAsync(groupId);

            if (group is null)
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);

            var chat = await _chatRepo.GetByIdAsync(groupId);

            if (chat is null)
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);

            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
                return Result.Failure(ApplicationErrors.User.IdNotFound);

            // Check if user is banned from this group
            if (await _chatBanRepo.IsUserBannedAsync(groupId, userId))
            {
                return Result.Failure(ApplicationErrors.ChatBan.UserBanned);
            }

            if (chat.PrivacyType == ChatPrivacyTypes.Private)
                return Result.Failure(new Error("Group.Private", "Cannot join private group"));

            if (group.IsGroupMember(userId))
                return Result.Failure(ApplicationErrors.User.NotParticipatedInChat);

            await _repo.AddMemberAsync(userId, groupId);

            // Grant default permissions to the new member
            await GrantDefaultPermissionsAsync(groupId, userId);

            return Result.Success();
        }

        /// <summary>
        /// Grants default permissions (SendMessage, AddUsers) to a new group member.
        /// </summary>
        private async Task GrantDefaultPermissionsAsync(Guid groupId, Guid userId)
        {
            foreach (var permissionName in DefaultGroupPermissions)
            {
                try
                {
                    var permission = await _chatPermissionRepository.GetByNameAsync(permissionName);
                    if (permission != null)
                    {
                        // Check if user already has this permission
                        var existing = await _chatUserPermissionRepository.GetByUserChatPermissionAsync(
                            groupId, userId, permission.Id);

                        if (existing == null)
                        {
                            var chatUserPermission = new ChatUserPermission
                            {
                                Id = Guid.NewGuid(),
                                ChatId = groupId,
                                UserId = userId,
                                PermissionId = permission.Id
                            };
                            await _chatUserPermissionRepository.CreateAsync(chatUserPermission);
                        }
                    }
                }
                catch
                {
                    // Silently ignore if permission doesn't exist or other errors
                    // This ensures the member is still added even if permissions fail
                }
            }
        }

        public async Task<Result<Guid>> CreateAsync(PostChatDto groupPostDto, UploadFileRequest? avatar)
        {
            var validationResult = await _createValidator.ValidateAsync(groupPostDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure<Guid>(ApplicationErrors.Validation.Failed, errors);
            }

            var user = await _userRepo.GetByIdAsync(groupPostDto.OwnerId);

            if (user is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.User.IdNotFound);
            }

            var chat = new Chat
            {
                Type = ChatTypes.Group,
                PrivacyType = groupPostDto.PrivacyType
            };

            var chatId = await _chatRepo.CreateAsync(chat);

            var group = new Group
            {
                Id = chatId,
                CreatedById = user.Id,
                Description = groupPostDto.Description,
                Name = groupPostDto.Name,
                Members = new List<GroupMember>
                {
                    new GroupMember{
                        Id = Guid.NewGuid(),  // FIX: Explicitly generate Id to avoid duplicate key errors
                        UserId = user.Id
                    }
                }
            };

            if (avatar is not null)
            {
                if (avatar.FileName != null && avatar.Content != null && avatar.ContentType != null)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{avatar.FileName}";
                    group.AvatarUrl = await _fileStorageService.UploadFileAsync(BucketName, uniqueFileName, avatar.Content, avatar.ContentType);
                }
            }

            await _repo.CreateAsync(group);

            return group.Id;
        }

        public async Task<Result> DeleteAsync(Guid groupId, Guid userId)
        {
            var group = await _repo.GetByIdAsync(groupId);

            if (group is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            var canDelete = group.IsGroupOwner(userId) ||
                            await _chatUserPermissionRepository.HasUserPermissionAsync(
                                groupId, userId, nameof(ChatPermissionTypes.ManageChatInfo));

            if (!canDelete)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            // Get member IDs before deletion to notify them
            var memberIds = group.Members?.Select(m => m.UserId).ToList() ?? new List<Guid>();

            await _repo.DeleteAsync(group);

            // Notify all members that the chat was deleted
            if (memberIds.Count > 0)
            {
                await _realTimeNotificationService.NotifyChatDeletedAsync(groupId, memberIds);
            }

            return Result.Success();
        }

        public async Task<Result> DeleteMemberAsync(Guid userId, Guid groupId, Guid requesterId)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure(ApplicationErrors.User.IdNotFound);
            }

            var chat = await _chatRepo.GetByIdAsync(groupId);

            if (chat is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            if (chat.Type != ChatTypes.Group)
            {
                return Result.Failure(ApplicationErrors.Chat.NotValidChatType);
            }

            var group = await _repo.GetByIdAsync(groupId);
            if (group is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            var isRemovingByOther = userId != requesterId;
            if (isRemovingByOther)
            {
                var canManage = group.CanManageChat(requesterId) ||
                                await _chatUserPermissionRepository.HasUserPermissionAsync(
                                    groupId, requesterId, nameof(ChatPermissionTypes.ManageUsers));

                if (!canManage)
                {
                    return Result.Failure(ApplicationErrors.ChatPermission.Denied);
                }
            }

            var groupMember = new GroupMember
            {
                GroupId = groupId,
                UserId = userId
            };

            await _repo.DeleteMemberAsync(groupMember);

            // Notify the user if they were removed by someone else (not leaving voluntarily)
            if (isRemovingByOther)
            {
                await _realTimeNotificationService.NotifyUserRemovedFromChatAsync(groupId, userId);
            }

            return Result.Success();
        }

        public async Task<Result<List<SearchChatResponseDto>?>> SearchAsync(string searchTerm)
        {
            var results = await _repo.SearchAsync(searchTerm);

            var modeledResults = new List<SearchChatResponseDto>();

            foreach (var group in results)
            {
                if (group.Chat != null && group.Chat.PrivacyType == ChatPrivacyTypes.Public)
                {
                    modeledResults.Add(new SearchChatResponseDto
                    {
                        EntityId = group.Id,
                        ChatId = group.Id,
                        AvatarUrl = group.AvatarUrl,
                        DisplayName = group.Name,
                        ChatType = ChatTypes.Group
                    });
                }
            }

            return modeledResults;
        }

        public async Task<Result<PaginationResult<SearchChatResponseDto>>> SearchPaginatedAsync(string searchTerm, int page, int pageSize)
        {
            var (groups, totalCount) = await _repo.SearchPaginatedAsync(searchTerm, page, pageSize);

            var modeledResults = groups.Select(group => new SearchChatResponseDto
            {
                EntityId = group.Id,
                ChatId = group.Id,
                AvatarUrl = group.AvatarUrl,
                DisplayName = group.Name,
                ChatType = ChatTypes.Group
            }).ToList();

            return new PaginationResult<SearchChatResponseDto>
            {
                Data = modeledResults,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Result> UpdateAsync(Guid groupId, UpdateChatDto updateChatDto, UploadFileRequest? avatar, Guid userId)
        {
            var validationResult = await _updateValidator.ValidateAsync(updateChatDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                  .GroupBy(e => e.PropertyName)
                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result.Failure<Guid>(ApplicationErrors.Validation.Failed, errors);
            }

            var group = await _repo.GetByIdAsync(groupId);

            if (group is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            var canUpdate = group.IsGroupOwner(userId) ||
                            await _chatUserPermissionRepository.HasUserPermissionAsync(
                                groupId, userId, nameof(ChatPermissionTypes.ManageChatInfo));

            if (!canUpdate)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            group.Name = updateChatDto.Name;
            group.Description = updateChatDto.Description;

            if (avatar is not null)
            {
                if (avatar.FileName != null && avatar.Content != null && avatar.ContentType != null)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{avatar.FileName}";
                    group.AvatarUrl = await _fileStorageService.UploadFileAsync(BucketName, uniqueFileName, avatar.Content, avatar.ContentType);
                }
            }

            await _repo.UpdateAsync(group);

            return Result.Success();
        }

        public async Task<Result<List<UserChatResponseDto>>> GetUserParticipatedAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure<List<UserChatResponseDto>>(ApplicationErrors.User.IdNotFound);
            }

            var groups = await _repo.GetUserParticipatedGroupsAsync(userId);

            var modeledGroups = new List<UserChatResponseDto>();

            foreach (var group in groups)
            {
                var notificationsCount = await _notificationRepo.GetUserChatNotificationsCountAsync(userId, group.Id);
                var lastMessage = await _messageRepo.GetLastMessageAsync(group.Id);
                var lastUserSendedMessage = await _messageRepo.GetUserLastSendedMessageAsync(userId, group.Id);

                var modeledGroup = new UserChatResponseDto
                {
                    Id = group.Id,
                    AvatarUrl = group.AvatarUrl,
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
                    Name = group.Name,
                    NotificationsCount = notificationsCount,
                    Type = ChatTypes.Group,
                    UserLastMessage = lastUserSendedMessage?.SentAt,
                    ParticipantsCount = group.Members?.Count ?? 0
                };

                modeledGroups.Add(modeledGroup);
            }

            modeledGroups = modeledGroups.OrderByDescending(mg => (DateTimeOffset?)mg.LastMessage.SentAt ?? DateTimeOffset.MinValue).ToList();

            return modeledGroups;
        }
    }
}
