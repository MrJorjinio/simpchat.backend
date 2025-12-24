using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simpchat.Domain.Entities;

namespace Simpchat.Infrastructure.Persistence.Configurations.Users
{
    internal class UserBanConfiguration : IEntityTypeConfiguration<UserBan>
    {
        public void Configure(EntityTypeBuilder<UserBan> builder)
        {
            builder.ToTable("UserBans");

            builder.Property(ub => ub.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(ub => ub.BannedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign key relationships
            builder.HasOne(ub => ub.Blocker)
                .WithMany(u => u.BlockedUsers)
                .HasForeignKey(ub => ub.BlockerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ub => ub.BlockedUser)
                .WithMany(u => u.BlockedByUsers)
                .HasForeignKey(ub => ub.BlockedUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: a user can only block another user once
            builder.HasIndex(ub => new { ub.BlockerId, ub.BlockedUserId })
                .IsUnique();
        }
    }
}
