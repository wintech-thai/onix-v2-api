using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class DocumentNumber_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentNumberConfig",
                columns: table => new
                {
                    document_number_config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    document_type = table.Column<string>(type: "text", nullable: true),
                    document_format = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    seq_digit = table.Column<int>(type: "integer", nullable: true),
                    reset_type = table.Column<string>(type: "text", nullable: true),
                    year_offset = table.Column<int>(type: "integer", nullable: true),
                    current_sequence_key = table.Column<string>(type: "text", nullable: true),
                    current_sequence_no = table.Column<int>(type: "integer", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentNumberConfig", x => x.document_number_config_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNumberConfig_org_id",
                table: "DocumentNumberConfig",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNumberConfig_org_id_document_type",
                table: "DocumentNumberConfig",
                columns: new[] { "org_id", "document_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentNumberConfig");
        }
    }
}
