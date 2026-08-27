using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Payment_PayerName_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payer_name",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payer_name",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_payer_name",
                table: "PaymentTransactions",
                column: "payer_name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_payer_name",
                table: "PaymentRequests",
                column: "payer_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_payer_name",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_payer_name",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payer_name",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "payer_name",
                table: "PaymentRequests");
        }
    }
}
