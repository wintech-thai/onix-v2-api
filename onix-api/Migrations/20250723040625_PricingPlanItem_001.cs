using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PricingPlanItem_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PricingPlanItems",
                columns: table => new
                {
                    pricing_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    pricing_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rate_type = table.Column<int>(type: "integer", nullable: true),
                    flate_rate = table.Column<double>(type: "double precision", nullable: true),
                    rate_definition = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPlanItems", x => x.pricing_item_id);
                    table.ForeignKey(
                        name: "FK_PricingPlanItems_Items_item_id",
                        column: x => x.item_id,
                        principalTable: "Items",
                        principalColumn: "item_id");
                    table.ForeignKey(
                        name: "FK_PricingPlanItems_PricingPlans_pricing_plan_id",
                        column: x => x.pricing_plan_id,
                        principalTable: "PricingPlans",
                        principalColumn: "plan_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingPlanItems_item_id",
                table: "PricingPlanItems",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_PricingPlanItems_org_id",
                table: "PricingPlanItems",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_PricingPlanItems_pricing_plan_id",
                table: "PricingPlanItems",
                column: "pricing_plan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingPlanItems");
        }
    }
}
