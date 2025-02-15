using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoPhotoBooth.Migrations
{
    public partial class updateSvgtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsQRRect",
                table: "SvgRectTags",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsQRRect",
                table: "SvgRectTags");
        }
    }
}
