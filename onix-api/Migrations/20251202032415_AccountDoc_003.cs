using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AccountDoc_003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "incentive_price",
                table: "AccountDocItems",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "incentive_rate",
                table: "AccountDocItems",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "incentive_price",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "incentive_rate",
                table: "AccountDocItems");
        }
    }
}
