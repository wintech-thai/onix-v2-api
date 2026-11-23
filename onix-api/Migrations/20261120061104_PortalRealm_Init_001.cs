using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PortalRealm_Init_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "auth_method",
                table: "PortalSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "realm_id",
                table: "PortalPages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PortalRealms",
                columns: table => new
                {
                    realm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    realm_name = table.Column<string>(type: "text", nullable: true),
                    auth_type = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalRealms", x => x.realm_id);
                });

            migrationBuilder.CreateTable(
                name: "PortalRealmUsers",
                columns: table => new
                {
                    realm_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    realm_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "text", nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    encrypted_password = table.Column<string>(type: "text", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_seen_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalRealmUsers", x => x.realm_user_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortalRealms_org_id",
                table: "PortalRealms",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "PortalRealm_Org_Name_Unique",
                table: "PortalRealms",
                columns: new[] { "org_id", "realm_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortalRealmUsers_org_id",
                table: "PortalRealmUsers",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_PortalRealmUsers_realm_id",
                table: "PortalRealmUsers",
                column: "realm_id");

            migrationBuilder.CreateIndex(
                name: "PortalRealmUser_Realm_UserName_Unique",
                table: "PortalRealmUsers",
                columns: new[] { "realm_id", "user_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortalRealms");

            migrationBuilder.DropTable(
                name: "PortalRealmUsers");

            migrationBuilder.DropColumn(
                name: "auth_method",
                table: "PortalSessions");

            migrationBuilder.DropColumn(
                name: "realm_id",
                table: "PortalPages");
        }
    }
}
