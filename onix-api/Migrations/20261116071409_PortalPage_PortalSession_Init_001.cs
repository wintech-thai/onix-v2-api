using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PortalPage_PortalSession_Init_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortalPages",
                columns: table => new
                {
                    portal_page_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    auth_method = table.Column<string>(type: "text", nullable: true),
                    theme = table.Column<string>(type: "text", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: true),
                    api_key_id = table.Column<Guid>(type: "uuid", nullable: true),
                    api_code = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalPages", x => x.portal_page_id);
                });

            migrationBuilder.CreateTable(
                name: "PortalSessions",
                columns: table => new
                {
                    portal_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    portal_page_code = table.Column<string>(type: "text", nullable: true),
                    parameters_json = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    status_desc = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    login_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalSessions", x => x.portal_session_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortalPages_org_id",
                table: "PortalPages",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "PortalPage_Org_Code_Unique",
                table: "PortalPages",
                columns: new[] { "org_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortalSessions_org_id",
                table: "PortalSessions",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortalPages");

            migrationBuilder.DropTable(
                name: "PortalSessions");
        }
    }
}
