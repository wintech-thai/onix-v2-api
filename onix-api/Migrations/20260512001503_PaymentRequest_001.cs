using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentRequest_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentRequests",
                columns: table => new
                {
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    ref_id = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    customer_email = table.Column<string>(type: "text", nullable: true),
                    customer_phone = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    bank_code = table.Column<string>(type: "text", nullable: true),
                    bank_account_no = table.Column<string>(type: "text", nullable: true),
                    bank_account_name = table.Column<string>(type: "text", nullable: true),
                    requested_amount = table.Column<double>(type: "double precision", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    direction = table.Column<string>(type: "text", nullable: true),
                    merchant_id = table.Column<string>(type: "text", nullable: true),
                    payment_tx_id = table.Column<string>(type: "text", nullable: true),
                    generated_amount = table.Column<double>(type: "double precision", nullable: true),
                    response_data = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequests", x => x.request_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_bank_account_name",
                table: "PaymentRequests",
                column: "bank_account_name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_bank_account_no",
                table: "PaymentRequests",
                column: "bank_account_no");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_created_date",
                table: "PaymentRequests",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_direction",
                table: "PaymentRequests",
                column: "direction");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_merchant_id",
                table: "PaymentRequests",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_org_id",
                table: "PaymentRequests",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_org_id_ref_id",
                table: "PaymentRequests",
                columns: new[] { "org_id", "ref_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_payment_tx_id",
                table: "PaymentRequests",
                column: "payment_tx_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_ref_id",
                table: "PaymentRequests",
                column: "ref_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_status",
                table: "PaymentRequests",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentRequests");
        }
    }
}
