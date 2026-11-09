using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AccountDocItem_Fields_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "location_code",
                table: "AccountDocItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location_id",
                table: "AccountDocItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location_name",
                table: "AccountDocItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "location_code",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "location_id",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "location_name",
                table: "AccountDocItems");
        }
    }
}
