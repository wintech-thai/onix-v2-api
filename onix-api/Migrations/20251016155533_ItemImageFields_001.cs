using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class ItemImageFields_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "file_size_byte",
                table: "ItemImages",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "file_type",
                table: "ItemImages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "image_height",
                table: "ItemImages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "image_width",
                table: "ItemImages",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_size_byte",
                table: "ItemImages");

            migrationBuilder.DropColumn(
                name: "file_type",
                table: "ItemImages");

            migrationBuilder.DropColumn(
                name: "image_height",
                table: "ItemImages");

            migrationBuilder.DropColumn(
                name: "image_width",
                table: "ItemImages");
        }
    }
}
