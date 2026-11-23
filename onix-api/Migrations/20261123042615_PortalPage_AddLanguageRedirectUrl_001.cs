using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PortalPage_AddLanguageRedirectUrl_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "PortalPages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "redirect_url_mode",
                table: "PortalPages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "redirect_url_value",
                table: "PortalPages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "language",
                table: "PortalPages");

            migrationBuilder.DropColumn(
                name: "redirect_url_mode",
                table: "PortalPages");

            migrationBuilder.DropColumn(
                name: "redirect_url_value",
                table: "PortalPages");
        }
    }
}
