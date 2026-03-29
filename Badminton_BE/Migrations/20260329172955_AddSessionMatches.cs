using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Badminton_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    TeamAScore = table.Column<int>(type: "int", nullable: false),
                    TeamBScore = table.Column<int>(type: "int", nullable: false),
                    Winner = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionMatches_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SessionMatchPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SessionMatchId = table.Column<int>(type: "int", nullable: false),
                    SessionPlayerId = table.Column<int>(type: "int", nullable: false),
                    Team = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionMatchPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionMatchPlayers_SessionMatches_SessionMatchId",
                        column: x => x.SessionMatchId,
                        principalTable: "SessionMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionMatchPlayers_SessionPlayers_SessionPlayerId",
                        column: x => x.SessionPlayerId,
                        principalTable: "SessionPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMatches_SessionId",
                table: "SessionMatches",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMatches_UserId",
                table: "SessionMatches",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMatchPlayers_SessionMatchId_SessionPlayerId",
                table: "SessionMatchPlayers",
                columns: new[] { "SessionMatchId", "SessionPlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionMatchPlayers_SessionPlayerId",
                table: "SessionMatchPlayers",
                column: "SessionPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMatchPlayers_UserId",
                table: "SessionMatchPlayers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionMatchPlayers");

            migrationBuilder.DropTable(
                name: "SessionMatches");
        }
    }
}
