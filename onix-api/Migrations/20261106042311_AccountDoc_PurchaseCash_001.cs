using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AccountDoc_PurchaseCash_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "document_sub_type",
                table: "AccountDocs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reject_reason",
                table: "AccountDocs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "document_sub_type",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "reject_reason",
                table: "AccountDocs");
        }
    }
}
