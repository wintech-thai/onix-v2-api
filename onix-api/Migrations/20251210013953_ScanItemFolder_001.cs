using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class ScanItemFolder_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScanItemFolders",
                columns: table => new
                {
                    folder_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    folder_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    scan_item_count = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanItemFolders", x => x.folder_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScanItemFolders_folder_name",
                table: "ScanItemFolders",
                column: "folder_name");

            migrationBuilder.CreateIndex(
                name: "IX_ScanItemFolders_org_id",
                table: "ScanItemFolders",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_ScanItemFolders_org_id_folder_name",
                table: "ScanItemFolders",
                columns: new[] { "org_id", "folder_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScanItemFolders");
        }
    }
}
