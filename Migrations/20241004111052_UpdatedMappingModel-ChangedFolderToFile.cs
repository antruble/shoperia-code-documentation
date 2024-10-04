using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoperiaDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedMappingModelChangedFolderToFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Mappings_MappingId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Mappings_Folders_ParentId",
                table: "Mappings");

            migrationBuilder.DropIndex(
                name: "IX_Mappings_ParentId",
                table: "Mappings");

            migrationBuilder.DropIndex(
                name: "IX_Files_MappingId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "MappingId",
                table: "Files");

            migrationBuilder.CreateIndex(
                name: "IX_Mappings_ParentId",
                table: "Mappings",
                column: "ParentId",
                unique: true,
                filter: "[ParentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Mappings_Files_ParentId",
                table: "Mappings",
                column: "ParentId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mappings_Files_ParentId",
                table: "Mappings");

            migrationBuilder.DropIndex(
                name: "IX_Mappings_ParentId",
                table: "Mappings");

            migrationBuilder.AddColumn<int>(
                name: "MappingId",
                table: "Files",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mappings_ParentId",
                table: "Mappings",
                column: "ParentId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Mappings_Folders_ParentId",
                table: "Mappings",
                column: "ParentId",
                principalTable: "Folders",
                principalColumn: "Id");
        }
    }
}
