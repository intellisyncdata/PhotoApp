using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoPhotoBooth.Migrations
{
    public partial class updateLayoutTable_delfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SvgInfors_LayoutApp_LayoutAppId",
                table: "SvgInfors");

            migrationBuilder.DropIndex(
                name: "IX_SvgInfors_LayoutAppId",
                table: "SvgInfors");

            migrationBuilder.DropColumn(
                name: "LayoutAppId",
                table: "SvgInfors");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LayoutAppId",
                table: "SvgInfors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SvgInfors_LayoutAppId",
                table: "SvgInfors",
                column: "LayoutAppId");

            migrationBuilder.AddForeignKey(
                name: "FK_SvgInfors_LayoutApp_LayoutAppId",
                table: "SvgInfors",
                column: "LayoutAppId",
                principalTable: "LayoutApp",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
