using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class ScanItem_002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "item_group",
                table: "ScanItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "run_id",
                table: "ScanItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sequence_no",
                table: "ScanItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "uploaded_path",
                table: "ScanItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "ScanItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "item_group",
                table: "ScanItems");

            migrationBuilder.DropColumn(
                name: "run_id",
                table: "ScanItems");

            migrationBuilder.DropColumn(
                name: "sequence_no",
                table: "ScanItems");

            migrationBuilder.DropColumn(
                name: "uploaded_path",
                table: "ScanItems");

            migrationBuilder.DropColumn(
                name: "url",
                table: "ScanItems");
        }
    }
}
