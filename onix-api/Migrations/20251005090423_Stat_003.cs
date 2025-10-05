using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Stat_003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "balance_date_key",
                table: "Stats",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stats_org_id_stat_code_balance_date_key",
                table: "Stats",
                columns: new[] { "org_id", "stat_code", "balance_date_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stats_org_id_stat_code_balance_date_key",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "balance_date_key",
                table: "Stats");
        }
    }
}
