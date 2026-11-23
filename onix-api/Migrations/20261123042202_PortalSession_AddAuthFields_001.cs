using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PortalSession_AddAuthFields_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mac_address",
                table: "PortalSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "realm_name",
                table: "PortalSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_name",
                table: "PortalSessions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mac_address",
                table: "PortalSessions");

            migrationBuilder.DropColumn(
                name: "realm_name",
                table: "PortalSessions");

            migrationBuilder.DropColumn(
                name: "user_name",
                table: "PortalSessions");
        }
    }
}
