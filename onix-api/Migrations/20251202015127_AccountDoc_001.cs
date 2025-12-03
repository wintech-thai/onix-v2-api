using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AccountDoc_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountDocItems",
                columns: table => new
                {
                    doc_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    account_doc_id = table.Column<string>(type: "text", nullable: true),
                    product_id = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    quantity = table.Column<double>(type: "double precision", nullable: true),
                    unit_price = table.Column<double>(type: "double precision", nullable: true),
                    total_price = table.Column<double>(type: "double precision", nullable: true),
                    document_params = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountDocItems", x => x.doc_item_id);
                });

            migrationBuilder.CreateTable(
                name: "AccountDocs",
                columns: table => new
                {
                    doc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    entity_id = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    document_type = table.Column<string>(type: "text", nullable: true),
                    product_type = table.Column<string>(type: "text", nullable: true),
                    document_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_price = table.Column<double>(type: "double precision", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountDocs", x => x.doc_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountDocItems_account_doc_id",
                table: "AccountDocItems",
                column: "account_doc_id");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDocItems_org_id",
                table: "AccountDocItems",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDocItems_product_id",
                table: "AccountDocItems",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDocs_code",
                table: "AccountDocs",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDocs_document_type",
                table: "AccountDocs",
                column: "document_type");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDocs_entity_id",
                table: "AccountDocs",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDocs_org_id",
                table: "AccountDocs",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDocs_org_id_code",
                table: "AccountDocs",
                columns: new[] { "org_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountDocs_product_type",
                table: "AccountDocs",
                column: "product_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountDocItems");

            migrationBuilder.DropTable(
                name: "AccountDocs");
        }
    }
}
