using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Infrastructure.Persistence.Configurations.Groups
{
    internal class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
    {
        public void Configure(EntityTypeBuilder<GroupMember> builder)
        {
            builder.Property(gm => gm.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // Primary key: (GroupId, UserId) - uniquely identifies a group member
            builder.HasKey(gp => new { gp.GroupId, gp.UserId });

            // Id is unique but not part of primary key
            builder.HasIndex(gp => gp.Id).IsUnique();
        }
    }
}
