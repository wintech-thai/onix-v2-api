using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentTx_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "generated_amount_str",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    merchant_id = table.Column<string>(type: "text", nullable: true),
                    payment_request_id = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    direction = table.Column<string>(type: "text", nullable: true),
                    tx_amount = table.Column<double>(type: "double precision", nullable: true),
                    pay_in_fee_pct = table.Column<double>(type: "double precision", nullable: true),
                    pay_in_fee = table.Column<double>(type: "double precision", nullable: true),
                    pay_out_fee_pct = table.Column<double>(type: "double precision", nullable: true),
                    pay_out_fee = table.Column<double>(type: "double precision", nullable: true),
                    total_payin_amount = table.Column<double>(type: "double precision", nullable: true),
                    total_payout_amount = table.Column<double>(type: "double precision", nullable: true),
                    payin_bank_account_id = table.Column<string>(type: "text", nullable: true),
                    payin_bank_code = table.Column<string>(type: "text", nullable: true),
                    payin_bank_account_no = table.Column<string>(type: "text", nullable: true),
                    payin_bank_account_name = table.Column<string>(type: "text", nullable: true),
                    payout_bank_code = table.Column<string>(type: "text", nullable: true),
                    payout_bank_account_no = table.Column<string>(type: "text", nullable: true),
                    payout_bank_account_name = table.Column<string>(type: "text", nullable: true),
                    from_bank_code = table.Column<string>(type: "text", nullable: true),
                    from_bank_account_no = table.Column<string>(type: "text", nullable: true),
                    from_bank_account_name = table.Column<string>(type: "text", nullable: true),
                    processing_messages = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.transaction_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_generated_amount_str",
                table: "PaymentRequests",
                column: "generated_amount_str");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_created_date",
                table: "PaymentTransactions",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_direction",
                table: "PaymentTransactions",
                column: "direction");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_merchant_id",
                table: "PaymentTransactions",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_org_id",
                table: "PaymentTransactions",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_payin_bank_account_id",
                table: "PaymentTransactions",
                column: "payin_bank_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_payment_request_id",
                table: "PaymentTransactions",
                column: "payment_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_status",
                table: "PaymentTransactions",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_generated_amount_str",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "generated_amount_str",
                table: "PaymentRequests");
        }
    }
}
