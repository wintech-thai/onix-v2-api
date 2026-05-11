using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class BankAccountMerchant_002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankAccountMerchants",
                columns: table => new
                {
                    bankaccount_merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    bank_account_id = table.Column<string>(type: "text", nullable: true),
                    merchant_id = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountMerchants", x => x.bankaccount_merchant_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountMerchants_org_id",
                table: "BankAccountMerchants",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccountMerchants");
        }
    }
}
