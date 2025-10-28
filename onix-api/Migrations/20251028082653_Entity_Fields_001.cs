using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Entity_Fields_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "primary_email_status",
                table: "Entities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "primary_phone",
                table: "Entities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "primary_phone_status",
                table: "Entities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "total_point",
                table: "Entities",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "primary_email_status",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "primary_phone",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "primary_phone_status",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "total_point",
                table: "Entities");
        }
    }
}
