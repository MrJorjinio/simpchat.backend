using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Infrastructure.Persistence.Configurations.IdentityConfigs
{
    internal class GlobalRolePermissionConfiguration : IEntityTypeConfiguration<GlobalRolePermission>
    {
        public void Configure(EntityTypeBuilder<GlobalRolePermission> builder)
        {
            builder.Property(grp => grp.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // Primary key: (RoleId, PermissionId) - uniquely identifies a role permission
            builder.HasKey(grp => new { grp.RoleId, grp.PermissionId });

            // Id is unique but not part of primary key
            builder.HasIndex(grp => grp.Id).IsUnique();
        }
    }
}
