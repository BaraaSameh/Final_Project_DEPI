using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepiFinalProject.Migrations
{
    /// <inheritdoc />
    public partial class flash_product_mapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "userid",
                table: "FlashSales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "userid",
                table: "FlashSaleProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Products_UserID",
                table: "Products",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FlashSales_userid",
                table: "FlashSales",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_FlashSaleProducts_userid",
                table: "FlashSaleProducts",
                column: "userid");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSaleProducts_Users_userid",
                table: "FlashSaleProducts",
                column: "userid",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSales_Users_userid",
                table: "FlashSales",
                column: "userid",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_UserID",
                table: "Products",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlashSaleProducts_Users_userid",
                table: "FlashSaleProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_FlashSales_Users_userid",
                table: "FlashSales");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_UserID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_UserID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_FlashSales_userid",
                table: "FlashSales");

            migrationBuilder.DropIndex(
                name: "IX_FlashSaleProducts_userid",
                table: "FlashSaleProducts");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "userid",
                table: "FlashSales");

            migrationBuilder.DropColumn(
                name: "userid",
                table: "FlashSaleProducts");
        }
    }
}
