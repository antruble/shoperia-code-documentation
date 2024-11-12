using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoperiaDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class AddedBackTheFileModelFromFielModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fields_Files_FileModelId",
                table: "Fields");

            migrationBuilder.DropIndex(
                name: "IX_Fields_FileModelId",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "FileModelId",
                table: "Fields");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_FileId",
                table: "Fields",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fields_Files_FileId",
                table: "Fields",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fields_Files_FileId",
                table: "Fields");

            migrationBuilder.DropIndex(
                name: "IX_Fields_FileId",
                table: "Fields");

            migrationBuilder.AddColumn<int>(
                name: "FileModelId",
                table: "Fields",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fields_FileModelId",
                table: "Fields",
                column: "FileModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fields_Files_FileModelId",
                table: "Fields",
                column: "FileModelId",
                principalTable: "Files",
                principalColumn: "Id");
        }
    }
}
