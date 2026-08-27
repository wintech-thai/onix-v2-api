using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Ioc_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Iocs",
                columns: table => new
                {
                    oic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    ioc_type = table.Column<string>(type: "text", nullable: true),
                    ioc_value = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: true),
                    risk_score = table.Column<int>(type: "integer", nullable: false),
                    confidence_score = table.Column<int>(type: "integer", nullable: false),
                    reputation = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    seen_count = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_seen_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_seen_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Iocs", x => x.oic_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Iocs_ioc_type",
                table: "Iocs",
                column: "ioc_type");

            migrationBuilder.CreateIndex(
                name: "IX_Iocs_ioc_value",
                table: "Iocs",
                column: "ioc_value");

            migrationBuilder.CreateIndex(
                name: "IX_Iocs_org_id",
                table: "Iocs",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_Iocs_org_id_ioc_type_ioc_value",
                table: "Iocs",
                columns: new[] { "org_id", "ioc_type", "ioc_value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Iocs");
        }
    }
}
