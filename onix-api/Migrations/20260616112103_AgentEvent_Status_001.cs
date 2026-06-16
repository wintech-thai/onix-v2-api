using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AgentEvent_Status_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "AgentEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_desc",
                table: "AgentEvents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "AgentEvents");

            migrationBuilder.DropColumn(
                name: "status_desc",
                table: "AgentEvents");
        }
    }
}
