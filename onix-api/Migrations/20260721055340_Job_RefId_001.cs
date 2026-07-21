using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Job_RefId_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ref_id",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ref_id",
                table: "Jobs",
                column: "ref_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_ref_id",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ref_id",
                table: "Jobs");
        }
    }
}
