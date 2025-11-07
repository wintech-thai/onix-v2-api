using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AdminUsers_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    admin_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    user_name = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    roles_list = table.Column<string>(type: "text", nullable: true),
                    user_status = table.Column<string>(type: "text", nullable: true),
                    tmp_user_email = table.Column<string>(type: "text", nullable: true),
                    previous_user_status = table.Column<string>(type: "text", nullable: true),
                    invited_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invited_by = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.admin_user_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_user_name",
                table: "AdminUsers",
                column: "user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUsers");
        }
    }
}
