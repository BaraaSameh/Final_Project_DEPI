using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepiFinalProject.Migrations
{
    /// <inheritdoc />
    public partial class cleanflashsale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlashSaleProducts_FlashSales_FlashSaleID",
            //    table: "FlashSaleProducts");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlashSaleProducts_Users_userid",
            //    table: "FlashSaleProducts");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlashSales_Users_userid",
            //    table: "FlashSales");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_UserID",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Products",
                newName: "userid");

            migrationBuilder.RenameIndex(
                name: "IX_Products_UserID",
                table: "Products",
                newName: "IX_Products_userid");

            migrationBuilder.AlterColumn<int>(
                name: "userid",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "FlashSales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "FlashSaleProducts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlashSales_UserID",
                table: "FlashSales",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FlashSaleProducts_UserID",
                table: "FlashSaleProducts",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSaleProducts_FlashSales_FlashSaleID",
                table: "FlashSaleProducts",
                column: "FlashSaleID",
                principalTable: "FlashSales",
                principalColumn: "FlashSaleID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSaleProducts_Users_UserID",
                table: "FlashSaleProducts",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSaleProducts_Users_userid",
                table: "FlashSaleProducts",
                column: "userid",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSales_Users_UserID",
                table: "FlashSales",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSales_Users_userid",
                table: "FlashSales",
                column: "userid",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_userid",
                table: "Products",
                column: "userid",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlashSaleProducts_FlashSales_FlashSaleID",
            //    table: "FlashSaleProducts");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlashSaleProducts_Users_UserID",
            //    table: "FlashSaleProducts");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlashSaleProducts_Users_userid",
            //    table: "FlashSaleProducts");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlashSales_Users_UserID",
            //    table: "FlashSales");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_FlashSales_Users_userid",
            //    table: "FlashSales");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_userid",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_FlashSales_UserID",
                table: "FlashSales");

            migrationBuilder.DropIndex(
                name: "IX_FlashSaleProducts_UserID",
                table: "FlashSaleProducts");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "FlashSales");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "FlashSaleProducts");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "Products",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Products_userid",
                table: "Products",
                newName: "IX_Products_UserID");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSaleProducts_FlashSales_FlashSaleID",
                table: "FlashSaleProducts",
                column: "FlashSaleID",
                principalTable: "FlashSales",
                principalColumn: "FlashSaleID",
                onDelete: ReferentialAction.Cascade);

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
    }
}
