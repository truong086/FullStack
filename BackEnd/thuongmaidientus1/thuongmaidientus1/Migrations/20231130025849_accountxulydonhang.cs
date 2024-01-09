using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thuongmaidientus1.Migrations
{
    public partial class accountxulydonhang : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Accountid",
                table: "xulydonhangs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_xulydonhangs_Accountid",
                table: "xulydonhangs",
                column: "Accountid");

            migrationBuilder.AddForeignKey(
                name: "FK_xulydonhangs_accounts_Accountid",
                table: "xulydonhangs",
                column: "Accountid",
                principalTable: "accounts",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_xulydonhangs_accounts_Accountid",
                table: "xulydonhangs");

            migrationBuilder.DropIndex(
                name: "IX_xulydonhangs_Accountid",
                table: "xulydonhangs");

            migrationBuilder.DropColumn(
                name: "Accountid",
                table: "xulydonhangs");
        }
    }
}
