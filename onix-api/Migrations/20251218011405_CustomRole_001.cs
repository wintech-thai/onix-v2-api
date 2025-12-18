using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class CustomRole_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomRoles",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    role_name = table.Column<string>(type: "text", nullable: true),
                    role_description = table.Column<string>(type: "text", nullable: true),
                    role_definition = table.Column<string>(type: "text", nullable: true),
                    role_created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomRoles", x => x.role_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomRoles_org_id_role_name",
                table: "CustomRoles",
                columns: new[] { "org_id", "role_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomRoles_role_name",
                table: "CustomRoles",
                column: "role_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomRoles");
        }
    }
}
