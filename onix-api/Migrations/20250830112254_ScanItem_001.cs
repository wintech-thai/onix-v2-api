using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class ScanItem_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScanItems",
                columns: table => new
                {
                    scan_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    serial = table.Column<string>(type: "text", nullable: true),
                    pin = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    product_code = table.Column<string>(type: "text", nullable: true),
                    registered_flag = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    registered_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanItems", x => x.scan_item_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScanItems_org_id",
                table: "ScanItems",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_ScanItems_org_id_serial_pin",
                table: "ScanItems",
                columns: new[] { "org_id", "serial", "pin" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScanItems");
        }
    }
}
