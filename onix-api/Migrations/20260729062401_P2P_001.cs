using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class P2P_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "partial_payout_history",
                table: "PaymentRequests",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "payin_is_peer_to_peer",
                table: "PaymentRequests",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payin_p2p_payout_id",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_payout_paid_amount_decimal",
                table: "PaymentRequests",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_payout_pending_paid_amount_decimal",
                table: "PaymentRequests",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "partial_payout_history",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_is_peer_to_peer",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_p2p_payout_id",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "total_payout_paid_amount_decimal",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "total_payout_pending_paid_amount_decimal",
                table: "PaymentRequests");
        }
    }
}
