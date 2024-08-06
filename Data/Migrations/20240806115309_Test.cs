using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoperiaDocumentation.Data.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DescriptionModel_Methods_MethodId",
                table: "DescriptionModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DescriptionModel",
                table: "DescriptionModel");

            migrationBuilder.RenameTable(
                name: "DescriptionModel",
                newName: "Descriptions");

            migrationBuilder.RenameIndex(
                name: "IX_DescriptionModel_MethodId",
                table: "Descriptions",
                newName: "IX_Descriptions_MethodId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Descriptions",
                table: "Descriptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Descriptions_Methods_MethodId",
                table: "Descriptions",
                column: "MethodId",
                principalTable: "Methods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Descriptions_Methods_MethodId",
                table: "Descriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Descriptions",
                table: "Descriptions");

            migrationBuilder.RenameTable(
                name: "Descriptions",
                newName: "DescriptionModel");

            migrationBuilder.RenameIndex(
                name: "IX_Descriptions_MethodId",
                table: "DescriptionModel",
                newName: "IX_DescriptionModel_MethodId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DescriptionModel",
                table: "DescriptionModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DescriptionModel_Methods_MethodId",
                table: "DescriptionModel",
                column: "MethodId",
                principalTable: "Methods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
