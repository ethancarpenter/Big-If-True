using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndCampaignOwnerFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            // Deterministic, environment-agnostic seed: this exact SQL runs identically in
            // every environment. Its *effect* depends on data already present, not on
            // ASPNETCORE_ENVIRONMENT - a fresh database (including a fresh production
            // deployment) has no Campaigns referencing the legacy placeholder id, so this
            // inserts nothing there. This database (and only a database with pre-existing
            // placeholder-owned Campaigns) does, so the row is seeded here to satisfy the
            // foreign key added below.
            //
            // The seeded row's PasswordHash is a hash of a cryptographically random value
            // generated once while authoring this migration and never recorded anywhere -
            // nobody can log in as this account with any known password. It stays locked
            // in every environment. A separate, Development-only, idempotent step at
            // application startup (not in any migration) is the only place the known
            // dev@local.test / DevPassword123! credential is ever established, and it can
            // only run when ASPNETCORE_ENVIRONMENT=Development.
            migrationBuilder.Sql(
                """
                INSERT INTO "Users" ("Id", "Email", "NormalizedEmail", "PasswordHash", "CreatedAt", "UpdatedAt")
                SELECT '00000000-0000-0000-0000-000000000001', 'dev@local.test', 'DEV@LOCAL.TEST',
                       'AQAAAAIAAYagAAAAEIyCVfdN9OpclgEWEsr+XNa+d/AXyCYDYtSF9YBwXnj3eWofwns+WzRdPXjsVms+vA==',
                       now(), now()
                WHERE EXISTS (SELECT 1 FROM "Campaigns" WHERE "UserId" = '00000000-0000-0000-0000-000000000001')
                  AND NOT EXISTS (SELECT 1 FROM "Users" WHERE "Id" = '00000000-0000-0000-0000-000000000001');
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Users_UserId",
                table: "Campaigns",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Users_UserId",
                table: "Campaigns");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
