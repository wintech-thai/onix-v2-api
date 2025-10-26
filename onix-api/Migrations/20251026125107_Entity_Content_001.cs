using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Entity_Content_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content",
                table: "Entities",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content",
                table: "Entities");
        }
    }
}
