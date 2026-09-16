using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Inventory_Doc_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryDocItems",
                columns: table => new
                {
                    inventory_document_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    document_id = table.Column<string>(type: "text", nullable: true),
                    document_no = table.Column<string>(type: "text", nullable: true),
                    document_type = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    lot_id = table.Column<string>(type: "text", nullable: true),
                    from_location_id = table.Column<string>(type: "text", nullable: true),
                    from_location_code = table.Column<string>(type: "text", nullable: true),
                    from_location_name = table.Column<string>(type: "text", nullable: true),
                    from_location_previous_quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    from_location_previous_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    from_location_previous_unit_price = table.Column<decimal>(type: "numeric", nullable: true),
                    from_location_current_quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    from_location_current_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    from_location_current_unit_price = table.Column<decimal>(type: "numeric", nullable: true),
                    to_location_id = table.Column<string>(type: "text", nullable: true),
                    to_location_code = table.Column<string>(type: "text", nullable: true),
                    to_location_name = table.Column<string>(type: "text", nullable: true),
                    to_location_previous_quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    to_location_previous_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    to_location_previous_unit_price = table.Column<decimal>(type: "numeric", nullable: true),
                    to_location_current_quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    to_location_current_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    to_location_current_unit_price = table.Column<decimal>(type: "numeric", nullable: true),
                    item_id = table.Column<string>(type: "text", nullable: true),
                    item_code = table.Column<string>(type: "text", nullable: true),
                    item_name = table.Column<string>(type: "text", nullable: true),
                    item_quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    item_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    item_unit_price = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryDocItems", x => x.inventory_document_item_id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryDocs",
                columns: table => new
                {
                    inventory_document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    document_no = table.Column<string>(type: "text", nullable: true),
                    document_type = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    document_status = table.Column<string>(type: "text", nullable: true),
                    from_location_id = table.Column<string>(type: "text", nullable: true),
                    from_location_code = table.Column<string>(type: "text", nullable: true),
                    from_location_name = table.Column<string>(type: "text", nullable: true),
                    to_location_id = table.Column<string>(type: "text", nullable: true),
                    to_location_code = table.Column<string>(type: "text", nullable: true),
                    to_location_name = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryDocs", x => x.inventory_document_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocItems_document_no",
                table: "InventoryDocItems",
                column: "document_no");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocItems_lot_id",
                table: "InventoryDocItems",
                column: "lot_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocItems_org_id",
                table: "InventoryDocItems",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_description",
                table: "InventoryDocs",
                column: "description");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_document_no",
                table: "InventoryDocs",
                column: "document_no");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_document_status",
                table: "InventoryDocs",
                column: "document_status");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_org_id",
                table: "InventoryDocs",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_org_id_document_no",
                table: "InventoryDocs",
                columns: new[] { "org_id", "document_no" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryDocItems");

            migrationBuilder.DropTable(
                name: "InventoryDocs");
        }
    }
}
