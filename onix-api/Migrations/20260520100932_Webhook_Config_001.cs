using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Webhook_Config_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebhookConfigs",
                columns: table => new
                {
                    webhook_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    endpoint_url = table.Column<string>(type: "text", nullable: true),
                    http_method = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true),
                    secret_key = table.Column<string>(type: "text", nullable: true),
                    signature_algorithm = table.Column<string>(type: "text", nullable: true),
                    headers_definition = table.Column<string>(type: "text", nullable: true),
                    timeout_sec = table.Column<int>(type: "integer", nullable: true),
                    max_retry_count = table.Column<int>(type: "integer", nullable: true),
                    retry_interval_sec = table.Column<int>(type: "integer", nullable: true),
                    payload_version = table.Column<string>(type: "text", nullable: true),
                    last_status = table.Column<string>(type: "text", nullable: true),
                    last_called_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookConfigs", x => x.webhook_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookConfigs_merchant_id_event_name",
                table: "WebhookConfigs",
                columns: new[] { "merchant_id", "event_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookConfigs_org_id_event_name",
                table: "WebhookConfigs",
                columns: new[] { "org_id", "event_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebhookConfigs");
        }
    }
}
