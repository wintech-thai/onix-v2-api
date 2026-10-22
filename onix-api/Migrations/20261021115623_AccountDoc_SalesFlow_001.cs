using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class AccountDoc_SalesFlow_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "approved_date",
                table: "AccountDocs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "document_no",
                table: "AccountDocs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "due_date",
                table: "AccountDocs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "paid_amount",
                table: "AccountDocs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "AccountDocs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "status_date",
                table: "AccountDocs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "product_code",
                table: "AccountDocItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "product_desc",
                table: "AccountDocItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "product_type",
                table: "AccountDocItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approved_date",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "document_no",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "due_date",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "paid_amount",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "status_date",
                table: "AccountDocs");

            migrationBuilder.DropColumn(
                name: "product_code",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "product_desc",
                table: "AccountDocItems");

            migrationBuilder.DropColumn(
                name: "product_type",
                table: "AccountDocItems");
        }
    }
}
