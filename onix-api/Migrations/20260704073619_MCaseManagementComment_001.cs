using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class MCaseManagementComment_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseManagementComments",
                columns: table => new
                {
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_id = table.Column<Guid>(type: "uuid", nullable: true),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    author_type = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseManagementComments", x => x.comment_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseManagementComments_case_id",
                table: "CaseManagementComments",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseManagementComments_org_id",
                table: "CaseManagementComments",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseManagementComments");
        }
    }
}
