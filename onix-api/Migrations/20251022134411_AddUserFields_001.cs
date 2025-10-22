using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFields_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "invited_date",
                table: "OrganizationsUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "previous_user_status",
                table: "OrganizationsUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_status",
                table: "OrganizationsUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invited_date",
                table: "OrganizationsUsers");

            migrationBuilder.DropColumn(
                name: "previous_user_status",
                table: "OrganizationsUsers");

            migrationBuilder.DropColumn(
                name: "user_status",
                table: "OrganizationsUsers");
        }
    }
}
