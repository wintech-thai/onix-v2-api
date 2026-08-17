using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AuditTrack_ApiName_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "api_name",
                table: "AuditTracks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "api_name",
                table: "AuditTracks");
        }
    }
}
