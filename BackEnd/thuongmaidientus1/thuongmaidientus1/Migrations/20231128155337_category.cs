using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thuongmaidientus1.Migrations
{
    public partial class category : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Categorysid",
                table: "products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_Categorysid",
                table: "products",
                column: "Categorysid");

            migrationBuilder.AddForeignKey(
                name: "FK_products_categories_Categorysid",
                table: "products",
                column: "Categorysid",
                principalTable: "categories",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_categories_Categorysid",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_Categorysid",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Categorysid",
                table: "products");
        }
    }
}
