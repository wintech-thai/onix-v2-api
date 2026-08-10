using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class Organization_CompanyProfile_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "logo_image_base64",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "org_name_th",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "org_name_en",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_name_th",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_name_en",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_th",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_en",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fax",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "Organizations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "logo_image_base64", table: "Organizations");
            migrationBuilder.DropColumn(name: "org_name_th", table: "Organizations");
            migrationBuilder.DropColumn(name: "org_name_en", table: "Organizations");
            migrationBuilder.DropColumn(name: "contact_name_th", table: "Organizations");
            migrationBuilder.DropColumn(name: "contact_name_en", table: "Organizations");
            migrationBuilder.DropColumn(name: "address_th", table: "Organizations");
            migrationBuilder.DropColumn(name: "address_en", table: "Organizations");
            migrationBuilder.DropColumn(name: "phone", table: "Organizations");
            migrationBuilder.DropColumn(name: "fax", table: "Organizations");
            migrationBuilder.DropColumn(name: "email", table: "Organizations");
        }
    }
}
