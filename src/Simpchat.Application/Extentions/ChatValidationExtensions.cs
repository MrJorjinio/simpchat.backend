using Simpchat.Application.Errors;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Domain.Entities;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Extentions
{
    public static class ChatValidationExtensions
    {
        public static async Task<bool> IsUserBannedAsync(
            this IChatBanRepository chatBanRepository,
            Guid chatId,
            Guid userId)
        {
            try
            {
                var ban = await chatBanRepository.GetIdAsync(chatId, userId);
                return ban != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsConversationParticipant(
            this Conversation conversation,
            Guid userId)
        {
            return conversation.UserId1 == userId || conversation.UserId2 == userId;
        }

        public static bool IsGroupMember(
            this Group group,
            Guid userId)
        {
            return group.Members.Any(m => m.UserId == userId);
        }

        public static bool IsChannelSubscriber(
            this Channel channel,
            Guid userId)
        {
            return channel.Subscribers.Any(s => s.UserId == userId);
        }

        public static bool IsGroupOwner(
            this Group group,
            Guid userId)
        {
            return group.CreatedById == userId;
        }

        public static bool IsChannelOwner(
            this Channel channel,
            Guid userId)
        {
            return channel.CreatedById == userId;
        }

        public static bool IsMessageSender(
            this Message message,
            Guid userId)
        {
            return message.SenderId == userId;
        }

        public static bool IsParticipant(
            this Chat chat,
            Guid userId,
            Conversation? conversation = null,
            Group? group = null,
            Channel? channel = null)
        {
            return chat.Type switch
            {
                ChatTypes.Conversation => conversation?.IsConversationParticipant(userId) ?? false,
                ChatTypes.Group => group?.IsGroupMember(userId) ?? false,
                ChatTypes.Channel => channel?.IsChannelSubscriber(userId) ?? false,
                _ => false
            };
        }

        public static bool CanManageChat(
            this Group group,
            Guid userId)
        {
            return group.IsGroupOwner(userId);
        }

        public static bool CanManageChat(
            this Channel channel,
            Guid userId)
        {
            return channel.IsChannelOwner(userId);
        }

        public static bool HasPermission(
            this Chat chat,
            Guid userId,
            ChatPermissionTypes permissionType,
            Group? group = null,
            Channel? channel = null)
        {
            if (chat.Type == ChatTypes.Group && group != null)
            {
                return group.IsGroupOwner(userId);
            }

            if (chat.Type == ChatTypes.Channel && channel != null)
            {
                return channel.IsChannelOwner(userId);
            }

            return false;
        }

        public static async Task<bool> HasUserPermissionAsync(
            this IChatUserPermissionRepository permissionRepository,
            Guid chatId,
            Guid userId,
            string permissionName)
        {
            try
            {
                if (permissionRepository == null)
                    return false;

                // Use efficient query instead of loading all permissions
                var userPermissions = await permissionRepository.GetUserChatPermissionsAsync(chatId, userId);
                if (userPermissions == null || userPermissions.Count == 0)
                    return false;

                return userPermissions.Any(p => p.Permission?.Name == permissionName);
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> HasUserPermissionsAsync(
            this IChatUserPermissionRepository permissionRepository,
            Guid chatId,
            Guid userId,
            params string[] permissionNames)
        {
            try
            {
                if (permissionRepository == null || permissionNames.Length == 0)
                    return false;

                // Use efficient query instead of loading all permissions
                var userPermissions = await permissionRepository.GetUserChatPermissionsAsync(chatId, userId);
                if (userPermissions == null || userPermissions.Count == 0)
                    return false;

                var userPermNames = userPermissions
                    .Where(p => p.Permission != null)
                    .Select(p => p.Permission.Name)
                    .ToList();

                return permissionNames.All(perm => userPermNames.Contains(perm));
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> HasAnyPermissionAsync(
            this IChatUserPermissionRepository permissionRepository,
            Guid chatId,
            Guid userId,
            params string[] permissionNames)
        {
            try
            {
                if (permissionRepository == null || permissionNames.Length == 0)
                    return false;

                // Use efficient query instead of loading all permissions
                var userPermissions = await permissionRepository.GetUserChatPermissionsAsync(chatId, userId);
                if (userPermissions == null || userPermissions.Count == 0)
                    return false;

                var userPermNames = userPermissions
                    .Where(p => p.Permission != null)
                    .Select(p => p.Permission.Name)
                    .ToList();

                return permissionNames.Any(perm => userPermNames.Contains(perm));
            }
            catch
            {
                return false;
            }
        }
    }

    public static class PermissionCheckExtensions
    {
        public static async Task<Result> CanEditMessageAsync(
            this (IMessageRepository msgRepo, Message message, Guid userId) context)
        {
            var (msgRepo, message, userId) = context;

            if (!message.IsMessageSender(userId))
            {
                return Result.Failure(
                    ApplicationErrors.ChatPermission.Denied);
            }

            return Result.Success();
        }

        public static async Task<Result> CanDeleteMessageAsync(
            this (IMessageRepository msgRepo, Message message, Guid userId) context)
        {
            var (msgRepo, message, userId) = context;

            if (!message.IsMessageSender(userId))
            {
                return Result.Failure(
                    ApplicationErrors.ChatPermission.Denied);
            }

            return Result.Success();
        }

        public static async Task<Result> CanManageChatAsync(
            this (Chat chat, Group? group, Channel? channel, Guid userId) context)
        {
            var (chat, group, channel, userId) = context;

            if (chat.Type == ChatTypes.Group && group != null)
            {
                if (!group.IsGroupOwner(userId))
                    return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }
            else if (chat.Type == ChatTypes.Channel && channel != null)
            {
                if (!channel.IsChannelOwner(userId))
                    return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            return Result.Success();
        }

        public static async Task<Result> CanManageUsersAsync(
            this (Chat chat, Group? group, Channel? channel, Guid userId, IChatUserPermissionRepository permRepo) context)
        {
            var (chat, group, channel, userId, permRepo) = context;

            if (chat.Type == ChatTypes.Group && group != null)
            {
                if (group.IsGroupOwner(userId))
                    return Result.Success();

                var hasPermission = await permRepo.HasUserPermissionAsync(
                    chat.Id, userId, nameof(ChatPermissionTypes.ManageUsers));

                if (!hasPermission)
                    return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }
            else if (chat.Type == ChatTypes.Channel && channel != null)
            {
                if (channel.IsChannelOwner(userId))
                    return Result.Success();

                var hasPermission = await permRepo.HasUserPermissionAsync(
                    chat.Id, userId, nameof(ChatPermissionTypes.ManageUsers));

                if (!hasPermission)
                    return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            return Result.Success();
        }

        public static async Task<Result> CanRemoveMemberAsync(
            this (Chat chat, Group? group, Channel? channel, Guid userId, Guid targetUserId, IChatUserPermissionRepository permRepo) context)
        {
            var (chat, group, channel, userId, targetUserId, permRepo) = context;

            if (userId == targetUserId)
                return Result.Success();

            return await (chat, group, channel, userId, permRepo).CanManageUsersAsync();
        }

        public static async Task<Result> CanSendMessageAsync(
            this (Chat chat, Guid userId, bool isBanned) context)
        {
            var (chat, userId, isBanned) = context;

            if (isBanned)
            {
                return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            return Result.Success();
        }

        public static async Task<Result> CanGrantPermissionAsync(
            this (Chat chat, Group? group, Channel? channel, Guid userId) context)
        {
            var (chat, group, channel, userId) = context;

            if (chat.Type == ChatTypes.Group && group != null)
            {
                if (!group.IsGroupOwner(userId))
                    return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }
            else if (chat.Type == ChatTypes.Channel && channel != null)
            {
                if (!channel.IsChannelOwner(userId))
                    return Result.Failure(ApplicationErrors.ChatPermission.Denied);
            }

            return Result.Success();
        }
    }
}
