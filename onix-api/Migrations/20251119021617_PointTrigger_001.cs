using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PointTrigger_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PointsTriggers",
                columns: table => new
                {
                    trigger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    wallet_id = table.Column<string>(type: "text", nullable: true),
                    trigger_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    triggered_event = table.Column<string>(type: "text", nullable: true),
                    trigger_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    points = table.Column<int>(type: "integer", nullable: true),
                    trigger_params = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsTriggers", x => x.trigger_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointsRules_rule_name",
                table: "PointsRules",
                column: "rule_name");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTriggers_org_id",
                table: "PointsTriggers",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTriggers_org_id_trigger_name",
                table: "PointsTriggers",
                columns: new[] { "org_id", "trigger_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointsTriggers_trigger_name",
                table: "PointsTriggers",
                column: "trigger_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointsTriggers");

            migrationBuilder.DropIndex(
                name: "IX_PointsRules_rule_name",
                table: "PointsRules");
        }
    }
}
