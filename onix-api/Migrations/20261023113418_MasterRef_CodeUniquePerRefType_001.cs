using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace onix.api.Migrations
{
    /// <inheritdoc />
    public partial class MasterRef_CodeUniquePerRefType_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MasterRefs_org_id_code",
                table: "MasterRefs");

            migrationBuilder.CreateIndex(
                name: "IX_MasterRefs_org_id_ref_type_code",
                table: "MasterRefs",
                columns: new[] { "org_id", "ref_type", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MasterRefs_org_id_ref_type_code",
                table: "MasterRefs");

            migrationBuilder.CreateIndex(
                name: "IX_MasterRefs_org_id_code",
                table: "MasterRefs",
                columns: new[] { "org_id", "code" },
                unique: true);
        }
    }
}
