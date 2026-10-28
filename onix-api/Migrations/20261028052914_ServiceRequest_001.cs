using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class ServiceRequest_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    service_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    service_type = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    otp = table.Column<string>(type: "text", nullable: true),
                    expired_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lang = table.Column<string>(type: "text", nullable: true),
                    admin_domain = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    bank_code = table.Column<string>(type: "text", nullable: true),
                    bank_account_number = table.Column<string>(type: "text", nullable: true),
                    bank_account_name = table.Column<string>(type: "text", nullable: true),
                    prompt_pay_id = table.Column<string>(type: "text", nullable: true),
                    book_bank_file_document_id = table.Column<string>(type: "text", nullable: true),
                    shop_code = table.Column<string>(type: "text", nullable: true),
                    username = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.service_request_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_org_id",
                table: "ServiceRequests",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_service_type",
                table: "ServiceRequests",
                column: "service_type");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_status",
                table: "ServiceRequests",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequests");
        }
    }
}
