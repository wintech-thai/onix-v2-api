using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Stat_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stats",
                columns: table => new
                {
                    stat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    stat_code = table.Column<string>(type: "text", nullable: true),
                    balance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tx_in = table.Column<long>(type: "bigint", nullable: true),
                    tx_out = table.Column<long>(type: "bigint", nullable: true),
                    balance_begin = table.Column<long>(type: "bigint", nullable: true),
                    balance_end = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stats", x => x.stat_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stats_org_id",
                table: "Stats",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_Stats_stat_code",
                table: "Stats",
                column: "stat_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stats");
        }
    }
}
