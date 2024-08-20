using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoperiaDocumentation.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedFileMethodConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Methods_Files_FileModelId",
                table: "Methods");

            migrationBuilder.DropIndex(
                name: "IX_Methods_FileModelId",
                table: "Methods");

            migrationBuilder.DropColumn(
                name: "FileModelId",
                table: "Methods");

            migrationBuilder.CreateIndex(
                name: "IX_Methods_FileId",
                table: "Methods",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Methods_Files_FileId",
                table: "Methods",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Methods_Files_FileId",
                table: "Methods");

            migrationBuilder.DropIndex(
                name: "IX_Methods_FileId",
                table: "Methods");

            migrationBuilder.AddColumn<int>(
                name: "FileModelId",
                table: "Methods",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Methods_FileModelId",
                table: "Methods",
                column: "FileModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Methods_Files_FileModelId",
                table: "Methods",
                column: "FileModelId",
                principalTable: "Files",
                principalColumn: "Id");
        }
    }
}
