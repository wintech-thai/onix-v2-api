using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Voucher_Barcode_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "barcode",
                table: "Vouchers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_barcode",
                table: "Vouchers",
                column: "barcode");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_piin",
                table: "Vouchers",
                column: "piin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vouchers_barcode",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_piin",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "barcode",
                table: "Vouchers");
        }
    }
}
