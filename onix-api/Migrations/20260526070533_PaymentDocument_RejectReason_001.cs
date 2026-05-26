using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentDocument_RejectReason_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ref_id",
                table: "PaymentDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reject_reason",
                table: "PaymentDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_ref_id",
                table: "PaymentDocuments",
                column: "ref_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentDocuments_ref_id",
                table: "PaymentDocuments");

            migrationBuilder.DropColumn(
                name: "ref_id",
                table: "PaymentDocuments");

            migrationBuilder.DropColumn(
                name: "reject_reason",
                table: "PaymentDocuments");
        }
    }
}
