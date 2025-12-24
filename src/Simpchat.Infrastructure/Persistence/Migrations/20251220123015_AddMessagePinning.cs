using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simpchat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagePinning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChatId",
                table: "Messages");

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PinnedAt",
                table: "Messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PinnedById",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId_IsPinned",
                table: "Messages",
                columns: new[] { "ChatId", "IsPinned" },
                filter: "\"IsPinned\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_PinnedById",
                table: "Messages",
                column: "PinnedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_PinnedById",
                table: "Messages",
                column: "PinnedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_PinnedById",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChatId_IsPinned",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_PinnedById",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "PinnedAt",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "PinnedById",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId",
                table: "Messages",
                column: "ChatId");
        }
    }
}
