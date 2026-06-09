using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentRequest_Override_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "account_type",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_payin_bank_account_override",
                table: "PaymentRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payin_account_type_override",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payin_bank_account_name_override",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payin_bank_account_no_override",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payin_bank_code_override",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payin_promptpay_id_override",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "promptpay_id",
                table: "PaymentRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_type",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "is_payin_bank_account_override",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_account_type_override",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_bank_account_name_override",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_bank_account_no_override",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_bank_code_override",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_promptpay_id_override",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "promptpay_id",
                table: "PaymentRequests");
        }
    }
}
