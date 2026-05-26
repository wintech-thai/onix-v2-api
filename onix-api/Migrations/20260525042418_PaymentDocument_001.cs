using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentDocument_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentDocuments",
                columns: table => new
                {
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    merchant_id = table.Column<string>(type: "text", nullable: true),
                    payment_request_id = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    direction = table.Column<string>(type: "text", nullable: true),
                    tx_amount = table.Column<double>(type: "double precision", nullable: true),
                    tx_amount_decimal = table.Column<decimal>(type: "numeric", nullable: true),
                    file_document_id = table.Column<string>(type: "text", nullable: true),
                    uploaded_file_path = table.Column<string>(type: "text", nullable: true),
                    payin_bank_account_id = table.Column<string>(type: "text", nullable: true),
                    payin_bank_code = table.Column<string>(type: "text", nullable: true),
                    payin_bank_account_no = table.Column<string>(type: "text", nullable: true),
                    payin_bank_account_name = table.Column<string>(type: "text", nullable: true),
                    payout_bank_account_id = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PaymentDocuments", x => x.document_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_created_date",
                table: "PaymentDocuments",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_direction",
                table: "PaymentDocuments",
                column: "direction");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_merchant_id",
                table: "PaymentDocuments",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_org_id",
                table: "PaymentDocuments",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_payin_bank_account_id",
                table: "PaymentDocuments",
                column: "payin_bank_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_payment_request_id",
                table: "PaymentDocuments",
                column: "payment_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_status",
                table: "PaymentDocuments",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentDocuments");
        }
    }
}
