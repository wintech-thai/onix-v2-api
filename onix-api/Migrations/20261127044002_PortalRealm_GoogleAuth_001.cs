using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PortalRealm_GoogleAuth_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "google_claims_json",
                table: "PortalSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "PortalRealms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "google_client_id",
                table: "PortalRealms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "google_client_secret",
                table: "PortalRealms",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "google_claims_json",
                table: "PortalSessions");

            migrationBuilder.DropColumn(
                name: "description",
                table: "PortalRealms");

            migrationBuilder.DropColumn(
                name: "google_client_id",
                table: "PortalRealms");

            migrationBuilder.DropColumn(
                name: "google_client_secret",
                table: "PortalRealms");
        }
    }
}
