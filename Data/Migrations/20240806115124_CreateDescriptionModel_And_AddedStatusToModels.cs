using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoperiaDocumentation.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateDescriptionModel_And_AddedStatusToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Methods");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Folders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DescriptionModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MethodId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescriptionModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DescriptionModel_Methods_MethodId",
                        column: x => x.MethodId,
                        principalTable: "Methods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionModel_MethodId",
                table: "DescriptionModel",
                column: "MethodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DescriptionModel");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Files");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Methods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
