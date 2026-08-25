using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class CurrencyAccount_Merchant_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrencyAccountMerchants",
                columns: table => new
                {
                    account_merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    currency_account_id = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    MerchantId = table.Column<string>(type: "text", nullable: true),
                    account_category = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyAccountMerchants", x => x.account_merchant_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccountMerchants_currency",
                table: "CurrencyAccountMerchants",
                column: "currency");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccountMerchants_currency_account_id",
                table: "CurrencyAccountMerchants",
                column: "currency_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccountMerchants_MerchantId",
                table: "CurrencyAccountMerchants",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccountMerchants_org_id",
                table: "CurrencyAccountMerchants",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyAccountMerchants");
        }
    }
}
