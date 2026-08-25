using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class CurrencyAccount_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrencyAccounts",
                columns: table => new
                {
                    currency_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    currency_name = table.Column<string>(type: "text", nullable: true),
                    currency_category = table.Column<string>(type: "text", nullable: true),
                    account_kyc_name = table.Column<string>(type: "text", nullable: true),
                    account_kyc_id = table.Column<string>(type: "text", nullable: true),
                    account_kyc_email = table.Column<string>(type: "text", nullable: true),
                    account_kyc_phone = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    account_type = table.Column<string>(type: "text", nullable: true),
                    account_level = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    crypto_wallet_id = table.Column<string>(type: "text", nullable: true),
                    crypto_wallet_network = table.Column<string>(type: "text", nullable: true),
                    crypto_wallet_type = table.Column<string>(type: "text", nullable: true),
                    crypto_derivation_path = table.Column<string>(type: "text", nullable: true),
                    crypto_qr_scheme = table.Column<string>(type: "text", nullable: true),
                    crypto_address_prefix = table.Column<string>(type: "text", nullable: true),
                    crypto_token_contract = table.Column<string>(type: "text", nullable: true),
                    crypto_decimal = table.Column<int>(type: "integer", nullable: false),
                    crypto_extended_public_key = table.Column<string>(type: "text", nullable: true),
                    crypto_next_address_index = table.Column<int>(type: "integer", nullable: false),
                    crypto_address_branch = table.Column<int>(type: "integer", nullable: false),
                    bank_code = table.Column<string>(type: "text", nullable: true),
                    bank_name = table.Column<string>(type: "text", nullable: true),
                    bank_account_name = table.Column<string>(type: "text", nullable: true),
                    bank_account_no = table.Column<string>(type: "text", nullable: true),
                    bank_promptpay_id = table.Column<string>(type: "text", nullable: true),
                    bank_account_type = table.Column<string>(type: "text", nullable: true),
                    bank_config = table.Column<string>(type: "text", nullable: true),
                    bank_is_native_qr_support = table.Column<bool>(type: "boolean", nullable: false),
                    tx_min_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    tx_max_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    daily_total_amount_limit = table.Column<decimal>(type: "numeric", nullable: true),
                    daily_total_count_limit = table.Column<int>(type: "integer", nullable: true),
                    current_total_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    current_total_count = table.Column<int>(type: "integer", nullable: true),
                    current_balance = table.Column<decimal>(type: "numeric", nullable: true),
                    is_random_cent = table.Column<bool>(type: "boolean", nullable: true),
                    decimal_action = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyAccounts", x => x.currency_account_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_account_kyc_name",
                table: "CurrencyAccounts",
                column: "account_kyc_name");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_account_type",
                table: "CurrencyAccounts",
                column: "account_type");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_crypto_wallet_id",
                table: "CurrencyAccounts",
                column: "crypto_wallet_id");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_crypto_wallet_network",
                table: "CurrencyAccounts",
                column: "crypto_wallet_network");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_currency",
                table: "CurrencyAccounts",
                column: "currency");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_currency_category",
                table: "CurrencyAccounts",
                column: "currency_category");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_org_id",
                table: "CurrencyAccounts",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyAccounts");
        }
    }
}
