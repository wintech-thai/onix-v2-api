using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class OrgUser_Fields_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrgDesc",
                table: "OrganizationsUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrgName",
                table: "OrganizationsUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrgDesc",
                table: "OrganizationsUsers");

            migrationBuilder.DropColumn(
                name: "OrgName",
                table: "OrganizationsUsers");
        }
    }
}
