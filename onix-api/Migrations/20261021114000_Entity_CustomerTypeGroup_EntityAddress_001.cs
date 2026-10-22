using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Entity_CustomerTypeGroup_EntityAddress_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_group_id",
                table: "Entities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "customer_type_id",
                table: "Entities",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EntityAddresses",
                columns: table => new
                {
                    entity_address_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    entity_id = table.Column<string>(type: "text", nullable: true),
                    address_type = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityAddresses", x => x.entity_address_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityAddresses_entity_id",
                table: "EntityAddresses",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAddresses_org_id",
                table: "EntityAddresses",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityAddresses");

            migrationBuilder.DropColumn(
                name: "customer_group_id",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "customer_type_id",
                table: "Entities");
        }
    }
}
