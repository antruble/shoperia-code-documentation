using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoperiaDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedFileModelForMapping2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Files");

            migrationBuilder.AddColumn<int>(
                name: "MappingId",
                table: "Files",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_MappingId",
                table: "Files",
                column: "MappingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Mappings_MappingId",
                table: "Files",
                column: "MappingId",
                principalTable: "Mappings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Mappings_MappingId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_MappingId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "MappingId",
                table: "Files");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Files",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
