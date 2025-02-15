using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoPhotoBooth.Migrations
{
    public partial class ModifyLayoutTable_AddFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "LayoutApp",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QRLink",
                table: "LayoutApp",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SVGMappingName",
                table: "LayoutApp",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "LayoutApp");

            migrationBuilder.DropColumn(
                name: "QRLink",
                table: "LayoutApp");

            migrationBuilder.DropColumn(
                name: "SVGMappingName",
                table: "LayoutApp");
        }
    }
}
