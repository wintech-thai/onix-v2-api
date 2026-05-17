using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentRequest_ProcessingMessages_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "processing_messages",
                table: "PaymentRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "processing_messages",
                table: "PaymentRequests");
        }
    }
}
