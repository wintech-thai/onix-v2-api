using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PricePlan_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PricingPlans",
                columns: table => new
                {
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    cycle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPlans", x => x.plan_id);
                    table.ForeignKey(
                        name: "FK_PricingPlans_Cycles_cycle_id",
                        column: x => x.cycle_id,
                        principalTable: "Cycles",
                        principalColumn: "cycle_id");
                    table.ForeignKey(
                        name: "FK_PricingPlans_Entities_customer_id",
                        column: x => x.customer_id,
                        principalTable: "Entities",
                        principalColumn: "entity_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingPlans_customer_id",
                table: "PricingPlans",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_PricingPlans_cycle_id",
                table: "PricingPlans",
                column: "cycle_id");

            migrationBuilder.CreateIndex(
                name: "IX_PricingPlans_org_id",
                table: "PricingPlans",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_PricingPlans_org_id_code",
                table: "PricingPlans",
                columns: new[] { "org_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingPlans");
        }
    }
}
