using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoPhotoBooth.Migrations
{
    public partial class addLayoutTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LayoutAppId",
                table: "SvgInfors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LayoutApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LayoutImage = table.Column<string>(type: "TEXT", nullable: false),
                    BackgroudImage = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Width = table.Column<decimal>(type: "TEXT", nullable: false),
                    Height = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayoutApp", x => x.Id);
                });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SvgInfors_LayoutApp_LayoutAppId",
                table: "SvgInfors");

            migrationBuilder.DropTable(
                name: "LayoutApp");

            migrationBuilder.DropIndex(
                name: "IX_SvgInfors_LayoutAppId",
                table: "SvgInfors");

            migrationBuilder.DropColumn(
                name: "LayoutAppId",
                table: "SvgInfors");
        }
    }
}
