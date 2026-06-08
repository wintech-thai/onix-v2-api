using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class NotiChannel_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotiChannels",
                columns: table => new
                {
                    noti_channel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    channel_name = table.Column<string>(type: "text", nullable: true),
                    channel_description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    events_matched = table.Column<string>(type: "text", nullable: true),
                    message_template = table.Column<string>(type: "text", nullable: true),
                    discord_webhook_url = table.Column<string>(type: "text", nullable: true),
                    telegram_webhook_url = table.Column<string>(type: "text", nullable: true),
                    telegram_chat_id = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotiChannels", x => x.noti_channel_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotiChannels_channel_description",
                table: "NotiChannels",
                column: "channel_description");

            migrationBuilder.CreateIndex(
                name: "IX_NotiChannels_channel_name",
                table: "NotiChannels",
                column: "channel_name");

            migrationBuilder.CreateIndex(
                name: "IX_NotiChannels_org_id_channel_name",
                table: "NotiChannels",
                columns: new[] { "org_id", "channel_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotiChannels_status",
                table: "NotiChannels",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_NotiChannels_tags",
                table: "NotiChannels",
                column: "tags");

            migrationBuilder.CreateIndex(
                name: "IX_NotiChannels_type",
                table: "NotiChannels",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotiChannels");
        }
    }
}
