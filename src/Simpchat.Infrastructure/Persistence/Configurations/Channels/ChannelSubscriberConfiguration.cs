using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Infrastructure.Persistence.Configurations.Channels
{
    internal class ChannelSubscriberConfiguration : IEntityTypeConfiguration<ChannelSubscriber>
    {
        public void Configure(EntityTypeBuilder<ChannelSubscriber> builder)
        {
            builder.Property(cs => cs.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // Primary key: (UserId, ChannelId) - uniquely identifies a channel subscriber
            builder.HasKey(cs => new { cs.UserId, cs.ChannelId });

            // Id is unique but not part of primary key
            builder.HasIndex(cs => cs.Id).IsUnique();
        }
    }
}
