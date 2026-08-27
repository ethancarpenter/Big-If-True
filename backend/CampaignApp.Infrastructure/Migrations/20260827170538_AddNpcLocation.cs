using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNpcLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NpcLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NpcId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpcLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NpcLocations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NpcLocations_Npcs_NpcId",
                        column: x => x.NpcId,
                        principalTable: "Npcs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NpcLocations_LocationId",
                table: "NpcLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_NpcLocations_NpcId_LocationId",
                table: "NpcLocations",
                columns: new[] { "NpcId", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NpcLocations_NpcId_Primary",
                table: "NpcLocations",
                column: "NpcId",
                unique: true,
                filter: "\"IsPrimary\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NpcLocations");
        }
    }
}
