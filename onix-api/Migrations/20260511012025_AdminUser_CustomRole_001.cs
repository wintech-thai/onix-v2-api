using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AdminUser_CustomRole_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrgDesc",
                table: "OrganizationsUsers");

            migrationBuilder.DropColumn(
                name: "OrgName",
                table: "OrganizationsUsers");

            migrationBuilder.DropColumn(
                name: "OrgType",
                table: "OrganizationsUsers");

            migrationBuilder.AddColumn<string>(
                name: "custom_role_id",
                table: "AdminUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "custom_role_id",
                table: "AdminUsers");

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

            migrationBuilder.AddColumn<string>(
                name: "OrgType",
                table: "OrganizationsUsers",
                type: "text",
                nullable: true);
        }
    }
}
