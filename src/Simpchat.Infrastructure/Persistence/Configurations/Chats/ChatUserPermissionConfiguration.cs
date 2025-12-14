using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simpchat.Domain.Entities;

namespace Simpchat.Infrastructure.Persistence.Configurations.Chats
{
    internal class ChatUserPermissionConfiguration : IEntityTypeConfiguration<ChatUserPermission>
    {
        public void Configure(EntityTypeBuilder<ChatUserPermission> builder)
        {
            builder.Property(cup => cup.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // Primary key: (UserId, ChatId, PermissionId) - uniquely identifies a user permission in a chat
            builder.HasKey(cup => new { cup.UserId, cup.ChatId, cup.PermissionId });

            // Id is unique but not part of primary key
            builder.HasIndex(cup => cup.Id).IsUnique();

            builder.HasOne(cup => cup.Permission)
                .WithMany(p => p.UsersAppliedTo)
                .HasForeignKey(cup => cup.PermissionId);
        }
    }
}
