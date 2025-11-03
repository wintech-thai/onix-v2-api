using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Points_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PointBalances",
                columns: table => new
                {
                    stat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    stat_code = table.Column<string>(type: "text", nullable: true),
                    wallet_id = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PointBalances", x => x.stat_id);
                });

            migrationBuilder.CreateTable(
                name: "PointsTxs",
                columns: table => new
                {
                    tx_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    wallet_id = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PointsTxs", x => x.tx_id);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    customer_id = table.Column<string>(type: "text", nullable: true),
                    point_balance = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.wallet_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointBalances_balance_date",
                table: "PointBalances",
                column: "balance_date");

            migrationBuilder.CreateIndex(
                name: "IX_PointBalances_org_id",
                table: "PointBalances",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_PointBalances_stat_code",
                table: "PointBalances",
                column: "stat_code");

            migrationBuilder.CreateIndex(
                name: "IX_PointBalances_wallet_id",
                table: "PointBalances",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTxs_org_id",
                table: "PointsTxs",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTxs_wallet_id",
                table: "PointsTxs",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_org_id",
                table: "Wallets",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointBalances");

            migrationBuilder.DropTable(
                name: "PointsTxs");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
