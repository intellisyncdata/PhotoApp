using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoPhotoBooth.Migrations
{
    public partial class addSvgtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SvgInfors",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SvgInfors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SvgRectTags",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SvgInforId = table.Column<uint>(type: "INTEGER", nullable: false),
                    No = table.Column<uint>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    X = table.Column<decimal>(type: "TEXT", nullable: false),
                    Y = table.Column<decimal>(type: "TEXT", nullable: false),
                    Width = table.Column<decimal>(type: "TEXT", nullable: false),
                    Height = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SvgRectTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SvgRectTags_SvgInfors_SvgInforId",
                        column: x => x.SvgInforId,
                        principalTable: "SvgInfors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SvgRectTags_SvgInforId",
                table: "SvgRectTags",
                column: "SvgInforId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SvgRectTags");

            migrationBuilder.DropTable(
                name: "SvgInfors");
        }
    }
}
