using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Financial_Doc_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialDocs",
                columns: table => new
                {
                    financial_doc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    document_no = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    to_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expense_items_def = table.Column<string>(type: "text", nullable: true),
                    revenue_items_def = table.Column<string>(type: "text", nullable: true),
                    sharing_items_def = table.Column<string>(type: "text", nullable: true),
                    total_revenue = table.Column<decimal>(type: "numeric", nullable: true),
                    total_expense = table.Column<decimal>(type: "numeric", nullable: true),
                    profit_loss = table.Column<decimal>(type: "numeric", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialDocs", x => x.financial_doc_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocs_org_id",
                table: "FinancialDocs",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocs_org_id_document_no",
                table: "FinancialDocs",
                columns: new[] { "org_id", "document_no" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialDocs");
        }
    }
}
