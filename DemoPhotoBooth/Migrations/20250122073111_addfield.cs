using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoPhotoBooth.Migrations
{
    public partial class addfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrintCount",
                table: "PhotoApps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintCount",
                table: "PhotoApps");
        }
    }
}
