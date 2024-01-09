using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thuongmaidientus1.Migrations
{
    public partial class vanchuyenxulydonhang : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "vanchuyenid",
                table: "xulydonhangs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_xulydonhangs_vanchuyenid",
                table: "xulydonhangs",
                column: "vanchuyenid");

            migrationBuilder.AddForeignKey(
                name: "FK_xulydonhangs_vanchuyens_vanchuyenid",
                table: "xulydonhangs",
                column: "vanchuyenid",
                principalTable: "vanchuyens",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_xulydonhangs_vanchuyens_vanchuyenid",
                table: "xulydonhangs");

            migrationBuilder.DropIndex(
                name: "IX_xulydonhangs_vanchuyenid",
                table: "xulydonhangs");

            migrationBuilder.DropColumn(
                name: "vanchuyenid",
                table: "xulydonhangs");
        }
    }
}
