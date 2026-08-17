using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class DuplicateRecord_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DuplicateRecords",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dup_type = table.Column<string>(type: "text", nullable: true),
                    first4 = table.Column<string>(type: "text", nullable: true),
                    last4 = table.Column<string>(type: "text", nullable: true),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    document_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateRecords", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateRecords_first4_last4",
                table: "DuplicateRecords",
                columns: new[] { "first4", "last4" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuplicateRecords");
        }
    }
}
