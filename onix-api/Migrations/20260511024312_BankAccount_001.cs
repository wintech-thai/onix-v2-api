using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class BankAccount_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    bank_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    bank_code = table.Column<string>(type: "text", nullable: true),
                    account_number = table.Column<string>(type: "text", nullable: true),
                    account_name = table.Column<string>(type: "text", nullable: true),
                    promptpay_id = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    account_type = table.Column<string>(type: "text", nullable: true),
                    account_category = table.Column<double>(type: "double precision", nullable: true),
                    account_level = table.Column<double>(type: "double precision", nullable: true),
                    payin_min_amount = table.Column<double>(type: "double precision", nullable: true),
                    payin_max_amount = table.Column<double>(type: "double precision", nullable: true),
                    payout_min_amount = table.Column<double>(type: "double precision", nullable: true),
                    payout_max_amount = table.Column<double>(type: "double precision", nullable: true),
                    daily_quota = table.Column<double>(type: "double precision", nullable: true),
                    current_daily_payin_amount = table.Column<double>(type: "double precision", nullable: true),
                    current_daily_payin_count = table.Column<double>(type: "double precision", nullable: true),
                    current_balance = table.Column<double>(type: "double precision", nullable: true),
                    daily_payin_count_quota = table.Column<double>(type: "double precision", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.bank_account_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_org_id",
                table: "BankAccounts",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_org_id_account_number",
                table: "BankAccounts",
                columns: new[] { "org_id", "account_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_org_id_bank_code_account_name",
                table: "BankAccounts",
                columns: new[] { "org_id", "bank_code", "account_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccounts");
        }
    }
}
