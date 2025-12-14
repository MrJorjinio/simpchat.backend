using Microsoft.Extensions.DependencyInjection;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using System.Collections.Concurrent;

namespace Simpchat.Application.Features
{
    internal class PresenceService : IPresenceService
    {
        private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
        private readonly object _lock = new();
        private readonly IServiceProvider _serviceProvider;

        public PresenceService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task UserConnectedAsync(Guid userId, string connectionId)
        {
            lock (_lock)
            {
                if (!_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = new HashSet<string>();
                }

                _userConnections[userId].Add(connectionId);
            }

            return Task.CompletedTask;
        }

        public Task UserDisconnectedAsync(Guid userId, string connectionId)
        {
            lock (_lock)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId].Remove(connectionId);

                    // Remove user entry if no connections left
                    if (_userConnections[userId].Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public bool IsUserOnline(Guid userId)
        {
            return _userConnections.ContainsKey(userId) && _userConnections[userId].Count > 0;
        }

        public List<string> GetUserConnections(Guid userId)
        {
            lock (_lock)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    return _userConnections[userId].ToList();
                }

                return new List<string>();
            }
        }

        public async Task<List<Guid>> GetRelatedUserIdsAsync(Guid userId)
        {
            var relatedUserIds = new HashSet<Guid>();

            // Create a scope to access scoped repositories from the singleton service
            using (var scope = _serviceProvider.CreateScope())
            {
                var conversationRepo = scope.ServiceProvider.GetRequiredService<IConversationRepository>();
                var groupRepo = scope.ServiceProvider.GetRequiredService<IGroupRepository>();
                var channelRepo = scope.ServiceProvider.GetRequiredService<IChannelRepository>();

                // Get conversation partners
                var conversations = await conversationRepo.GetUserConversationsAsync(userId);
                foreach (var conversation in conversations)
                {
                    var otherUserId = conversation.UserId1 == userId ? conversation.UserId2 : conversation.UserId1;
                    relatedUserIds.Add(otherUserId);
                }

                // Get group members
                var groups = await groupRepo.GetUserParticipatedGroupsAsync(userId);
                foreach (var group in groups)
                {
                    if (group.Members != null)
                    {
                        foreach (var member in group.Members)
                        {
                            if (member.UserId != userId)
                            {
                                relatedUserIds.Add(member.UserId);
                            }
                        }
                    }

                    // Also add the group owner
                    if (group.CreatedById != userId)
                    {
                        relatedUserIds.Add(group.CreatedById);
                    }
                }

                // Get channel subscribers
                var channels = await channelRepo.GetUserSubscribedChannelsAsync(userId);
                foreach (var channel in channels)
                {
                    if (channel.Subscribers != null)
                    {
                        foreach (var subscriber in channel.Subscribers)
                        {
                            if (subscriber.UserId != userId)
                            {
                                relatedUserIds.Add(subscriber.UserId);
                            }
                        }
                    }

                    // Also add the channel owner
                    if (channel.CreatedById != userId)
                    {
                        relatedUserIds.Add(channel.CreatedById);
                    }
                }
            }

            return relatedUserIds.ToList();
        }
    }
}
