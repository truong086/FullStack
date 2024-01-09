using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thuongmaidientus1.Migrations
{
    public partial class shopproduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Shopsid",
                table: "products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_Shopsid",
                table: "products",
                column: "Shopsid");

            migrationBuilder.AddForeignKey(
                name: "FK_products_shops_Shopsid",
                table: "products",
                column: "Shopsid",
                principalTable: "shops",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_shops_Shopsid",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_Shopsid",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Shopsid",
                table: "products");
        }
    }
}
