using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PayOutSlipUploads_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "payout_slip_upload_count",
                table: "PaymentRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "payout_slip_uploads",
                table: "PaymentRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payout_slip_upload_count",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payout_slip_uploads",
                table: "PaymentRequests");
        }
    }
}
