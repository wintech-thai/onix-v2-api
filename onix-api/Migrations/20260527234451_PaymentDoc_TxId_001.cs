using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentDoc_TxId_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_transaction_id",
                table: "PaymentDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_payment_transaction_id",
                table: "PaymentDocuments",
                column: "payment_transaction_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentDocuments_payment_transaction_id",
                table: "PaymentDocuments");

            migrationBuilder.DropColumn(
                name: "payment_transaction_id",
                table: "PaymentDocuments");
        }
    }
}
