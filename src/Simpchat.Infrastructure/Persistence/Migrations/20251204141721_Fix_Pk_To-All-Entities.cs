using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simpchat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Pk_ToAllEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessagesReactions",
                table: "MessagesReactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupsMembers",
                table: "GroupsMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GlobalRolesPermissions",
                table: "GlobalRolesPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Conversations",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_UserId1",
                table: "Conversations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatsUsersPermissions",
                table: "ChatsUsersPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChannelsSubscribers",
                table: "ChannelsSubscribers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessagesReactions",
                table: "MessagesReactions",
                columns: new[] { "MessageId", "ReactionId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupsMembers",
                table: "GroupsMembers",
                columns: new[] { "GroupId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GlobalRolesPermissions",
                table: "GlobalRolesPermissions",
                columns: new[] { "RoleId", "PermissionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Conversations",
                table: "Conversations",
                columns: new[] { "UserId1", "UserId2" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatsUsersPermissions",
                table: "ChatsUsersPermissions",
                columns: new[] { "UserId", "ChatId", "PermissionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChannelsSubscribers",
                table: "ChannelsSubscribers",
                columns: new[] { "UserId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_MessagesReactions_Id",
                table: "MessagesReactions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupsMembers_Id",
                table: "GroupsMembers",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalRolesPermissions_Id",
                table: "GlobalRolesPermissions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatsUsersPermissions_Id",
                table: "ChatsUsersPermissions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelsSubscribers_Id",
                table: "ChannelsSubscribers",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessagesReactions",
                table: "MessagesReactions");

            migrationBuilder.DropIndex(
                name: "IX_MessagesReactions_Id",
                table: "MessagesReactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupsMembers",
                table: "GroupsMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupsMembers_Id",
                table: "GroupsMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GlobalRolesPermissions",
                table: "GlobalRolesPermissions");

            migrationBuilder.DropIndex(
                name: "IX_GlobalRolesPermissions_Id",
                table: "GlobalRolesPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Conversations",
                table: "Conversations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatsUsersPermissions",
                table: "ChatsUsersPermissions");

            migrationBuilder.DropIndex(
                name: "IX_ChatsUsersPermissions_Id",
                table: "ChatsUsersPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChannelsSubscribers",
                table: "ChannelsSubscribers");

            migrationBuilder.DropIndex(
                name: "IX_ChannelsSubscribers_Id",
                table: "ChannelsSubscribers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessagesReactions",
                table: "MessagesReactions",
                columns: new[] { "MessageId", "ReactionId", "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupsMembers",
                table: "GroupsMembers",
                columns: new[] { "GroupId", "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GlobalRolesPermissions",
                table: "GlobalRolesPermissions",
                columns: new[] { "RoleId", "PermissionId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Conversations",
                table: "Conversations",
                columns: new[] { "Id", "UserId1", "UserId2" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatsUsersPermissions",
                table: "ChatsUsersPermissions",
                columns: new[] { "UserId", "ChatId", "PermissionId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChannelsSubscribers",
                table: "ChannelsSubscribers",
                columns: new[] { "UserId", "ChannelId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserId1",
                table: "Conversations",
                column: "UserId1");
        }
    }
}
