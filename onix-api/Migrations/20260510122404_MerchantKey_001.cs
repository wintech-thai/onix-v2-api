using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class MerchantKey_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Merchants_org_id_code",
                table: "Merchants");

            migrationBuilder.DropIndex(
                name: "IX_Merchants_org_id_name",
                table: "Merchants");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_code",
                table: "Merchants",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_name",
                table: "Merchants",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Merchants_code",
                table: "Merchants");

            migrationBuilder.DropIndex(
                name: "IX_Merchants_name",
                table: "Merchants");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_org_id_code",
                table: "Merchants",
                columns: new[] { "org_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_org_id_name",
                table: "Merchants",
                columns: new[] { "org_id", "name" },
                unique: true);
        }
    }
}
