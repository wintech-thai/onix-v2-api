using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Entity_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    entity_type = table.Column<int>(type: "integer", nullable: true),
                    entity_category = table.Column<int>(type: "integer", nullable: true),
                    credit_term_day = table.Column<int>(type: "integer", nullable: true),
                    credit_amount = table.Column<double>(type: "double precision", nullable: true),
                    tax_id = table.Column<string>(type: "text", nullable: true),
                    national_card_id = table.Column<string>(type: "text", nullable: true),
                    primary_email = table.Column<string>(type: "text", nullable: true),
                    secondary_email = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entities", x => x.entity_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entities_org_id",
                table: "Entities",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_org_id_code",
                table: "Entities",
                columns: new[] { "org_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entities");
        }
    }
}
