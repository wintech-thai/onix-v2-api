using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class InventoryLocation_InventoryItem_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    reference_code = table.Column<string>(type: "text", nullable: true),
                    name_th = table.Column<string>(type: "text", nullable: true),
                    name_en = table.Column<string>(type: "text", nullable: true),
                    item_type = table.Column<string>(type: "text", nullable: true),
                    unit = table.Column<string>(type: "text", nullable: true),
                    item_group = table.Column<string>(type: "text", nullable: true),
                    remark = table.Column<string>(type: "text", nullable: true),
                    minimum_quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    is_vat_included = table.Column<bool>(type: "boolean", nullable: false),
                    image_base64 = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.inventory_item_id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryLocations",
                columns: table => new
                {
                    inventory_location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    location_type = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLocations", x => x.inventory_location_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_item_type",
                table: "InventoryItems",
                column: "item_type");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_org_id",
                table: "InventoryItems",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocations_location_type",
                table: "InventoryLocations",
                column: "location_type");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocations_org_id",
                table: "InventoryLocations",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "InventoryLocations");
        }
    }
}
