using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class ScanItemTemplate_002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "ScanItemTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tags",
                table: "ScanItemTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "template_name",
                table: "ScanItemTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanItemTemplates_org_id_template_name",
                table: "ScanItemTemplates",
                columns: new[] { "org_id", "template_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanItemTemplates_template_name",
                table: "ScanItemTemplates",
                column: "template_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScanItemTemplates_org_id_template_name",
                table: "ScanItemTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ScanItemTemplates_template_name",
                table: "ScanItemTemplates");

            migrationBuilder.DropColumn(
                name: "description",
                table: "ScanItemTemplates");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "ScanItemTemplates");

            migrationBuilder.DropColumn(
                name: "template_name",
                table: "ScanItemTemplates");
        }
    }
}
