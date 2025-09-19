using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class ScanItem_CustId_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "applied_flag",
                table: "ScanItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "customer_id",
                table: "ScanItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entities_org_id_primary_email",
                table: "Entities",
                columns: new[] { "org_id", "primary_email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entities_org_id_primary_email",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "applied_flag",
                table: "ScanItems");

            migrationBuilder.DropColumn(
                name: "customer_id",
                table: "ScanItems");
        }
    }
}
