using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Wallet_Decimal_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "merchant_id",
                table: "Wallets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "point_balance_decimal",
                table: "Wallets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "current_balance_decimal",
                table: "PointsTxs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "previous_balance_decimal",
                table: "PointsTxs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tx_amount_decimal",
                table: "PointsTxs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "balance_begin_decimal",
                table: "PointBalances",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "balance_end_decimal",
                table: "PointBalances",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tx_in_decimal",
                table: "PointBalances",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tx_out_decimal",
                table: "PointBalances",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "merchant_id",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "point_balance_decimal",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "current_balance_decimal",
                table: "PointsTxs");

            migrationBuilder.DropColumn(
                name: "previous_balance_decimal",
                table: "PointsTxs");

            migrationBuilder.DropColumn(
                name: "tx_amount_decimal",
                table: "PointsTxs");

            migrationBuilder.DropColumn(
                name: "balance_begin_decimal",
                table: "PointBalances");

            migrationBuilder.DropColumn(
                name: "balance_end_decimal",
                table: "PointBalances");

            migrationBuilder.DropColumn(
                name: "tx_in_decimal",
                table: "PointBalances");

            migrationBuilder.DropColumn(
                name: "tx_out_decimal",
                table: "PointBalances");
        }
    }
}
