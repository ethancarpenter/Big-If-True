using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestConnectionAndQuestGraphPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceQuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetQuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestConnections", x => x.Id);
                    table.CheckConstraint("CK_QuestConnections_NoSelfLink", "\"SourceQuestId\" <> \"TargetQuestId\"");
                    table.ForeignKey(
                        name: "FK_QuestConnections_Quests_SourceQuestId",
                        column: x => x.SourceQuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestConnections_Quests_TargetQuestId",
                        column: x => x.TargetQuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestGraphPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    X = table.Column<double>(type: "double precision", nullable: false),
                    Y = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestGraphPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestGraphPositions_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestConnections_SourceQuestId_TargetQuestId",
                table: "QuestConnections",
                columns: new[] { "SourceQuestId", "TargetQuestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestConnections_TargetQuestId",
                table: "QuestConnections",
                column: "TargetQuestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestGraphPositions_QuestId",
                table: "QuestGraphPositions",
                column: "QuestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestConnections");

            migrationBuilder.DropTable(
                name: "QuestGraphPositions");
        }
    }
}
