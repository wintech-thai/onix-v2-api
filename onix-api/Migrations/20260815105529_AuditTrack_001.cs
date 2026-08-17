using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AuditTrack_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditTracks",
                columns: table => new
                {
                    track_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    track_model = table.Column<string>(type: "text", nullable: true),
                    row_id = table.Column<string>(type: "text", nullable: true),
                    action_name = table.Column<string>(type: "text", nullable: true),
                    action_by = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    current_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTracks", x => x.track_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTracks_org_id",
                table: "AuditTracks",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTracks_row_id",
                table: "AuditTracks",
                column: "row_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTracks_track_model",
                table: "AuditTracks",
                column: "track_model");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditTracks");
        }
    }
}
