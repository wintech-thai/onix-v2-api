using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "risk_policy_id",
                table: "Merchants",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationPolicies",
                columns: table => new
                {
                    org_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    web_whitelist_ips = table.Column<string>(type: "text", nullable: true),
                    api_whitelist_ips = table.Column<string>(type: "text", nullable: true),
                    web_blacklist_ips = table.Column<string>(type: "text", nullable: true),
                    api_blacklist_ips = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationPolicies", x => x.org_policy_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_risk_policy_id",
                table: "Merchants",
                column: "risk_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPolicies_org_id",
                table: "OrganizationPolicies",
                column: "org_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationPolicies");

            migrationBuilder.DropIndex(
                name: "IX_Merchants_risk_policy_id",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "risk_policy_id",
                table: "Merchants");
        }
    }
}
