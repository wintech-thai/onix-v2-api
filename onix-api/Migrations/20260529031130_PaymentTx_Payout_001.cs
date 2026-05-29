using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentTx_Payout_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payout_bank_account_id",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "payout_fee_decimal",
                table: "PaymentTransactions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_payout_amount_decimal",
                table: "PaymentTransactions",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payout_bank_account_id",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "payout_fee_decimal",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "total_payout_amount_decimal",
                table: "PaymentTransactions");
        }
    }
}
