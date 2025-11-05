using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Item_Balance_002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemBalances",
                columns: table => new
                {
                    balance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    stat_code = table.Column<string>(type: "text", nullable: true),
                    item_id = table.Column<string>(type: "text", nullable: true),
                    balance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    balance_date_key = table.Column<string>(type: "text", nullable: true),
                    tx_in = table.Column<long>(type: "bigint", nullable: true),
                    tx_out = table.Column<long>(type: "bigint", nullable: true),
                    balance_begin = table.Column<long>(type: "bigint", nullable: true),
                    balance_end = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemBalances", x => x.balance_id);
                });

            migrationBuilder.CreateTable(
                name: "ItemTxs",
                columns: table => new
                {
                    tx_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    item_id = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tx_amount = table.Column<long>(type: "bigint", nullable: true),
                    tx_type = table.Column<int>(type: "integer", nullable: true),
                    current_balance = table.Column<long>(type: "bigint", nullable: true),
                    previous_balance = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTxs", x => x.tx_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemBalances_balance_date",
                table: "ItemBalances",
                column: "balance_date");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBalances_balance_date_key",
                table: "ItemBalances",
                column: "balance_date_key");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBalances_item_id",
                table: "ItemBalances",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBalances_org_id",
                table: "ItemBalances",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBalances_stat_code",
                table: "ItemBalances",
                column: "stat_code");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTxs_item_id",
                table: "ItemTxs",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTxs_org_id",
                table: "ItemTxs",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemBalances");

            migrationBuilder.DropTable(
                name: "ItemTxs");
        }
    }
}
