using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simpchat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdFromMessageReaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessagesReactions_Id",
                table: "MessagesReactions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MessagesReactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "MessagesReactions",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesReactions_Id",
                table: "MessagesReactions",
                column: "Id",
                unique: true);
        }
    }
}
