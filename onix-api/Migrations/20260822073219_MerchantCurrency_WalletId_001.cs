using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class MerchantCurrency_WalletId_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ref_id",
                table: "Wallets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "wallet_id",
                table: "MerchantCurrencies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ref_id",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "wallet_id",
                table: "MerchantCurrencies");
        }
    }
}
