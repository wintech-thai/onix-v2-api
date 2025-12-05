using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class ScanItemAction_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "action_name",
                table: "ScanItemActions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "ScanItemActions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "is_default",
                table: "ScanItemActions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tags",
                table: "ScanItemActions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanItemActions_action_name",
                table: "ScanItemActions",
                column: "action_name");

            migrationBuilder.CreateIndex(
                name: "IX_ScanItemActions_org_id_action_name",
                table: "ScanItemActions",
                columns: new[] { "org_id", "action_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScanItemActions_action_name",
                table: "ScanItemActions");

            migrationBuilder.DropIndex(
                name: "IX_ScanItemActions_org_id_action_name",
                table: "ScanItemActions");

            migrationBuilder.DropColumn(
                name: "action_name",
                table: "ScanItemActions");

            migrationBuilder.DropColumn(
                name: "description",
                table: "ScanItemActions");

            migrationBuilder.DropColumn(
                name: "is_default",
                table: "ScanItemActions");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "ScanItemActions");
        }
    }
}
