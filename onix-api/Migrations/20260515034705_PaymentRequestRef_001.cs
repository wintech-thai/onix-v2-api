using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentRequestRef_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ref_id1",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ref_id2",
                table: "PaymentRequests",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_ref_id1",
                table: "PaymentRequests",
                column: "ref_id1");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_ref_id2",
                table: "PaymentRequests",
                column: "ref_id2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_ref_id1",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_ref_id2",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "ref_id1",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "ref_id2",
                table: "PaymentRequests");
        }
    }
}
