using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Initial_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    key_id = table.Column<Guid>(type: "uuid", nullable: false),
                    api_key = table.Column<string>(type: "text", nullable: true),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    key_created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    key_expired_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    key_description = table.Column<string>(type: "text", nullable: true),
                    roles_list = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.key_id);
                });

            migrationBuilder.CreateTable(
                name: "MSystemVariables",
                columns: table => new
                {
                    variable_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    varaible_name = table.Column<string>(type: "text", nullable: true),
                    varaible_value = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MSystemVariables", x => x.variable_id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_custom_id = table.Column<string>(type: "text", nullable: true),
                    org_name = table.Column<string>(type: "text", nullable: true),
                    org_description = table.Column<string>(type: "text", nullable: true),
                    org_created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.org_id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationsUsers",
                columns: table => new
                {
                    org_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_custom_id = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    user_name = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    roles_list = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationsUsers", x => x.org_user_id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_name = table.Column<string>(type: "text", nullable: true),
                    role_description = table.Column<string>(type: "text", nullable: true),
                    role_created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    role_definition = table.Column<string>(type: "text", nullable: true),
                    role_level = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: true),
                    user_email = table.Column<string>(type: "text", nullable: true),
                    user_created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.user_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_api_key",
                table: "ApiKeys",
                column: "api_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_org_id",
                table: "ApiKeys",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_org_custom_id",
                table: "Organizations",
                column: "org_custom_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationsUsers_org_custom_id",
                table: "OrganizationsUsers",
                column: "org_custom_id");

            migrationBuilder.CreateIndex(
                name: "OrgUser_Unique1",
                table: "OrganizationsUsers",
                columns: new[] { "org_custom_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_role_name",
                table: "Roles",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_user_email",
                table: "Users",
                column: "user_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_user_name",
                table: "Users",
                column: "user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "MSystemVariables");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "OrganizationsUsers");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
