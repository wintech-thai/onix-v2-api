using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PayoutIsWithdrawal_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "payout_is_withdrawal",
                table: "PaymentTransactions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "payout_is_withdrawal",
                table: "PaymentRequests",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payout_is_withdrawal",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "payout_is_withdrawal",
                table: "PaymentRequests");
        }
    }
}
