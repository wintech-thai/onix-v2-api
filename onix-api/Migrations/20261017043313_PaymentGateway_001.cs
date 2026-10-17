using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentGateway_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fee_deduction_source",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fee_deduction_source",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "payment_gateway_id",
                table: "Merchants",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentGateways",
                columns: table => new
                {
                    payment_gateway_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    scheme = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    endpoint_base_url = table.Column<string>(type: "text", nullable: true),
                    endpoint_org_id = table.Column<string>(type: "text", nullable: true),
                    endpoint_merchant_id = table.Column<string>(type: "text", nullable: true),
                    api_key = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGateways", x => x.payment_gateway_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_org_id",
                table: "PaymentGateways",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentGateways");

            migrationBuilder.DropColumn(
                name: "fee_deduction_source",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "fee_deduction_source",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "payment_gateway_id",
                table: "Merchants");
        }
    }
}
