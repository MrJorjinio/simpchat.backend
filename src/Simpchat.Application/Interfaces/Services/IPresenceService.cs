namespace Simpchat.Application.Interfaces.Services
{
    public interface IPresenceService
    {
        Task UserConnectedAsync(Guid userId, string connectionId);
        Task UserDisconnectedAsync(Guid userId, string connectionId);
        bool IsUserOnline(Guid userId);
        List<string> GetUserConnections(Guid userId);
        Task<List<Guid>> GetRelatedUserIdsAsync(Guid userId);
    }
}
