using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simpchat.Domain.Entities;

namespace Simpchat.Infrastructure.Persistence.Configurations.Messages
{
    internal class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
    {
        public void Configure(EntityTypeBuilder<MessageReaction> builder)
        {
            // Primary key: (MessageId, ReactionType, UserId) - uniquely identifies a user's reaction to a message
            // A user can only have one instance of the same reaction type per message
            builder.HasKey(mr => new { mr.MessageId, mr.ReactionType, mr.UserId });

            // Ignore the Id property from BaseEntity since we use composite key
            builder.Ignore(mr => mr.Id);

            // Store enum as string for readability
            builder.Property(mr => mr.ReactionType)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Index for efficient querying by message
            builder.HasIndex(mr => mr.MessageId);
        }
    }
}
