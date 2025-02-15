using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoPhotoBooth.Migrations
{
    public partial class addtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintCount",
                table: "PhotoApps");

            migrationBuilder.CreateTable(
                name: "CommonDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PrinterId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrintCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommonDatas", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommonDatas");

            migrationBuilder.AddColumn<int>(
                name: "PrintCount",
                table: "PhotoApps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
