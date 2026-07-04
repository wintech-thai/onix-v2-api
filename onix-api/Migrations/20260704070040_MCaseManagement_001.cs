using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class MCaseManagement_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseManagements",
                columns: table => new
                {
                    case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    @ref = table.Column<string>(name: "ref", type: "text", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_by = table.Column<string>(type: "text", nullable: true),
                    closed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseManagements", x => x.case_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseManagements_org_id",
                table: "CaseManagements",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseManagements_org_id_ref",
                table: "CaseManagements",
                columns: new[] { "org_id", "ref" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseManagements_ref",
                table: "CaseManagements",
                column: "ref");

            migrationBuilder.CreateIndex(
                name: "IX_CaseManagements_status",
                table: "CaseManagements",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseManagements");
        }
    }
}
