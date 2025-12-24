namespace Simpchat.Application.Models.Reactions
{
    /// <summary>
    /// Represents a reaction summary for a message, grouped by reaction type
    /// </summary>
    public class MessageReactionSummaryDto
    {
        public string ReactionType { get; set; }
        public int Count { get; set; }
        public List<string> UserIds { get; set; } = new();
    }

    /// <summary>
    /// Individual reaction with user info
    /// </summary>
    public class MessageReactionDto
    {
        public Guid Id { get; set; }
        public string ReactionType { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }
}
