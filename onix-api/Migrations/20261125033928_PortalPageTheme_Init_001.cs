using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PortalPageTheme_Init_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "theme_id",
                table: "PortalPages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PortalPageThemes",
                columns: table => new
                {
                    theme_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    theme_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    auth_type = table.Column<string>(type: "text", nullable: true),
                    language = table.Column<string>(type: "text", nullable: true),
                    variables_json = table.Column<string>(type: "text", nullable: true),
                    html_template = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalPageThemes", x => x.theme_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortalPageThemes_org_id",
                table: "PortalPageThemes",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "PortalPageTheme_Org_Name_Unique",
                table: "PortalPageThemes",
                columns: new[] { "org_id", "theme_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortalPageThemes");

            migrationBuilder.DropColumn(
                name: "theme_id",
                table: "PortalPages");
        }
    }
}
