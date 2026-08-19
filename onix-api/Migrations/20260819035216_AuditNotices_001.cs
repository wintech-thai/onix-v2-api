using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AuditNotices_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditNotices",
                columns: table => new
                {
                    notice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    track_model = table.Column<string>(type: "text", nullable: true),
                    row_id = table.Column<string>(type: "text", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditNotices", x => x.notice_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditNotices_org_id",
                table: "AuditNotices",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditNotices_row_id",
                table: "AuditNotices",
                column: "row_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditNotices_track_model",
                table: "AuditNotices",
                column: "track_model");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditNotices");
        }
    }
}
