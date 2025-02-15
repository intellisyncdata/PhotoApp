using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoPhotoBooth.Migrations
{
    public partial class ModifyLayoutTable_AddFields2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFolderPath",
                table: "LayoutApp",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFolderPath",
                table: "LayoutApp");
        }
    }
}
