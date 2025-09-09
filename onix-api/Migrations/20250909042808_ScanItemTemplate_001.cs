using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class ScanItemTemplate_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScanItemActions",
                columns: table => new
                {
                    action_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    redirect_url = table.Column<string>(type: "text", nullable: true),
                    encryption_key = table.Column<string>(type: "text", nullable: true),
                    encryption_iv = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanItemActions", x => x.action_id);
                });

            migrationBuilder.CreateTable(
                name: "ScanItemTemplates",
                columns: table => new
                {
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    serial_prefix_digit = table.Column<int>(type: "integer", nullable: true),
                    generator_count = table.Column<int>(type: "integer", nullable: true),
                    serial_digit = table.Column<int>(type: "integer", nullable: true),
                    pin_digit = table.Column<int>(type: "integer", nullable: true),
                    url_template = table.Column<string>(type: "text", nullable: true),
                    noti_email = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanItemTemplates", x => x.template_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScanItemActions_org_id",
                table: "ScanItemActions",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_ScanItemTemplates_org_id",
                table: "ScanItemTemplates",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScanItemActions");

            migrationBuilder.DropTable(
                name: "ScanItemTemplates");
        }
    }
}
