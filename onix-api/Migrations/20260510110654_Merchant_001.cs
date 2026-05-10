using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Merchant_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Merchants",
                columns: table => new
                {
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    contact_phone = table.Column<string>(type: "text", nullable: true),
                    payin_fee_pct = table.Column<double>(type: "double precision", nullable: true),
                    payout_fee_pct = table.Column<double>(type: "double precision", nullable: true),
                    payin_min_amount = table.Column<double>(type: "double precision", nullable: true),
                    payin_max_amount = table.Column<double>(type: "double precision", nullable: true),
                    payout_min_amount = table.Column<double>(type: "double precision", nullable: true),
                    payout_max_amount = table.Column<double>(type: "double precision", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.merchant_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_org_id",
                table: "Merchants",
                column: "org_id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Merchants");
        }
    }
}
