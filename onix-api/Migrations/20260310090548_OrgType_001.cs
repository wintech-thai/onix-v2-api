using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class OrgType_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrgType",
                table: "OrganizationsUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "org_type",
                table: "Organizations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrgType",
                table: "OrganizationsUsers");

            migrationBuilder.DropColumn(
                name: "org_type",
                table: "Organizations");
        }
    }
}
