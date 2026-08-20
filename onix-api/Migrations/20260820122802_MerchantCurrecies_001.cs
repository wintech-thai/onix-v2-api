using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class MerchantCurrecies_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MerchantCurrencies",
                columns: table => new
                {
                    merchant_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    MerchantId = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    currency_category = table.Column<string>(type: "text", nullable: true),
                    is_default_currency = table.Column<bool>(type: "boolean", nullable: false),
                    payin_fee_pct = table.Column<double>(type: "double precision", nullable: true),
                    payin_min_amount = table.Column<double>(type: "double precision", nullable: true),
                    payin_max_amount = table.Column<double>(type: "double precision", nullable: true),
                    pay_indiscard_cent = table.Column<bool>(type: "boolean", nullable: false),
                    payin_include_global_bank_account = table.Column<bool>(type: "boolean", nullable: false),
                    payin_whitelist_bank_account_names = table.Column<string>(type: "text", nullable: true),
                    payin_random_decimal = table.Column<bool>(type: "boolean", nullable: true),
                    payin_daily_tx_amount_limit = table.Column<decimal>(type: "numeric", nullable: true),
                    payin_daily_tx_count_limit = table.Column<decimal>(type: "numeric", nullable: true),
                    payin_expire_minute = table.Column<int>(type: "integer", nullable: true),
                    payout_fee_pct = table.Column<double>(type: "double precision", nullable: true),
                    payout_min_amount = table.Column<double>(type: "double precision", nullable: true),
                    payout_max_amount = table.Column<double>(type: "double precision", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantCurrencies", x => x.merchant_policy_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantCurrencies_currency",
                table: "MerchantCurrencies",
                column: "currency");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantCurrencies_currency_category",
                table: "MerchantCurrencies",
                column: "currency_category");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantCurrencies_MerchantId",
                table: "MerchantCurrencies",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantCurrencies_org_id",
                table: "MerchantCurrencies",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantCurrencies");
        }
    }
}
