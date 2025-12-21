using Microsoft.EntityFrameworkCore;
using Simpchat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Infrastructure.Persistence
{
    public class SimpchatDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<MessageReaction> MessagesReactions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatUserPermission> ChatsUsersPermissions { get; set; }
        public DbSet<ChatPermission> ChatPermissions { get; set; }
        public DbSet<GroupMember> GroupsMembers { get; set; }
        public DbSet<ChannelSubscriber> ChannelsSubscribers { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<GlobalRole> GlobalRoles { get; set; }
        public DbSet<GlobalPermission> GlobalPermissions { get; set; }
        public DbSet<GlobalRolePermission> GlobalRolesPermissions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ChatBan> ChatBans { get; set; }
        public DbSet<UserBan> UserBans { get; set; }
        public DbSet<UserOtp> UserOtps { get; set; }
        public DbSet<EmailOtp> EmailOtps { get; set; }
        public SimpchatDbContext(DbContextOptions<SimpchatDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SimpchatDbContext).Assembly);
        }
    }
}