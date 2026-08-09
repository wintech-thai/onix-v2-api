using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PayInSlipUpload_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "payin_slip_upload_count",
                table: "PaymentRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "payin_slip_uploads",
                table: "PaymentRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payin_slip_upload_count",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payin_slip_uploads",
                table: "PaymentRequests");
        }
    }
}
