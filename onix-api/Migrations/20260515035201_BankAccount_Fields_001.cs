using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class BankAccount_Fields_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payin_bank_account_name",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payin_bank_account_no",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payin_bank_code",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payin_bank_id",
                table: "PaymentRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payin_bank_account_name",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_bank_account_no",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_bank_code",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_bank_id",
                table: "PaymentRequests");
        }
    }
}
