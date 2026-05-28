using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentRequest_PayOut_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payout_account_level",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payout_account_type",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payout_bank_account_name",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payout_bank_account_no",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payout_bank_code",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payout_bank_id",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "payout_fee_pct",
                table: "PaymentRequests",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payout_promptpay_id",
                table: "PaymentRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payout_account_level",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payout_account_type",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payout_bank_account_name",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payout_bank_account_no",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payout_bank_code",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payout_bank_id",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payout_fee_pct",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payout_promptpay_id",
                table: "PaymentRequests");
        }
    }
}
