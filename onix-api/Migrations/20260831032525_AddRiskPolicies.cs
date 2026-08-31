using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiskPolicies",
                columns: table => new
                {
                    risk_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    allow_blank_payer_name = table.Column<bool>(type: "boolean", nullable: false),
                    allow_unknown_payer_name = table.Column<bool>(type: "boolean", nullable: false),
                    allow_suspicious_payer_name = table.Column<bool>(type: "boolean", nullable: false),
                    allow_malicious_payer_name = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskPolicies", x => x.risk_policy_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskPolicies_name",
                table: "RiskPolicies",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_RiskPolicies_org_id",
                table: "RiskPolicies",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_RiskPolicies_status",
                table: "RiskPolicies",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskPolicies");
        }
    }
}
