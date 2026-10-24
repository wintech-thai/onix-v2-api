using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AccountDoc_VatWithholdingDiscount_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "discount_amount",
                table: "AccountDocs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "grand_total",
                table: "AccountDocs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "net_amount",
                table: "AccountDocs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "total_with_vat",
                table: "AccountDocs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "vat_amount",
                table: "AccountDocs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "vat_rate",
                table: "AccountDocs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "withholding_tax_amount",
                table: "AccountDocs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "item_discount_amount",
                table: "AccountDocItems",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_discount_type",
                table: "AccountDocItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "item_discount_value",
                table: "AccountDocItems",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "net_amount",
                table: "AccountDocItems",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "vat_amount",
                table: "AccountDocItems",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "vat_enabled",
                table: "AccountDocItems",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "withholding_tax_amount",
                table: "AccountDocItems",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "withholding_tax_enabled",
                table: "AccountDocItems",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "withholding_tax_rate",
                table: "AccountDocItems",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "grand_total",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "net_amount",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "total_with_vat",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "vat_amount",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "vat_rate",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "withholding_tax_amount",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "item_discount_amount",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "item_discount_type",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "item_discount_value",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "net_amount",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "vat_amount",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "vat_enabled",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "withholding_tax_amount",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "withholding_tax_enabled",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "withholding_tax_rate",
                table: "AccountDocItems");
        }
    }
}
