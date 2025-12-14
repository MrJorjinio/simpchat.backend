using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Infrastructure.Persistence.Configurations.Messages
{
    internal class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
    {
        public void Configure(EntityTypeBuilder<MessageReaction> builder)
        {
            builder.Property(mr => mr.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // Primary key: (MessageId, ReactionId, UserId) - uniquely identifies a user's reaction to a message
            builder.HasKey(mr => new { mr.MessageId, mr.ReactionId, mr.UserId });

            // Id is unique but not part of primary key
            builder.HasIndex(mr => mr.Id).IsUnique();
        }
    }
}
