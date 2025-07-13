using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Item_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "master_ref_id",
                table: "Cycles",
                newName: "cycle_id");

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    item_type = table.Column<int>(type: "integer", nullable: true),
                    narrative = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.item_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_org_id",
                table: "Items",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_Items_org_id_code",
                table: "Items",
                columns: new[] { "org_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.RenameColumn(
                name: "cycle_id",
                table: "Cycles",
                newName: "master_ref_id");
        }
    }
}
