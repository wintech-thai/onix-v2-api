using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Merchant_Tx_Limit_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "payin_daily_tx_amount_limit",
                table: "Merchants",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "payin_daily_tx_count_limit",
                table: "Merchants",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payin_daily_tx_amount_limit",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "payin_daily_tx_count_limit",
                table: "Merchants");
        }
    }
}
