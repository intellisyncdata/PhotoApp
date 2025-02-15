using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoPhotoBooth.Migrations
{
    public partial class updatesvginfotable_5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ActualHeight",
                table: "SvgInfors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActualWidth",
                table: "SvgInfors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualHeight",
                table: "SvgInfors");

            migrationBuilder.DropColumn(
                name: "ActualWidth",
                table: "SvgInfors");
        }
    }
}
