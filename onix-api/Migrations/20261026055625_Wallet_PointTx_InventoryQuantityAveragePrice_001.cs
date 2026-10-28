using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Wallet_PointTx_InventoryQuantityAveragePrice_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "inventory_item_id",
                table: "Wallets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "inventory_location_id",
                table: "Wallets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "point_average_price_decimal",
                table: "Wallets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "point_quantity_balance_decimal",
                table: "Wallets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "current_average_price_decimal",
                table: "PointsTxs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "current_balance_quantity_decimal",
                table: "PointsTxs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "previous_average_price_decimal",
                table: "PointsTxs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "previous_balance_quantity_decimal",
                table: "PointsTxs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tx_average_price_decimal",
                table: "PointsTxs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tx_quantity_decimal",
                table: "PointsTxs",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "Wallet_InventoryLocationItem_Unique",
                table: "Wallets",
                columns: new[] { "org_id", "inventory_location_id", "inventory_item_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Wallet_InventoryLocationItem_Unique",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "inventory_item_id",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "inventory_location_id",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "point_average_price_decimal",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "point_quantity_balance_decimal",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "current_average_price_decimal",
                table: "PointsTxs");

            migrationBuilder.DropColumn(
                name: "current_balance_quantity_decimal",
                table: "PointsTxs");

            migrationBuilder.DropColumn(
                name: "previous_average_price_decimal",
                table: "PointsTxs");

            migrationBuilder.DropColumn(
                name: "previous_balance_quantity_decimal",
                table: "PointsTxs");

            migrationBuilder.DropColumn(
                name: "tx_average_price_decimal",
                table: "PointsTxs");

            migrationBuilder.DropColumn(
                name: "tx_quantity_decimal",
                table: "PointsTxs");
        }
    }
}
