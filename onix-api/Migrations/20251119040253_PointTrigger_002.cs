using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PointTrigger_002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "is_rule_match",
                table: "PointsTriggers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_rule_match",
                table: "PointsTriggers");
        }
    }
}
