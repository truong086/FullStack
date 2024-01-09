using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thuongmaidientus1.Migrations
{
    public partial class shopaccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "accountid",
                table: "shops",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_shops_accountid",
                table: "shops",
                column: "accountid");

            migrationBuilder.AddForeignKey(
                name: "FK_shops_accounts_accountid",
                table: "shops",
                column: "accountid",
                principalTable: "accounts",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_shops_accounts_accountid",
                table: "shops");

            migrationBuilder.DropIndex(
                name: "IX_shops_accountid",
                table: "shops");

            migrationBuilder.DropColumn(
                name: "accountid",
                table: "shops");
        }
    }
}
