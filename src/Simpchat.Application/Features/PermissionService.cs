using Simpchat.Application.Errors;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.Permissions;
using Simpchat.Domain.Entities;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Features
{
    public class PermissionService : IPermissionService
    {
        private readonly IChatUserPermissionRepository _permissionRepository;
        private readonly IChatPermissionRepository _chatPermissionRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IUserRepository _userRepository;

        public PermissionService(
            IChatUserPermissionRepository permissionRepository,
            IChatPermissionRepository chatPermissionRepository,
            IChatRepository chatRepository,
            IGroupRepository groupRepository,
            IChannelRepository channelRepository,
            IUserRepository userRepository)
        {
            _permissionRepository = permissionRepository;
            _chatPermissionRepository = chatPermissionRepository;
            _chatRepository = chatRepository;
            _groupRepository = groupRepository;
            _channelRepository = channelRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<Guid>> GrantPermissionAsync(GrantPermissionDto dto, Guid requesterId)
        {
            var chat = await _chatRepository.GetByIdAsync(dto.ChatId);
            if (chat is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.Chat.IdNotFound);
            }

            var targetUser = await _userRepository.GetByIdAsync(dto.UserId);
            if (targetUser is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.User.IdNotFound);
            }

            var canGrant = await ValidateCanGrantPermissionAsync(chat, dto.ChatId, requesterId);
            if (!canGrant)
            {
                return Result.Failure<Guid>(ApplicationErrors.ChatPermission.Denied);
            }

            var permission = await _chatPermissionRepository.GetByNameAsync(dto.PermissionName);
            if (permission is null)
            {
                return Result.Failure<Guid>(ApplicationErrors.ChatPermission.NameNotFound);
            }

            var existingPermission = await _permissionRepository.GetByUserChatPermissionAsync(
                dto.ChatId, dto.UserId, permission.Id);

            if (existingPermission is not null)
            {
                return Result.Failure<Guid>(new Error(
                    "Permission.AlreadyExists",
                    $"User already has {dto.PermissionName} permission in this chat"));
            }

            var chatUserPermission = new ChatUserPermission
            {
                ChatId = dto.ChatId,
                UserId = dto.UserId,
                PermissionId = permission.Id
            };

            await _permissionRepository.CreateAsync(chatUserPermission);

            return chatUserPermission.Id;
        }

        public async Task<Result> RevokePermissionAsync(RevokePermissionDto dto, Guid requesterId)
        {
            var chat = await _chatRepository.GetByIdAsync(dto.ChatId);
            if (chat is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            var targetUser = await _userRepository.GetByIdAsync(dto.UserId);
            if (targetUser is null)
            {
                return Result.Failure(ApplicationErrors.User.IdNotFound);
            }

            var canRevoke = await ValidateCanGrantPermissionAsync(chat, dto.ChatId, requesterId);
            if (!canRevoke)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            var permission = await _chatPermissionRepository.GetByNameAsync(dto.PermissionName);
            if (permission is null)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.NameNotFound);
            }

            var existingPermission = await _permissionRepository.GetByUserChatPermissionAsync(
                dto.ChatId, dto.UserId, permission.Id);

            if (existingPermission is null)
            {
                return Result.Failure(new Error(
                    "Permission.NotFound",
                    $"User does not have {dto.PermissionName} permission in this chat"));
            }

            await _permissionRepository.DeleteAsync(existingPermission);

            return Result.Success();
        }

        public async Task<Result<UserChatPermissionsResponseDto>> GetUserPermissionsAsync(
            Guid chatId, Guid userId, Guid requesterId)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);
            if (chat is null)
            {
                return Result.Failure<UserChatPermissionsResponseDto>(ApplicationErrors.Chat.IdNotFound);
            }

            var targetUser = await _userRepository.GetByIdAsync(userId);
            if (targetUser is null)
            {
                return Result.Failure<UserChatPermissionsResponseDto>(ApplicationErrors.User.IdNotFound);
            }

            // Allow users to query their own permissions, or check if they have ManageUsers permission
            var isQueryingOwnPermissions = requesterId == userId;
            var canView = isQueryingOwnPermissions || await ValidateCanViewPermissionsAsync(chat, chatId, requesterId);
            if (!canView)
            {
                return Result.Failure<UserChatPermissionsResponseDto>(ApplicationErrors.ChatPermission.Denied);
            }

            var userPermissions = await _permissionRepository.GetUserChatPermissionsAsync(chatId, userId);

            var response = new UserChatPermissionsResponseDto
            {
                UserId = userId,
                Username = targetUser.Username,
                ChatId = chatId,
                Permissions = userPermissions
                    .Select(p => new UserPermissionResponseDto
                    {
                        PermissionId = p.Permission.Id,
                        PermissionName = p.Permission.Name
                    })
                    .ToList()
            };

            return response;
        }

        public async Task<Result<List<UserChatPermissionsResponseDto>>> GetAllChatPermissionsAsync(
            Guid chatId, Guid requesterId)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);
            if (chat is null)
            {
                return Result.Failure<List<UserChatPermissionsResponseDto>>(ApplicationErrors.Chat.IdNotFound);
            }

            var canView = await ValidateCanViewPermissionsAsync(chat, chatId, requesterId);
            if (!canView)
            {
                return Result.Failure<List<UserChatPermissionsResponseDto>>(ApplicationErrors.ChatPermission.Denied);
            }

            var allPermissions = await _permissionRepository.GetChatPermissionsAsync(chatId);

            var groupedPermissions = allPermissions
                .GroupBy(p => p.UserId)
                .Select(g => new UserChatPermissionsResponseDto
                {
                    UserId = g.Key,
                    Username = g.First().User.Username,
                    ChatId = chatId,
                    Permissions = g
                        .Select(p => new UserPermissionResponseDto
                        {
                            PermissionId = p.Permission.Id,
                            PermissionName = p.Permission.Name
                        })
                        .ToList()
                })
                .ToList();

            return groupedPermissions;
        }

        public async Task<Result> RevokeAllPermissionsAsync(Guid chatId, Guid userId, Guid requesterId)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);
            if (chat is null)
            {
                return Result.Failure(ApplicationErrors.Chat.IdNotFound);
            }

            var targetUser = await _userRepository.GetByIdAsync(userId);
            if (targetUser is null)
            {
                return Result.Failure(ApplicationErrors.User.IdNotFound);
            }

            var canRevoke = await ValidateCanGrantPermissionAsync(chat, chatId, requesterId);
            if (!canRevoke)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            var userPermissions = await _permissionRepository.GetUserChatPermissionsAsync(chatId, userId);

            foreach (var permission in userPermissions)
            {
                await _permissionRepository.DeleteAsync(permission);
            }

            return Result.Success();
        }

        private async Task<bool> ValidateCanGrantPermissionAsync(Chat chat, Guid chatId, Guid requesterId)
        {
            if (chat.Type == ChatTypes.Group)
            {
                var group = await _groupRepository.GetByIdAsync(chatId);
                if (group is null)
                    return false;

                return group.IsGroupOwner(requesterId) ||
                       await _permissionRepository.HasUserPermissionAsync(
                           chatId, requesterId, nameof(ChatPermissionTypes.ManageUsers));
            }

            if (chat.Type == ChatTypes.Channel)
            {
                var channel = await _channelRepository.GetByIdAsync(chatId);
                if (channel is null)
                    return false;

                return channel.IsChannelOwner(requesterId) ||
                       await _permissionRepository.HasUserPermissionAsync(
                           chatId, requesterId, nameof(ChatPermissionTypes.ManageUsers));
            }

            return false;
        }

        private async Task<bool> ValidateCanViewPermissionsAsync(Chat chat, Guid chatId, Guid requesterId)
        {
            if (chat.Type == ChatTypes.Group)
            {
                var group = await _groupRepository.GetByIdAsync(chatId);
                if (group is null)
                    return false;

                return group.IsGroupOwner(requesterId) ||
                       await _permissionRepository.HasUserPermissionAsync(
                           chatId, requesterId, nameof(ChatPermissionTypes.ManageUsers));
            }

            if (chat.Type == ChatTypes.Channel)
            {
                var channel = await _channelRepository.GetByIdAsync(chatId);
                if (channel is null)
                    return false;

                return channel.IsChannelOwner(requesterId) ||
                       await _permissionRepository.HasUserPermissionAsync(
                           chatId, requesterId, nameof(ChatPermissionTypes.ManageUsers));
            }

            return false;
        }
    }
}
