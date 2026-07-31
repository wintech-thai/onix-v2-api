using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentTx_Fields_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payin_promptpay_id",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payout_promptpay_id",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ref_id1",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ref_id2",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ref_id3",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payin_promptpay_id",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "payout_promptpay_id",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ref_id1",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ref_id2",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ref_id3",
                table: "PaymentTransactions");
        }
    }
}
