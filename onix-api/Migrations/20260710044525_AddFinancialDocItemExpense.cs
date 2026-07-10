using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialDocItemExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "financial_doc_item_expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    financial_doc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    expense_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expense_code = table.Column<string>(type: "text", nullable: true),
                    expense_desc = table.Column<string>(type: "text", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_doc_item_expenses", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_financial_doc_item_expenses_financial_doc_id",
                table: "financial_doc_item_expenses",
                column: "financial_doc_id");

            migrationBuilder.CreateIndex(
                name: "IX_financial_doc_item_expenses_org_id",
                table: "financial_doc_item_expenses",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "financial_doc_item_expenses");
        }
    }
}
