using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simpchat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyReactionsToKeywords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessagesReactions_Reactions_ReactionId",
                table: "MessagesReactions");

            migrationBuilder.DropTable(
                name: "Reactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MessagesReactions",
                table: "MessagesReactions");

            migrationBuilder.DropIndex(
                name: "IX_MessagesReactions_ReactionId",
                table: "MessagesReactions");

            migrationBuilder.DropColumn(
                name: "ReactionId",
                table: "MessagesReactions");

            migrationBuilder.AddColumn<string>(
                name: "ReactionType",
                table: "MessagesReactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessagesReactions",
                table: "MessagesReactions",
                columns: new[] { "MessageId", "ReactionType", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_MessagesReactions_MessageId",
                table: "MessagesReactions",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessagesReactions",
                table: "MessagesReactions");

            migrationBuilder.DropIndex(
                name: "IX_MessagesReactions_MessageId",
                table: "MessagesReactions");

            migrationBuilder.DropColumn(
                name: "ReactionType",
                table: "MessagesReactions");

            migrationBuilder.AddColumn<Guid>(
                name: "ReactionId",
                table: "MessagesReactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessagesReactions",
                table: "MessagesReactions",
                columns: new[] { "MessageId", "ReactionId", "UserId" });

            migrationBuilder.CreateTable(
                name: "Reactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessagesReactions_ReactionId",
                table: "MessagesReactions",
                column: "ReactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessagesReactions_Reactions_ReactionId",
                table: "MessagesReactions",
                column: "ReactionId",
                principalTable: "Reactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
