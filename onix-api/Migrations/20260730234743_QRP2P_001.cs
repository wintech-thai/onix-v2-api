using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class QRP2P_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "qr_code_p2p",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_payout_amount_decimal_p2p",
                table: "PaymentRequests",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "qr_code_p2p",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "total_payout_amount_decimal_p2p",
                table: "PaymentRequests");
        }
    }
}
