using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentRequest_PartialLimit_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "payout_partial_count_limit_p2p",
                table: "PaymentRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payout_partial_count_p2p",
                table: "PaymentRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payout_partial_count_limit_p2p",
                table: "Merchants",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payout_partial_count_limit_p2p",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payout_partial_count_p2p",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payout_partial_count_limit_p2p",
                table: "Merchants");
        }
    }
}
