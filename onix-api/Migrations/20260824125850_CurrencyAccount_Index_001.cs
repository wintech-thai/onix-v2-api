using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class CurrencyAccount_Index_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_bank_account_name",
                table: "CurrencyAccounts",
                column: "bank_account_name");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_bank_account_no",
                table: "CurrencyAccounts",
                column: "bank_account_no");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_bank_code",
                table: "CurrencyAccounts",
                column: "bank_code");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_crypto_extended_public_key",
                table: "CurrencyAccounts",
                column: "crypto_extended_public_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CurrencyAccounts_bank_account_name",
                table: "CurrencyAccounts");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyAccounts_bank_account_no",
                table: "CurrencyAccounts");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyAccounts_bank_code",
                table: "CurrencyAccounts");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyAccounts_crypto_extended_public_key",
                table: "CurrencyAccounts");
        }
    }
}
