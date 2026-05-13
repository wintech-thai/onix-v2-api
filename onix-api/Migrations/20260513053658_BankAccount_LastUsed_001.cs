using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class BankAccount_LastUsed_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_used_date",
                table: "BankAccounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_account_category",
                table: "BankAccounts",
                column: "account_category");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_account_type",
                table: "BankAccounts",
                column: "account_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_account_category",
                table: "BankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_account_type",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "last_used_date",
                table: "BankAccounts");
        }
    }
}
