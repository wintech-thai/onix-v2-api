using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Limit_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Limits",
                columns: table => new
                {
                    limit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    stat_code = table.Column<string>(type: "text", nullable: true),
                    limit = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Limits", x => x.limit_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Limits_org_id",
                table: "Limits",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_Limits_org_id_stat_code",
                table: "Limits",
                columns: new[] { "org_id", "stat_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Limits_stat_code",
                table: "Limits",
                column: "stat_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Limits");
        }
    }
}
