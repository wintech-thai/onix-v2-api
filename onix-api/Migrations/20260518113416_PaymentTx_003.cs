using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentTx_003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "pay_in_fee_decimal",
                table: "PaymentTransactions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_payin_amount_decimal",
                table: "PaymentTransactions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tx_amount_decimal",
                table: "PaymentTransactions",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pay_in_fee_decimal",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "total_payin_amount_decimal",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "tx_amount_decimal",
                table: "PaymentTransactions");
        }
    }
}
