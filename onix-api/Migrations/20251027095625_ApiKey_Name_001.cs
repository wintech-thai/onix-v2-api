using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class ApiKey_Name_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "key_name",
                table: "ApiKeys",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_key_name",
                table: "ApiKeys",
                column: "key_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_key_name",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "key_name",
                table: "ApiKeys");
        }
    }
}
