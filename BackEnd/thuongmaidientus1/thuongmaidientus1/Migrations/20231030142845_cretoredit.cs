using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thuongmaidientus1.Migrations
{
    public partial class cretoredit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CretorEdit",
                table: "tokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CretorEdit",
                table: "roles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CretorEdit",
                table: "products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CretorEdit",
                table: "productCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CretorEdit",
                table: "orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CretorEdit",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CretorEdit",
                table: "categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CretorEdit",
                table: "accounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CretorEdit",
                table: "tokens");

            migrationBuilder.DropColumn(
                name: "CretorEdit",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "CretorEdit",
                table: "products");

            migrationBuilder.DropColumn(
                name: "CretorEdit",
                table: "productCategories");

            migrationBuilder.DropColumn(
                name: "CretorEdit",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "CretorEdit",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "CretorEdit",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "CretorEdit",
                table: "accounts");
        }
    }
}
